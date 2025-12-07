using FluentValidation;
using GestorAtividades.Models;

namespace GestorAtividades.Validators
{
    public class AtividadeValidator : AbstractValidator<Atividade>
    {
        public AtividadeValidator()
        {
             RuleFor(a => a.Titulo)
                .NotEmpty().WithMessage("O título é obrigatório.")
                .MinimumLength(5).WithMessage("O título deve ter no mínimo 5 caracteres.")
                .MaximumLength(200).WithMessage("O título deve ter no máximo 200 caracteres.");

            RuleFor(a => a.Descricao)
                .NotEmpty().WithMessage("A descrição é obrigatória.")
                .MaximumLength(1000).WithMessage("A descrição deve ter no máximo 1000 caracteres.");

            RuleFor(a => a.DataInicio)
                .GreaterThanOrEqualTo(a => a.DataRegistro)
                .WithMessage("A data/hora de início não pode ser anterior à data de registro.")
                .LessThan(a => a.DataConclusao)
                .WithMessage("A data de início deve ser anterior à data de conclusão.");

            RuleFor(a => a.DataConclusao)
                .GreaterThan(a => a.DataInicio)
                .WithMessage("A data de conclusão deve ser posterior à data de início.")
                .GreaterThanOrEqualTo(DateTime.Now)
                .WithMessage("A data de conclusão não pode ser no passado.");
        }
    }
}
