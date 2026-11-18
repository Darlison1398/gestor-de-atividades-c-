using GestorAtividades.ViewModels;

namespace GestorAtividades.Tests.Unidade;

public class FiltroAtividadesTests
{
    [Fact]
    public void ComoRota_troca_um_filtro_e_mantem_os_outros()
    {
        var filtro = new FiltroAtividades { Visao = "lista", Q = "contrato", Ordem = "titulo" };

        var rota = filtro.ComoRota(new { situacao = "atrasadas" });

        Assert.Equal("lista", rota["visao"]);
        Assert.Equal("contrato", rota["q"]);
        Assert.Equal("titulo", rota["ordem"]);
        Assert.Equal("atrasadas", rota["situacao"]);
    }

    [Fact]
    public void ComoRota_remove_valores_vazios()
    {
        var filtro = new FiltroAtividades { Visao = "lista", Status = StatusAtividade.EmTeste };

        var rota = filtro.ComoRota(new { visao = "", status = "" });

        Assert.Empty(rota);
    }

    [Fact]
    public void Status_so_conta_como_filtro_na_lista()
    {
        Assert.False(new FiltroAtividades { Status = StatusAtividade.EmTeste }.TemFiltro);
        Assert.True(new FiltroAtividades { Visao = "lista", Status = StatusAtividade.EmTeste }.TemFiltro);
    }
}
