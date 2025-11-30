public static class TimeHelper
{
    public static DateTime NowInBrazil()
    {
        var brasil = TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");
        var nowInBrazil = TimeZoneInfo.ConvertTime(DateTime.UtcNow, brasil);
        
        // ❗ converter para UTC antes de salvar no banco
        return TimeZoneInfo.ConvertTimeToUtc(nowInBrazil, brasil);
    }
}
