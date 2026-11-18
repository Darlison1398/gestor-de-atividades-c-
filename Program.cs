using GestorAtividades.Data;
using GestorAtividades.Helper;
using GestorAtividades.Service;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<UsuarioService>();
builder.Services.AddScoped<AtividadeService>();

builder.Services.AddControllersWithViews(options =>
{
    // Com Nullable habilitado, o MVC trataria toda string não anulável como [Required] implícito,
    // com mensagem em inglês e antes do FluentValidation. Quem define obrigatoriedade são os validators.
    options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;

    // Mensagens do model binding (valor que nem chega a ser convertido, ex.: data ou status inválidos) em português.
    var mensagens = options.ModelBindingMessageProvider;
    mensagens.SetAttemptedValueIsInvalidAccessor((valor, campo) => $"O valor '{valor}' não é válido para {campo}.");
    mensagens.SetValueIsInvalidAccessor(valor => $"O valor '{valor}' é inválido.");
    mensagens.SetValueMustNotBeNullAccessor(campo => $"O campo {campo} é obrigatório.");
    mensagens.SetMissingBindRequiredValueAccessor(campo => $"O campo {campo} é obrigatório.");
    mensagens.SetMissingKeyOrValueAccessor(() => "Um valor é obrigatório.");
    mensagens.SetUnknownValueIsInvalidAccessor(campo => $"O valor informado não é válido para {campo}.");
    mensagens.SetValueMustBeANumberAccessor(campo => $"O campo {campo} deve ser um número.");
    mensagens.SetNonPropertyAttemptedValueIsInvalidAccessor(valor => $"O valor '{valor}' não é válido.");
    mensagens.SetNonPropertyUnknownValueIsInvalidAccessor(() => "O valor informado não é válido.");
    mensagens.SetNonPropertyValueMustBeANumberAccessor(() => "O campo deve ser um número.");
    mensagens.SetMissingRequestBodyRequiredValueAccessor(() => "O corpo da requisição é obrigatório.");
});

// Validação: validators do assembly (pasta Validators) rodam automaticamente no model binding,
// e as regras simples também geram validação no navegador (jquery.validate).
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();

// Banco de dados PostgreSQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// Autenticação por cookies
builder.Services.AddAuthentication(Autenticacao.Esquema)
    .AddCookie(Autenticacao.Esquema, config =>
    {
        config.LoginPath = "/Auth/Login";
        config.AccessDeniedPath = "/Home/Erro?codigo=403";
        config.ExpireTimeSpan = TimeSpan.FromHours(2);
        config.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Erro");
    app.UseHsts();
}

// 404, 400 (antiforgery), 405... mostram a página de erro amigável.
app.UseStatusCodePagesWithReExecute("/Home/Erro", "?codigo={0}");

app.UseHttpsRedirection();
// UseStaticFiles em vez de MapStaticAssets: com MapStaticAssets, um GET numa ação só-POST
// (ex.: /Auth/Logout) responde 404 em vez de 405 (coberto por SegurancaTests).
app.UseStaticFiles();
app.UseRouting();

// Autenticação antes da autorização
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.Run();
