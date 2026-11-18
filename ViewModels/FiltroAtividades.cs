namespace GestorAtividades.ViewModels;

/// <summary>
/// Filtros da tela de atividades, vindos da query string (?visao=&q=&status=&situacao=&ordem=).
/// Valores desconhecidos caem no padrão.
/// </summary>
public class FiltroAtividades
{
    public const string VisaoQuadro = "quadro";
    public const string VisaoLista = "lista";

    /// <summary>"quadro" (Kanban, padrão) ou "lista".</summary>
    public string? Visao { get; set; }

    /// <summary>Busca no título.</summary>
    public string? Q { get; set; }

    /// <summary>Só na visão em lista: no quadro cada status já é uma coluna.</summary>
    public StatusAtividade? Status { get; set; }

    /// <summary>"atrasadas", "hoje" ou "semana" (vencem nos próximos 7 dias). Ignora concluídas.</summary>
    public string? Situacao { get; set; }

    /// <summary>"prazo" (padrão), "recentes" ou "titulo".</summary>
    public string? Ordem { get; set; }

    public bool EhQuadro => Visao != VisaoLista;

    public bool TemFiltro => !string.IsNullOrWhiteSpace(Q) || (!EhQuadro && Status != null) || !string.IsNullOrEmpty(Situacao);

    /// <summary>Valores atuais como rota, para montar links que trocam um filtro e mantêm os outros.</summary>
    public Dictionary<string, string> ComoRota(object? alteracoes = null)
    {
        var rota = new Dictionary<string, string?>
        {
            ["visao"] = Visao,
            ["q"] = Q,
            ["status"] = Status?.ToString(),
            ["situacao"] = Situacao,
            ["ordem"] = Ordem
        };

        if (alteracoes != null)
            foreach (var p in alteracoes.GetType().GetProperties())
                rota[p.Name] = p.GetValue(alteracoes)?.ToString();

        return rota.Where(kv => !string.IsNullOrEmpty(kv.Value)).ToDictionary(kv => kv.Key, kv => kv.Value!);
    }
}
