using GestorAtividades.Exceptions;
using GestorAtividades.Service;
using GestorAtividades.Tests.Infra;
using GestorAtividades.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace GestorAtividades.Tests.Servicos;

[Collection(BancoCollection.Nome)]
public class UsuarioServiceTests(BancoFixture banco)
{
    [Fact]
    public async Task Registrar_normaliza_o_email_e_guarda_so_o_hash()
    {
        await using var ctx = banco.CriarContexto();
        var email = Dados.EmailUnico("Maiusculas").ToUpperInvariant();

        await new UsuarioService(ctx).RegistrarAsync(new RegistrarUsuarioViewModel { Nome = " Ana Souza ", Email = $"  {email} ", Senha = "abc12!" });

        var salvo = await ctx.Usuarios.AsNoTracking().SingleAsync(u => u.Email == email.ToLowerInvariant());
        Assert.Equal("Ana Souza", salvo.Nome);
        Assert.NotEqual("abc12!", salvo.SenhaHash);
        Assert.True(BCrypt.Net.BCrypt.Verify("abc12!", salvo.SenhaHash));
    }

    [Fact]
    public async Task Registrar_recusa_email_ja_usado_mesmo_com_outra_caixa()
    {
        await using var ctx = banco.CriarContexto();
        var existente = await Dados.CriarUsuarioAsync(ctx);

        var erro = await Assert.ThrowsAsync<RegraNegocioException>(() => new UsuarioService(ctx).RegistrarAsync(
            new RegistrarUsuarioViewModel { Nome = "Outra", Email = existente.Email.ToUpperInvariant(), Senha = "abc12!" }));

        Assert.Equal("Este email já está registrado.", erro.Message);
    }

    [Fact]
    public async Task Autenticar_aceita_qualquer_caixa_e_recusa_senha_errada()
    {
        await using var ctx = banco.CriarContexto();
        var usuario = await Dados.CriarUsuarioAsync(ctx, senha: "certa12!");
        var servico = new UsuarioService(ctx);

        Assert.NotNull(await servico.AutenticarAsync($" {usuario.Email.ToUpperInvariant()} ", "certa12!"));
        Assert.Null(await servico.AutenticarAsync(usuario.Email, "errada12!"));
        Assert.Null(await servico.AutenticarAsync(Dados.EmailUnico("inexistente"), "certa12!"));
        Assert.Null(await servico.AutenticarAsync("", ""));
    }

    [Fact]
    public async Task Autenticar_encontra_conta_antiga_gravada_com_maiusculas()
    {
        await using var ctx = banco.CriarContexto();
        var legado = await Dados.CriarUsuarioAsync(ctx, email: "Conta.Antiga." + Guid.NewGuid().ToString("N") + "@Teste.com");

        Assert.NotNull(await new UsuarioService(ctx).AutenticarAsync(legado.Email.ToLowerInvariant(), "abc12!"));
    }

    [Fact]
    public async Task Editar_recusa_email_de_outra_conta()
    {
        await using var ctx = banco.CriarContexto();
        var ana = await Dados.CriarUsuarioAsync(ctx);
        var bia = await Dados.CriarUsuarioAsync(ctx);

        var erro = await Assert.ThrowsAsync<RegraNegocioException>(() => new UsuarioService(ctx).EditarAsync(ana.Id,
            new EditarPerfilViewModel { Nome = "Ana", Email = bia.Email.ToUpperInvariant() }));

        Assert.Equal("Este email já está em uso por outra conta.", erro.Message);
    }

    [Fact]
    public async Task Editar_mantendo_o_proprio_email_e_sem_senha_preserva_o_hash()
    {
        await using var ctx = banco.CriarContexto();
        var ana = await Dados.CriarUsuarioAsync(ctx);
        var hashAntes = ana.SenhaHash;

        var atualizado = await new UsuarioService(ctx).EditarAsync(ana.Id,
            new EditarPerfilViewModel { Nome = "Ana Nova", Email = ana.Email.ToUpperInvariant() });

        Assert.NotNull(atualizado);
        Assert.Equal("Ana Nova", atualizado.Nome);
        Assert.Equal(ana.Email.ToLowerInvariant(), atualizado.Email);
        Assert.Equal(hashAntes, atualizado.SenhaHash);
    }

    [Fact]
    public async Task Editar_com_nova_senha_troca_o_login()
    {
        await using var ctx = banco.CriarContexto();
        var ana = await Dados.CriarUsuarioAsync(ctx, senha: "antiga1!");
        var servico = new UsuarioService(ctx);

        await servico.EditarAsync(ana.Id, new EditarPerfilViewModel { Nome = "Ana", Email = ana.Email, Senha = "nova12!", ConfirmarSenha = "nova12!" });

        Assert.Null(await servico.AutenticarAsync(ana.Email, "antiga1!"));
        Assert.NotNull(await servico.AutenticarAsync(ana.Email, "nova12!"));
    }

    [Fact]
    public async Task Perfil_de_usuario_inexistente_e_nulo()
    {
        await using var ctx = banco.CriarContexto();
        var servico = new UsuarioService(ctx);

        Assert.Null(await servico.ObterPerfilAsync(int.MaxValue));
        Assert.Null(await servico.EditarAsync(int.MaxValue, new EditarPerfilViewModel { Nome = "X", Email = "x@x.com" }));
    }
}
