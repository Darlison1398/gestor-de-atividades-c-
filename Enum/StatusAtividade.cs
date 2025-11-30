using System.ComponentModel.DataAnnotations;

public enum StatusAtividade
{
    [Display(Name = "Pendente")]
    Pendente,

    [Display(Name = "Em Desenvolvimento")]
    EmDesenvolvimento,

    [Display(Name = "Em Teste")]
    EmTeste,

    [Display(Name = "Concluído")]
    Concluido
}
