namespace GestorAtividades.Helper;
public static class StatusExtensions
{
    public static string GetColor(this StatusAtividade status)
    {
        return status switch
        {
            StatusAtividade.Pendente => "badge bg-warning",
            StatusAtividade.EmDesenvolvimento => "badge bg-primary",
            StatusAtividade.EmTeste => "badge bg-info",
            StatusAtividade.Concluido => "badge bg-success",
            _ => "badge bg-dark"
        };
    }
}
