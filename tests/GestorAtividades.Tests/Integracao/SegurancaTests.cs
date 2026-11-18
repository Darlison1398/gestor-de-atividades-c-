using System.Net;
using System.Text.RegularExpressions;
using GestorAtividades.Tests.Infra;

namespace GestorAtividades.Tests.Integracao;

/// <summary>Requisições HTTP reais contra a aplicação completa (rotas, filtros, antiforgery, autorização).</summary>
[Collection(BancoCollection.Nome)]
public partial class SegurancaTests(BancoFixture banco) : IAsyncLifetime
{
    private AppFactory _app = null!;

    public Task InitializeAsync()
    {
        _app = new AppFactory(banco);
        return Task.CompletedTask;
    }

    public async Task DisposeAsync() => await _app.DisposeAsync();

    [GeneratedRegex(@"/Atividades/Detalhes/(\d+)")]
    private static partial Regex RegexDetalhes();

    private static async Task<int> CriarAtividadeAsync(HttpClient cliente, string titulo)
    {
        var hoje = TimeHelper.HojeNoBrasil();
        var token = await cliente.TokenDaPaginaAsync("/Atividades/Criar");
        var resposta = await cliente.PostFormAsync("/Atividades/Criar", new()
        {
            ["Titulo"] = titulo,
            ["Descricao"] = "<p>Descrição</p>",
            ["DataInicio"] = hoje.ToString("yyyy-MM-dd"),
            ["DataConclusao"] = hoje.AddDays(2).ToString("yyyy-MM-dd")
        }, token);
        Assert.Equal(HttpStatusCode.Redirect, resposta.StatusCode);

        var lista = await cliente.GetStringAsync("/Atividades?visao=lista&ordem=recentes");
        return int.Parse(RegexDetalhes().Match(lista).Groups[1].Value);
    }

    [Theory]
    [InlineData("/Atividades")]
    [InlineData("/Atividades/Criar")]
    [InlineData("/Perfil")]
    public async Task Paginas_protegidas_exigem_login(string url)
    {
        var resposta = await _app.NovoCliente().GetAsync(url);

        Assert.Equal(HttpStatusCode.Redirect, resposta.StatusCode);
        Assert.StartsWith("http://localhost/Auth/Login?ReturnUrl=", resposta.Headers.Location?.ToString());
    }

