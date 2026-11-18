using FluentValidation;
using GestorAtividades.ViewModels;

namespace GestorAtividades.Validators;

public class RegistrarUsuarioValidator : AbstractValidator<RegistrarUsuarioViewModel>
{
    public RegistrarUsuarioValidator()
    {
        RuleFor(u => u.Nome).NomeValido();
        RuleFor(u => u.Email).EmailValido();
        RuleFor(u => u.Senha)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("A senha é obrigatória.")
            .SenhaValida();
    }
}
