namespace GestorAtividades.ViewModels;

/// <summary>Contagens de todas as atividades do usuário (independe dos filtros), para o painel e o perfil.</summary>
public class ResumoAtividades
{
    public int Total { get; set; }
    public int Abertas => Total - Concluidas;
    public int Concluidas { get; set; }
    public int Atrasadas { get; set; }
    public int VencemHoje { get; set; }

    /// <summary>Vencem de hoje até 7 dias (não concluídas, não atrasadas).</summary>
    public int VencemNaSemana { get; set; }

    public Dictionary<StatusAtividade, int> PorStatus { get; set; } = new();

    public int TaxaConclusao => Total == 0 ? 0 : (int)Math.Round(100.0 * Concluidas / Total);
}
