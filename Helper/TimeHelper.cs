/// <summary>
/// Convenção de datas do projeto:
/// - No banco (timestamp with time zone) tudo é gravado em UTC.
/// - Formulários e telas trabalham no horário de Brasília.
/// Converta com BrazilToUtc antes de salvar e com UtcToBrazil antes de exibir.
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
