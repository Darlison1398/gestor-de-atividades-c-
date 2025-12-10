namespace GestorAtividades.Helper;
public static class StatusExtensions
{
    public static string GetColor(this StatusAtividade status)
    {
        return status switch
        {
            StatusAtividade.Pendente => "text-warning",
            StatusAtividade.EmDesenvolvimento => "text-primary",
            StatusAtividade.EmTeste => "text-info",
            StatusAtividade.Concluido => "text-success",
            _ => "text-dark"
        };
    }
}
