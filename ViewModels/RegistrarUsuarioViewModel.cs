using System.ComponentModel.DataAnnotations;

namespace GestorAtividades.ViewModels;

public class RegistrarUsuarioViewModel
{
    [Display(Name = "Nome completo")]
    public string Nome { get; set; } = string.Empty;

    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Senha")]
    [DataType(DataType.Password)]
    public string? Senha { get; set; }
}
