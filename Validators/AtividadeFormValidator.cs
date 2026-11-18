using FluentValidation;
using GestorAtividades.Helper;
using GestorAtividades.ViewModels;

namespace GestorAtividades.Validators
{
    /// <summary>
    /// As datas não têm hora; "hoje" é o dia em Brasília (TimeHelper.HojeNoBrasil()). Início e conclusão
    /// no mesmo dia são permitidos. As regras de "não pode ser no passado" valem só na criação
    /// (Id == 0), para que atividades antigas continuem editáveis (ex.: marcar como Concluída).
    /// </summary>
    public class AtividadeFormValidator : AbstractValidator<AtividadeFormViewModel>
    {
        public AtividadeFormValidator()
        {
            RuleFor(a => a.Titulo)
                .NotEmpty().WithMessage("O título é obrigatório.")
                .MinimumLength(5).WithMessage("O título deve ter no mínimo 5 caracteres.")
                .MaximumLength(200).WithMessage("O título deve ter no máximo 200 caracteres.");

            // O editor envia HTML: "<p><br></p>" é uma descrição vazia.
            RuleFor(a => a.Descricao)
                .Must(TemTexto).WithMessage("A descrição é obrigatória.")
                .MaximumLength(1000).WithMessage("A descrição deve ter no máximo 1000 caracteres.");

            RuleFor(a => a.Status)
                .IsInEnum().WithMessage("Status inválido.");

            RuleFor(a => a.DataInicio)
                .NotEmpty().WithMessage("Informe a data de início.");

            RuleFor(a => a.DataConclusao)
                .NotEmpty().WithMessage("Informe a data prevista de conclusão.")
                .GreaterThanOrEqualTo(a => a.DataInicio)
                .WithMessage("A data de conclusão não pode ser anterior à data de início.");

            // Regra separada: em FluentValidation, When() se aplica a todas as regras anteriores da mesma cadeia.
            RuleFor(a => a.DataInicio)
                .Must(inicio => inicio >= TimeHelper.HojeNoBrasil())
                .When(a => !a.EhEdicao)
                .WithMessage("A data de início não pode ser anterior a hoje.");
        }

        private static bool TemTexto(string? html) =>
            DescricaoSanitizer.ParaTextoPlano(html).Length > 0;
    }
}
