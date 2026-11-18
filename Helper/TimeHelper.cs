/// <summary>
/// Convenção de datas do projeto:
/// - Datas da atividade (início/conclusão) são DateOnly: sem hora e sem fuso.
/// - Instantes (DataRegistro) são gravados em UTC (timestamp with time zone) e exibidos no horário de Brasília
///   com UtcToBrazil. "Hoje" é sempre o dia de Brasília (HojeNoBrasil), não o do servidor.
/// </summary>
public static class TimeHelper
{
    private static readonly TimeZoneInfo BrasilTZ =
        TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");

    /// <summary>Data/hora atual no horário de Brasília (para comparar com valores de formulário). Não gravar no banco.</summary>
    public static DateTime NowInBrazil()
    {
        return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, BrasilTZ);
    }

    /// <summary>Data de hoje no horário de Brasília.</summary>
    public static DateOnly HojeNoBrasil() => DateOnly.FromDateTime(NowInBrazil());

    public static DateTime BrazilToUtc(DateTime date)
    {
        if (date.Kind == DateTimeKind.Utc)
            return date;

        return TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(date, DateTimeKind.Unspecified), BrasilTZ);
    }

    public static DateTime UtcToBrazil(DateTime date)
    {
        return TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(date, DateTimeKind.Utc), BrasilTZ);
    }
}
