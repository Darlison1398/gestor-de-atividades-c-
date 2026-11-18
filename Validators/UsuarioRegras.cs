using FluentValidation;

namespace GestorAtividades.Validators;

/// <summary>Regras de nome, email e senha compartilhadas entre cadastro e edição de perfil.</summary>
public static class UsuarioRegras
{
    public static IRuleBuilderOptions<T, string> NomeValido<T>(this IRuleBuilder<T, string> regra) =>
        regra
            .NotEmpty().WithMessage("O nome é obrigatório.")
            .Length(2, 100).WithMessage("O nome deve ter entre 2 e 100 caracteres.")
            .Matches(@"^[A-Za-zÀ-ÖØ-öø-ÿ' \-]+$").WithMessage("O nome deve conter apenas letras, espaços, hífens ou apóstrofos.");

    public static IRuleBuilderOptions<T, string> EmailValido<T>(this IRuleBuilder<T, string> regra) =>
        regra
            .NotEmpty().WithMessage("O email é obrigatório.")
            .MaximumLength(255).WithMessage("O email deve ter no máximo 255 caracteres.")
            .EmailAddress().WithMessage("Informe um email válido.");

    public static IRuleBuilderOptions<T, string?> SenhaValida<T>(this IRuleBuilder<T, string?> regra) =>
        regra
            .Length(5, 100).WithMessage("A senha deve ter entre 5 e 100 caracteres.")
            .Matches(@"^(?=.*[A-Za-z])(?=.*\d)(?=.*[^A-Za-z0-9]).{5,}$")
            .WithMessage("A senha deve conter pelo menos uma letra, um número e um símbolo especial.");
}