    [Fact]
    public async Task Post_sem_token_antiforgery_e_recusado()
    {
        var cliente = _app.NovoCliente();
        await cliente.CadastrarELogarAsync();

        var resposta = await cliente.PostFormAsync("/Atividades/Criar", new() { ["Titulo"] = "Sem token" });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    [Theory]
    [InlineData("/Auth/Logout")]
    [InlineData("/Atividades/Excluir/1")]
    [InlineData("/Atividades/AlterarStatus/1")]
    public async Task Acoes_que_alteram_dados_nao_aceitam_GET(string url)
    {
        var cliente = _app.NovoCliente();
        await cliente.CadastrarELogarAsync();

        var resposta = await cliente.GetAsync(url);

        Assert.Equal(HttpStatusCode.MethodNotAllowed, resposta.StatusCode);
    }

    [Fact]
    public async Task Fluxo_completo_cadastro_login_criacao_e_listagem()
    {
        var cliente = _app.NovoCliente();
        await cliente.CadastrarELogarAsync();

        var id = await CriarAtividadeAsync(cliente, "Atividade pelo navegador");

        var detalhes = WebUtility.HtmlDecode(await cliente.GetStringAsync($"/Atividades/Detalhes/{id}"));
        Assert.Contains("Atividade pelo navegador", detalhes);
        Assert.Contains("Vence em 2 dias", detalhes);
    }

    [Fact]
    public async Task Outro_usuario_recebe_404_em_todas_as_acoes()
    {
        var dona = _app.NovoCliente();
        await dona.CadastrarELogarAsync();
        var id = await CriarAtividadeAsync(dona, "Atividade privada");

        var intruso = _app.NovoCliente();
        await intruso.CadastrarELogarAsync();
        var token = await intruso.TokenDaPaginaAsync("/Atividades/Criar");

        Assert.Equal(HttpStatusCode.NotFound, (await intruso.GetAsync($"/Atividades/Detalhes/{id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await intruso.GetAsync($"/Atividades/Editar/{id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await intruso.PostFormAsync($"/Atividades/AlterarStatus/{id}",
            new() { ["status"] = "Concluido" }, token, viaFetch: true)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await intruso.PostFormAsync($"/Atividades/Excluir/{id}", new(), token)).StatusCode);

        var intacta = await dona.GetStringAsync($"/Atividades/Detalhes/{id}");
        Assert.Contains("Pendente", intacta);
    }

    [Fact]
    public async Task AlterarStatus_via_fetch_responde_204_e_recusa_status_invalido()
    {
        var cliente = _app.NovoCliente();
        await cliente.CadastrarELogarAsync();
        var id = await CriarAtividadeAsync(cliente, "Mover no quadro");
        var token = await cliente.TokenDaPaginaAsync("/Atividades/Criar");

        var invalido = await cliente.PostFormAsync($"/Atividades/AlterarStatus/{id}", new() { ["status"] = "99" }, token, viaFetch: true);
        Assert.Equal(HttpStatusCode.BadRequest, invalido.StatusCode);
        Assert.Contains("Mover no quadro", await cliente.GetStringAsync("/Atividades?visao=lista&status=Pendente"));

        var valido = await cliente.PostFormAsync($"/Atividades/AlterarStatus/{id}", new() { ["status"] = "EmTeste" }, token, viaFetch: true);
        Assert.Equal(HttpStatusCode.NoContent, valido.StatusCode);
        Assert.Contains("Mover no quadro", await cliente.GetStringAsync("/Atividades?visao=lista&status=EmTeste"));
    }

    [Fact]
    public async Task AlterarStatus_sem_JavaScript_volta_so_para_url_local()
    {
        var cliente = _app.NovoCliente();
        await cliente.CadastrarELogarAsync();
        var id = await CriarAtividadeAsync(cliente, "Sem JavaScript");
        var token = await cliente.TokenDaPaginaAsync("/Atividades/Criar");

        var local = await cliente.PostFormAsync($"/Atividades/AlterarStatus/{id}",
            new() { ["status"] = "EmTeste", ["returnUrl"] = "/Atividades?visao=lista" }, token);
        var externa = await cliente.PostFormAsync($"/Atividades/AlterarStatus/{id}",
            new() { ["status"] = "Pendente", ["returnUrl"] = "https://site-malicioso.com" }, token);

        Assert.Equal("/Atividades?visao=lista", local.Headers.Location?.OriginalString);
        Assert.Equal("/Atividades", externa.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Login_nao_redireciona_para_fora_do_site()
    {
        var cliente = _app.NovoCliente();
        var email = await cliente.CadastrarELogarAsync();
        await cliente.PostFormAsync("/Auth/Logout", new(), await cliente.TokenDaPaginaAsync("/Perfil"));

        var token = await cliente.TokenDaPaginaAsync("/");
        var resposta = await cliente.PostFormAsync("/Auth/Login",
            new() { ["Email"] = email, ["Senha"] = "abc12!", ["ReturnUrl"] = "https://site-malicioso.com" }, token);

        Assert.Equal("/Atividades", resposta.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Pagina_inexistente_mostra_erro_amigavel_em_portugues()
    {
        var resposta = await _app.NovoCliente().GetAsync("/nao-existe");

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
        // O Razor codifica acentos no HTML ("P&#xE1;gina"): decodifica antes de comparar.
        Assert.Contains("Página não encontrada", WebUtility.HtmlDecode(await resposta.Content.ReadAsStringAsync()));
    }
}
