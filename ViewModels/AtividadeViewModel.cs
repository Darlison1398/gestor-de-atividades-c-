using GestorAtividades.Helper;

namespace GestorAtividades.ViewModels;

/// <summary>Atividade pronta para exibição: DataRegistro no horário de Brasília e descrição já sanitizada.</summary>
public class AtividadeViewModel
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string DescricaoHtml { get; set; } = string.Empty;

    /// <summary>Trecho da descrição em texto puro, para os cards da lista.</summary>
    public string Resumo { get; set; } = string.Empty;

    public StatusAtividade Status { get; set; }
    public DateOnly DataInicio { get; set; }
    public DateOnly DataConclusao { get; set; }
    public DateTime DataRegistro { get; set; }

    /// <summary>Situação do prazo em relação a hoje (calculada no service).</summary>
    public Prazo Prazo { get; set; } = null!;
}
