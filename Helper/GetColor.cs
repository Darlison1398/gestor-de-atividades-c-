using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace GestorAtividades.Helper;

public static class StatusExtensions
{
    /// <summary>Classes Bootstrap do badge do status (já inclui "badge").</summary>
    public static string GetColor(this StatusAtividade status)
    {
        return status switch
        {
            StatusAtividade.Pendente => "badge bg-warning text-dark",
            StatusAtividade.EmDesenvolvimento => "badge bg-primary",
            StatusAtividade.EmTeste => "badge bg-info text-dark",
            StatusAtividade.Concluido => "badge bg-success",
            _ => "badge bg-dark"
        };
    }

    /// <summary>Nome do atributo [Display] do enum (ex.: "Em Desenvolvimento").</summary>
    public static string GetDisplayName(this StatusAtividade status)
    {
        return typeof(StatusAtividade)
            .GetMember(status.ToString())
            .FirstOrDefault()?
            .GetCustomAttribute<DisplayAttribute>()?
            .GetName() ?? status.ToString();
    }
}
