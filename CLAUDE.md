# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Overview

GestorAtividades is an ASP.NET Core MVC app (target `net10.0`) for per-user task/activity tracking. It uses EF Core with PostgreSQL (Npgsql), cookie authentication, BCrypt password hashing, and FluentValidation. Code, identifiers, UI text and error messages are in Portuguese (pt-BR); keep new code consistent with that.

## Commands

```bash
dotnet build                      # build
dotnet run                        # run (http://localhost:5097; --launch-profile https adds https://localhost:7147)
dotnet watch run                  # run with hot reload

# EF Core migrations (Microsoft.EntityFrameworkCore.Tools/Design are referenced; needs dotnet-ef)
dotnet ef migrations add <Nome>
dotnet ef database update
```

There is no test project and no linter configured. `dotnet ef migrations has-pending-model-changes` confirms an entity change didn't alter the schema.

The connection string `DefaultConnection` lives in `appsettings.json` and points to a local PostgreSQL (`localhost:5432`, database `dbgetoratividadeslocal`).

`bin/` and `obj/` were committed in the past and are now ignored via `.gitignore`. Credentials in `appsettings.json` are intentionally kept in the tracked file (owner's choice); don't move them to env vars/user secrets unless asked.

## Architecture

Flow is Controller → Service → `ApplicationDbContext` (no repository layer). Controllers never touch the DbContext or entities directly: they exchange **ViewModels** (`ViewModels/`) with the services (`Service/AtividadeService.cs`, `Service/UsuarioService.cs`, scoped). Entities in `Models/` are persistence-only.
- Forms bind to `AtividadeFormViewModel`, `RegistrarUsuarioViewModel`, `EditarPerfilViewModel`, `LoginViewModel` (only user-editable fields, so no overposting of `UserId`/`DataRegistro`).
- Screens get read models (`AtividadeViewModel`, `PerfilViewModel`) with dates already in Brazil time and descriptions already sanitized.

**Errors:** services throw `Exceptions/RegraNegocioException` for user-facing rule violations (e.g. duplicate email); controllers catch only that one and add it to `ModelState`. "Not found / not yours" is returned as `null`/`false` and becomes `NotFound()`. Any other exception is a bug: outside Development it goes to `UseExceptionHandler("/Home/Erro")`. Status codes (404, 400 from antiforgery, 405) are re-executed to `HomeController.Erro`, which renders `Views/Shared/Error.cshtml` with a pt-BR message from `ErrorViewModel`. Success messages go in `TempData["SuccessMessage"]`, rendered by `Views/Shared/_Messages.cshtml`.

**Auth:** cookie scheme `Autenticacao.Esquema` (`"CookieAuth"`, 2h sliding). `Helper/Autenticacao.cs` builds the claims principal (`NameIdentifier`, `Name`, `Email`) and normalizes emails. Emails are stored lowercase/trimmed; lookups compare `Email.ToLower()` so legacy mixed-case rows still match. After a profile edit the cookie is re-issued so the displayed name/email stay current. `BaseController.UserId` reads the `NameIdentifier` claim.

**Ownership:** every activity query in `AtividadeService` filters by both `Id` and `UserId`, so users only see/modify their own activities. Preserve this when adding queries.

**Controllers / routes** (default route `{controller=Home}/{action=Index}/{id?}`):
- `HomeController`: public landing page (redirects to `/Atividades` when logged in), `Sobre`, `Autor` (old `/Home/Privacy` URL 301-redirects there), `Erro`. `Views/Home/Index.cshtml` hosts the login, register and forgot-password partials (`_LoginForm`, `_RegisterForm`, `_ForgotPasswordForm`); `ViewBag.ActiveForm` (`register`/`forgot`) tells it which one to reopen after a POST.
- `AuthController`: `Login` (GET is the cookie `LoginPath` and redirects to Home keeping `returnUrl`; POST signs in), `Registrar`, `EsqueciSenha` (stub), `Logout`.
- `AtividadesController` (`[Authorize]`): `Index`, `Detalhes`, `Criar`, `Editar`, `Excluir`, `AlterarStatus`.
  - `Index` binds `ViewModels/FiltroAtividades` from the query string (`visao=lista` (default is the Kanban board), `q`, `status` (list only), `situacao=atrasadas|hoje|semana`, `ordem=recentes|titulo`); unknown values are normalized to the default, never an error. `FiltroAtividades.ComoRota(...)` builds links that change one filter and keep the others. The service applies the filters in SQL (title search with `ILike`, escaping `%`/`_`) and returns a `PainelAtividadesViewModel` with a `ResumoAtividades` (counts over all the user's activities, one grouped query) used by the indicator tiles and the profile.
  - `AlterarStatus` is called by the board via fetch (drag between columns, card menu "Mover para") with `X-Requested-With: fetch` → 204; without JS the menu forms do a normal POST and redirect to a local `returnUrl`. Check `ModelState.IsValid` there: an invalid enum binds as the default (`Pendente`). Create and edit share `Views/Atividades/_AtividadeForm.cshtml` (`EhEdicao` = `Id != 0` toggles the status field and target action).
- `PerfilController` (`[Authorize]`): `Index`, `Editar`.

**Validation:** FluentValidation only, one validator per form ViewModel in `Validators/` (registered with `AddValidatorsFromAssemblyContaining<Program>()`, run automatically via `AddFluentValidationAutoValidation()`, and simple rules also emitted client-side via `AddFluentValidationClientsideAdapters()`). Shared name/email/password rules live in `Validators/UsuarioRegras.cs`. Built-in model-binding messages are translated in `Program.cs`, and `SuppressImplicitRequiredAttributeForNonNullableReferenceTypes` is on: with `Nullable` enabled MVC would otherwise add an implicit `[Required]` (English message, client and server) to every non-nullable `string`, so required-ness must be declared in the validator (`NotEmpty`). Use `[Display(Name = ...)]` on ViewModels for labels.

**Dates/time zones:** `Atividade.DataInicio`/`DataConclusao` are `DateOnly` (PostgreSQL `date`, no time, no time zone; forms use `<input type="date">`). Only `DataRegistro` (activities and users) is an instant: stored as UTC in `timestamp with time zone` (Npgsql requires UTC `DateTime`s), converted with `TimeHelper.UtcToBrazil` when building ViewModels, and displayed date-only. "Today" is always Brazil's date via `TimeHelper.HojeNoBrasil()` (`America/Sao_Paulo`), never the server's; `NowInBrazil()` must never be persisted. In `AtividadeFormValidator`, start and conclusion may be the same day, and the "start not before today" rule applies only on create (`!EhEdicao`) as a separate `RuleFor` chain, because `When()` applies to every earlier rule in a chain. Migration `DatasAtividadeSemHora` converts the old timestamps with `AT TIME ZONE 'America/Sao_Paulo'` in raw SQL; a plain `ALTER COLUMN TYPE date` would use the session time zone (UTC) and shift evening activities to the next day.

**HTML descriptions:** `Descricao` holds HTML from the Quill editor. It is sanitized with `Helper/DescricaoSanitizer` (HtmlSanitizer) when saved and when mapped to ViewModels, so views render `DescricaoHtml` with `Html.Raw`. The editor script (`wwwroot/js/atividade-form.js`) loads the initial HTML from the hidden `Descricao` input and writes it back on submit; never inject the description into a JS string literal.

**State-changing actions** (`Logout`, `Criar`/`Editar`/`Excluir` POSTs, `Perfil/Editar` POST, `Registrar`, `EsqueciSenha`) are `[HttpPost]` + `[ValidateAntiForgeryToken]`; links that trigger them must be forms.

**Deadlines:** `Helper/Prazo.cs` turns `DataConclusao` + status into natural language ("Atrasada há 2 dias", "Vence hoje", "Vence amanhã", "Vence em N dias" up to 7, then the date) with an icon and CSS class (`prazo-*`); `AtividadeViewModel.Prazo` is filled in the service with `TimeHelper.HojeNoBrasil()`. `Prazo.Calcular` takes "today" as a parameter so it can be unit tested.

**Status:** `Enum/StatusAtividade.cs` (global namespace, `[Display]` names) is stored as a string via `HasConversion<string>()`. `Helper/GetColor.cs` provides `GetColor()` (full Bootstrap badge classes, including `badge`) and `GetDisplayName()`.

**Static files** are served with `MapStaticAssets()` / `.WithStaticAssets()` (not `UseStaticFiles`).

## Front-end

Bootstrap 5.3 (local, `wwwroot/lib`) + Font Awesome 6 (CDN, `fa-solid`/`fa-regular` only) + jQuery only for unobtrusive validation. All UI text is pt-BR (`<html lang="pt-BR">`).
- `Views/Shared/_Layout.cshtml`: navbar varies by login state (account dropdown with initials avatar, profile, logout form); light/dark theme via `data-bs-theme` on `<html>`, set by an inline `<head>` script from `localStorage['tema']` or the OS preference; sticky footer via `body.d-flex.flex-column.min-vh-100`. Views render inside `<main class="container">`, so they must not add their own `.container`. Sections: `Styles` (head) and `Scripts` (end of body).
- `wwwroot/css/site.css` holds the shared classes (`card-app`, `cabecalho-pagina`, `estado-vazio`, `lista-info`, `avatar`, Quill dark-mode overrides). Use Bootstrap CSS variables (`--bs-body-bg`, `--bs-secondary-color`...) instead of fixed colors so both themes work.
- `wwwroot/js/site.js` wires global behaviors by markup: theme toggle (`#alternar-tema`), toasts for `TempData` (`_Messages.cshtml`), the shared delete modal (`_ConfirmarExclusao.cshtml`: trigger buttons carry `data-acao` = POST URL and `data-titulo`), and the Home login/register/forgot panels (`[data-auth]` container, `data-painel` buttons, `data-painel-conteudo` panes).
- Activities screen: `Views/Atividades/_AtividadeCard.cshtml` is shared by the board and the list (`ViewData` flags `MostrarStatus`, `Arrastavel`, `ReturnUrl`); `_PainelResumo` (indicator tiles that double as filter shortcuts) and `_Filtros` (GET form; selects auto-submit). `wwwroot/js/atividades.js` uses event delegation on `document` because after each status change it re-fetches the current URL and swaps `#painel-resumo` and `#area-atividades`. Card dropdowns use Popper's `fixed` strategy so they are not clipped by the board's horizontal scroll on small screens.
- Indicators follow a stat-tile pattern (sentence-case label, large semibold tabular number, no chart); status/deadline colors always come with an icon and text, never color alone.
- Quill 1.3.7 is loaded from CDN only on `Atividades/Criar` and `Atividades/Editar` (via the `Styles`/`Scripts` sections). Don't upgrade to Quill 2 without adjusting `DescricaoSanitizer`: Quill 2 marks bullet lists with `data-list` attributes, which HtmlSanitizer strips by default.
