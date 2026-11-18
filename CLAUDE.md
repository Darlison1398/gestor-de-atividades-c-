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

There is no test project and no linter configured.

The connection string `DefaultConnection` lives in `appsettings.json` and points to a local PostgreSQL (`localhost:5432`, database `dbgetoratividadeslocal`). A comment in `Program.cs` says "MySQL" but the provider is Npgsql.

`bin/` and `obj/` were committed in the past and are now ignored via `.gitignore`. Credentials in `appsettings.json` are intentionally kept in the tracked file (owner's choice); don't move them to env vars/user secrets unless asked.

## Architecture

Flow is Controller → Service → `ApplicationDbContext` (no repository layer). Services (`Service/AtividadeService.cs`, `Service/UsuarioService.cs`) are registered as scoped in `Program.cs` and signal failures by throwing plain `Exception` with a user-facing message; controllers catch it, put the message in `TempData["ErrorMessage"]` (or `TempData["SuccessMessage"]` on success) and redirect. `Views/Shared/_Messages.cshtml` renders these.

**Auth:** cookie scheme named `"CookieAuth"` (login path `/Auth/Login`, 2h expiry). On login, claims `NameIdentifier` (user id), `Name`, `Email` are issued. `Controllers/BaseController.CS` exposes `UserId` parsed from `NameIdentifier` (0 if missing); controllers check `UserId == 0` → `Unauthorized()`. `AtividadeController` doesn't inherit `BaseController` and parses the claim itself.

**Ownership:** every activity query in `AtividadeService` filters by both `Id` and `UserId`, so users only see/modify their own activities. Preserve this when adding queries.

**Controllers / routes** (default route `{controller=Home}/{action=Index}/{id?}`):
- `HomeController`: public landing page; `Views/Home/Index.cshtml` hosts the login, register and forgot-password forms as partials (`_LoginForm`, `_RegisterForm`, `_ForgotPasswordForm`) which post to `AuthController`.
- `AuthController`: login/logout, register (`CreateUsuario`), profile (`Perfil`, `EditarUsuario`, `UpdateUsuario` using `Dto/UsuarioDto`). `ForgotPassword` is a stub.
- `PagesController` (`[Authorize]`): the authenticated area — activity list (`Main`), detail, edit/update, delete, and the create form view.
- `AtividadeController`: only `CriarAtividade` (POST from `RegistrarAtividade` view). Note activity creation lives here while the rest of the activity CRUD lives in `PagesController`.

**Validation:** two mechanisms coexist — `Usuario` uses DataAnnotations; `Atividade` is validated by `Validators/AtividadeValidator.cs` (FluentValidation, auto-registered from the assembly via `AddFluentValidation`). `Usuario.Senha` is `[NotMapped]`; only `SenhaHash` is persisted.

**Dates/time zones:** `Helper/TimeHelper.cs` (global namespace) handles `America/Sao_Paulo`. Form inputs are treated as Brazil local time and converted with `TimeHelper.BrazilToUtc` before saving; DB columns are `timestamp with time zone` (configured in `ApplicationDbContext.OnModelCreating`), so Npgsql requires UTC `DateTime`s. Read methods in the services (`ListarPorUsuarioAsync`, `BuscarPorIdAsync`, `GetUsuarioByIdAsync`) load with `AsNoTracking` and already return dates converted to Brazil time, so views and edit forms use them as-is. `TimeHelper.NowInBrazil()` returns Brazil local time (for comparing with form input in validators) and must never be persisted; use `DateTime.UtcNow` for stored timestamps.

**HTML descriptions:** `Descricao` holds HTML from the Quill editor. It is sanitized with `Helper/DescricaoSanitizer` (HtmlSanitizer) when saved and again when rendered with `Html.Raw`. In scripts, pass it via `@Json.Serialize(...)`, never inside a JS string literal.

**State-changing actions** (`Logout`, `DeletarAtividade`, `UpdateAtividade`, `UpdateUsuario`, etc.) are `[HttpPost]` + `[ValidateAntiForgeryToken]`; links that trigger them must be forms.

**Status:** `Enum/StatusAtividade.cs` (global namespace, `[Display]` names) is stored as a string via `HasConversion<string>()`. `Helper/GetColor.cs` provides the `GetColor()` extension that maps a status to Bootstrap badge classes used in views.
