using System.Net;
using System.Text.RegularExpressions;
using GestorAtividades.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace GestorAtividades.Tests.Infra;

/// <summary>
/// A aplicação inteira em memória, apontando para o PostgreSQL do contêiner (nunca para o banco
/// de desenvolvimento do appsettings.json: o registro do DbContext é substituído).
/// </summary>
public sealed class AppFactory(BancoFixture banco) : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<ApplicationDbContext>>();
            services.AddDbContext<ApplicationDbContext>(o => o.UseNpgsql(banco.ConnectionString));
        });
    }

    /// <summary>Cliente com cookies e sem seguir redirecionamentos (para conferir status e Location).</summary>
    public HttpClient NovoCliente() => CreateClient(new WebApplicationFactoryClientOptions
    {
        AllowAutoRedirect = false,
        HandleCookies = true
    });
}

public static partial class Http
{
    [GeneratedRegex("name=\"__RequestVerificationToken\" type=\"hidden\" value=\"([^\"]+)\"")]
    private static partial Regex RegexToken();

    public static async Task<string> TokenDaPaginaAsync(this HttpClient cliente, string url)
    {
        var html = await cliente.GetStringAsync(url);
        var token = RegexToken().Match(html);
        Assert.True(token.Success, $"Página {url} não tem token antiforgery.");
        return token.Groups[1].Value;
    }

    public static Task<HttpResponseMessage> PostFormAsync(this HttpClient cliente, string url, Dictionary<string, string> campos,
        string? token = null, bool viaFetch = false)
    {
        if (token != null)
            campos["__RequestVerificationToken"] = token;

        var requisicao = new HttpRequestMessage(HttpMethod.Post, url) { Content = new FormUrlEncodedContent(campos) };
        if (viaFetch)
            requisicao.Headers.Add("X-Requested-With", "fetch");

        return cliente.SendAsync(requisicao);
    }

    /// <summary>Cadastra e faz login de um usuário novo pela própria interface.</summary>
    public static async Task<string> CadastrarELogarAsync(this HttpClient cliente)
    {
        var email = Dados.EmailUnico("http");
        var token = await cliente.TokenDaPaginaAsync("/");

        var cadastro = await cliente.PostFormAsync("/Auth/Registrar",
            new() { ["Nome"] = "Teste Http", ["Email"] = email, ["Senha"] = "abc12!" }, token);
        Assert.Equal(HttpStatusCode.Redirect, cadastro.StatusCode);

        var login = await cliente.PostFormAsync("/Auth/Login",
            new() { ["Email"] = email, ["Senha"] = "abc12!" }, token);
        Assert.Equal(HttpStatusCode.Redirect, login.StatusCode);
        Assert.Equal("/Atividades", login.Headers.Location?.OriginalString);

        return email;
    }
}
