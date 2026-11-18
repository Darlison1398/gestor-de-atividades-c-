namespace GestorAtividades.Tests.Unidade;

public class TimeHelperTests
{
    private static readonly TimeZoneInfo Brasilia = TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");

    [Fact]
    public void Hoje_e_o_dia_de_Brasilia_e_nao_o_do_servidor()
    {
        var esperado = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, Brasilia));

        Assert.Equal(esperado, TimeHelper.HojeNoBrasil());
    }

    [Fact]
    public void Noite_em_Brasilia_ja_e_o_dia_seguinte_em_UTC()
    {
        var utc = new DateTime(2026, 11, 18, 1, 30, 0, DateTimeKind.Utc); // 22h30 de 17/11 em Brasília

        var local = TimeHelper.UtcToBrazil(utc);

        Assert.Equal(new DateTime(2026, 11, 17, 22, 30, 0), local);
    }

    [Fact]
    public void Conversao_ida_e_volta_preserva_o_instante()
    {
        var utc = new DateTime(2026, 6, 1, 15, 0, 0, DateTimeKind.Utc);

        var volta = TimeHelper.BrazilToUtc(TimeHelper.UtcToBrazil(utc));

        Assert.Equal(utc, volta);
        Assert.Equal(DateTimeKind.Utc, volta.Kind);
    }
}
