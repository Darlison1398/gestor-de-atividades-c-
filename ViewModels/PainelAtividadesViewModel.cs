namespace GestorAtividades.ViewModels;

public class PainelAtividadesViewModel
{
    public FiltroAtividades Filtro { get; set; } = new();
    public ResumoAtividades Resumo { get; set; } = new();

    /// <summary>Atividades que passaram pelos filtros, já ordenadas.</summary>
    public List<AtividadeViewModel> Atividades { get; set; } = new();

    public IEnumerable<AtividadeViewModel> DoStatus(StatusAtividade status) => Atividades.Where(a => a.Status == status);
}
