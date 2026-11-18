using FluentValidation;
using GestorAtividades.Models;

namespace GestorAtividades.Validators
{
    /// <summary>
    /// As datas chegam do formulário no horário de Brasília, então toda comparação com "agora"
    /// usa TimeHelper.NowInBrazil(). As regras de "não pode ser no passado" valem só na criação
    /// (Id == 0), para que atividades antigas continuem editáveis (ex.: marcar como Concluída).
    /// </summary>
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
                .LessThan(a => a.DataConclusao)
                .WithMessage("A data de início deve ser anterior à data de conclusão.");

            RuleFor(a => a.DataConclusao)
                .GreaterThan(a => a.DataInicio)
                .WithMessage("A data de conclusão deve ser posterior à data de início.");

            // Regras separadas: em FluentValidation, When() se aplica a todas as regras anteriores da mesma cadeia.
            RuleFor(a => a.DataInicio)
                .Must(inicio => inicio.Date >= TimeHelper.NowInBrazil().Date)
                .When(a => a.Id == 0)
                .WithMessage("A data de início não pode ser anterior a hoje.");

            RuleFor(a => a.DataConclusao)
                .Must(conclusao => conclusao >= TimeHelper.NowInBrazil())
                .When(a => a.Id == 0)
                .WithMessage("A data de conclusão não pode ser no passado.");
        }
    }
}
