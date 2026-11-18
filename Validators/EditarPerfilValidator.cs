using FluentValidation;
using GestorAtividades.ViewModels;

namespace GestorAtividades.Validators;

public class EditarPerfilValidator : AbstractValidator<EditarPerfilViewModel>
{
    public EditarPerfilValidator()
    {
        RuleFor(u => u.Nome).NomeValido();
        RuleFor(u => u.Email).EmailValido();

        // Senha em branco = manter a atual. Se informada, segue as mesmas regras do cadastro.
        RuleFor(u => u.Senha)
            .SenhaValida()
            .When(u => !string.IsNullOrEmpty(u.Senha));

        RuleFor(u => u.ConfirmarSenha)
            .Equal(u => u.Senha).WithMessage("As senhas não conferem.")
            .When(u => !string.IsNullOrEmpty(u.Senha));
    }
}
