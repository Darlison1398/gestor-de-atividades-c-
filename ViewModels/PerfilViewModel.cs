namespace GestorAtividades.ViewModels;

public class PerfilViewModel
{
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    /// <summary>Horário de Brasília.</summary>
    public DateTime DataRegistro { get; set; }

    public ResumoAtividades Resumo { get; set; } = new();
}
