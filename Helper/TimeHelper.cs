public static class TimeHelper
{
    private static readonly TimeZoneInfo BrasilTZ = 
        TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");

    public static DateTime NowInBrazil()
    {
        var nowInBrazil = TimeZoneInfo.ConvertTime(DateTime.UtcNow, BrasilTZ);
        return TimeZoneInfo.ConvertTimeToUtc(nowInBrazil, BrasilTZ);
    }

    public static DateTime BrazilToUtc(DateTime date)
    {
        return TimeZoneInfo.ConvertTimeToUtc(date, BrasilTZ);
    }

    public static DateTime UtcToBrazil(DateTime date)
    {
        return TimeZoneInfo.ConvertTimeFromUtc(date, BrasilTZ);
    }
}
