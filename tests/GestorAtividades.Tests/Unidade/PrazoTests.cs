using GestorAtividades.Helper;

namespace GestorAtividades.Tests.Unidade;

public class PrazoTests
{
    private static readonly DateOnly Hoje = new(2026, 11, 17);

    [Theory]
    [InlineData(-5, SituacaoPrazo.Atrasada, "Atrasada há 5 dias", "prazo-atrasada")]
    [InlineData(-1, SituacaoPrazo.Atrasada, "Atrasada há 1 dia", "prazo-atrasada")]
    [InlineData(0, SituacaoPrazo.VenceHoje, "Vence hoje", "prazo-hoje")]
    [InlineData(1, SituacaoPrazo.VenceEmBreve, "Vence amanhã", "prazo-breve")]
    [InlineData(3, SituacaoPrazo.VenceEmBreve, "Vence em 3 dias", "prazo-breve")]
    [InlineData(7, SituacaoPrazo.VenceEmBreve, "Vence em 7 dias", "prazo-breve")]
    [InlineData(8, SituacaoPrazo.NoPrazo, "Vence em 25/11", "prazo-normal")]
    public void Descreve_o_prazo_de_atividades_abertas(int dias, SituacaoPrazo situacao, string texto, string classe)
    {
        var prazo = Prazo.Calcular(Hoje.AddDays(dias), StatusAtividade.EmDesenvolvimento, Hoje);

        Assert.Equal(situacao, prazo.Situacao);
        Assert.Equal(texto, prazo.Texto);
        Assert.Equal(classe, prazo.Classe);
        Assert.False(string.IsNullOrEmpty(prazo.Icone)); // cor nunca é a única pista
    }

    [Fact]
    public void Prazo_em_outro_ano_mostra_o_ano()
    {
        var prazo = Prazo.Calcular(new DateOnly(2027, 1, 10), StatusAtividade.Pendente, Hoje);

        Assert.Equal("Vence em 10/01/2027", prazo.Texto);
    }

    [Theory]
    [InlineData(-30)]
    [InlineData(0)]
    [InlineData(30)]
    public void Concluida_ignora_a_data(int dias)
    {
        var prazo = Prazo.Calcular(Hoje.AddDays(dias), StatusAtividade.Concluido, Hoje);

        Assert.Equal(SituacaoPrazo.Concluida, prazo.Situacao);
        Assert.Equal("Concluída", prazo.Texto);
    }
}
