namespace GestorAtividades.Helper;

public enum SituacaoPrazo
{
    Concluida,
    Atrasada,
    VenceHoje,
    VenceEmBreve, // amanhã até 7 dias
    NoPrazo
}

/// <summary>Prazo de uma atividade em linguagem natural ("Vence amanhã", "Atrasada há 2 dias").</summary>
public record Prazo(SituacaoPrazo Situacao, string Texto, string Icone, string Classe)
{
    public const int DiasEmBreve = 7;

    /// <param name="hoje">Dia de referência (TimeHelper.HojeNoBrasil()); parâmetro para facilitar testes.</param>
    public static Prazo Calcular(DateOnly conclusao, StatusAtividade status, DateOnly hoje)
    {
        if (status == StatusAtividade.Concluido)
            return new(SituacaoPrazo.Concluida, "Concluída", "fa-solid fa-circle-check", "prazo-concluida");

        var dias = conclusao.DayNumber - hoje.DayNumber;

        return dias switch
        {
            < 0 => new(SituacaoPrazo.Atrasada, dias == -1 ? "Atrasada há 1 dia" : $"Atrasada há {-dias} dias", "fa-solid fa-triangle-exclamation", "prazo-atrasada"),
            0 => new(SituacaoPrazo.VenceHoje, "Vence hoje", "fa-solid fa-hourglass-half", "prazo-hoje"),
            1 => new(SituacaoPrazo.VenceEmBreve, "Vence amanhã", "fa-regular fa-clock", "prazo-breve"),
            <= DiasEmBreve => new(SituacaoPrazo.VenceEmBreve, $"Vence em {dias} dias", "fa-regular fa-clock", "prazo-breve"),
            _ => new(SituacaoPrazo.NoPrazo,
                     $"Vence em {conclusao.ToString(conclusao.Year == hoje.Year ? "dd/MM" : "dd/MM/yyyy")}",
                     "fa-regular fa-calendar", "prazo-normal")
        };
    }
}
