using FluentValidation;
using GestorAtividades.Models;

namespace GestorAtividades.Validators
{
    public class AtividadeValidator : AbstractValidator<Atividade>
    {
        public AtividadeValidator()
        {
            // TÍTULO
            RuleFor(a => a.Titulo)
                .NotEmpty().WithMessage("O título é obrigatório.")
                .MinimumLength(5).WithMessage("O título deve ter no mínimo 5 caracteres.")
                .MaximumLength(200).WithMessage("O título deve ter no máximo 200 caracteres.");

            // DATA INÍCIO >= DATA REGISTRO
            RuleFor(a => a.DataInicio)
                .Must((atividade, dataInicio) =>
                    dataInicio >= atividade.DataRegistro
                )
                .WithMessage("A data de início não pode ser anterior à data de registro.");

            // DATA CONCLUSÃO >= DATA INÍCIO
            RuleFor(a => a.DataConclusao)
                .Must((atividade, dataConclusao) =>
                    dataConclusao >= atividade.DataInicio
                )
                .WithMessage("A data de conclusão não pode ser anterior à data de início.");
        }
    }
}
