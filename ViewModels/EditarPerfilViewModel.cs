using System.ComponentModel.DataAnnotations;

namespace GestorAtividades.ViewModels;

public class EditarPerfilViewModel
{
    [Display(Name = "Nome completo")]
    public string Nome { get; set; } = string.Empty;

    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    /// <summary>Opcional: em branco mantém a senha atual.</summary>
    [Display(Name = "Nova senha")]
    [DataType(DataType.Password)]
    public string? Senha { get; set; }

    [Display(Name = "Confirmar nova senha")]
    [DataType(DataType.Password)]
    public string? ConfirmarSenha { get; set; }
}
