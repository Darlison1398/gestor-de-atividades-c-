using System.ComponentModel.DataAnnotations;

namespace GestorAtividades.ViewModels;

/// <summary>
/// Formulário de criação/edição. Contém só o que o usuário pode alterar:
/// UserId e DataRegistro nunca vêm do formulário. As datas não têm hora.
/// Id == 0 significa criação.
/// </summary>
public class AtividadeFormViewModel
{
    public int Id { get; set; }

    [Display(Name = "Título")]
    public string Titulo { get; set; } = string.Empty;

    [Display(Name = "Descrição")]
    public string Descricao { get; set; } = string.Empty;

    [Display(Name = "Status")]
    public StatusAtividade Status { get; set; } = StatusAtividade.Pendente;

    [Display(Name = "Data de início")]
    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    public DateOnly DataInicio { get; set; }

    [Display(Name = "Data prevista de conclusão")]
    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    public DateOnly DataConclusao { get; set; }

    public bool EhEdicao => Id != 0;
}
