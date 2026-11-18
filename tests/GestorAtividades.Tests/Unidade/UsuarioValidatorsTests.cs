using FluentValidation.TestHelper;
using GestorAtividades.Validators;
using GestorAtividades.ViewModels;

namespace GestorAtividades.Tests.Unidade;

public class UsuarioValidatorsTests
{
    private readonly RegistrarUsuarioValidator _cadastro = new();
    private readonly EditarPerfilValidator _edicao = new();

    [Fact]
    public void Cadastro_valido_passa()
    {
        _cadastro.TestValidate(new RegistrarUsuarioViewModel { Nome = "Ana Souza", Email = "ana@teste.com", Senha = "abc12!" })
            .ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("abcdef", "A senha deve conter pelo menos uma letra, um número e um símbolo especial.")]
    [InlineData("ab1!", "A senha deve ter entre 5 e 100 caracteres.")]
    [InlineData("", "A senha é obrigatória.")]
    public void Cadastro_recusa_senha_fraca_com_uma_mensagem(string senha, string mensagem)
    {
        var resultado = _cadastro.TestValidate(new RegistrarUsuarioViewModel { Nome = "Ana", Email = "ana@teste.com", Senha = senha });

        // Cascade(Stop): só a primeira regra que falhar aparece.
        var erros = resultado.Errors.Where(e => e.PropertyName == nameof(RegistrarUsuarioViewModel.Senha)).ToList();
        Assert.Single(erros);
        Assert.Equal(mensagem, erros[0].ErrorMessage);
    }

    [Theory]
    [InlineData("Ana2")]
    [InlineData("A")]
    [InlineData("")]
    public void Nome_invalido_e_recusado(string nome)
    {
        _cadastro.TestValidate(new RegistrarUsuarioViewModel { Nome = nome, Email = "ana@teste.com", Senha = "abc12!" })
            .ShouldHaveValidationErrorFor(u => u.Nome);
    }

    [Theory]
    [InlineData("")]
    [InlineData("sem-arroba")]
    public void Email_invalido_e_recusado(string email)
    {
        _cadastro.TestValidate(new RegistrarUsuarioViewModel { Nome = "Ana", Email = email, Senha = "abc12!" })
            .ShouldHaveValidationErrorFor(u => u.Email);
    }

    [Fact]
    public void Edicao_com_senha_em_branco_mantem_a_atual()
    {
        _edicao.TestValidate(new EditarPerfilViewModel { Nome = "Ana", Email = "ana@teste.com" })
            .ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Edicao_exige_confirmacao_igual()
    {
        _edicao.TestValidate(new EditarPerfilViewModel { Nome = "Ana", Email = "ana@teste.com", Senha = "nova12!", ConfirmarSenha = "outra12!" })
            .ShouldHaveValidationErrorFor(u => u.ConfirmarSenha).WithErrorMessage("As senhas não conferem.");
    }

    [Fact]
    public void Edicao_aplica_as_regras_de_senha_do_cadastro()
    {
        _edicao.TestValidate(new EditarPerfilViewModel { Nome = "Ana", Email = "ana@teste.com", Senha = "fraca", ConfirmarSenha = "fraca" })
            .ShouldHaveValidationErrorFor(u => u.Senha);
    }
}
