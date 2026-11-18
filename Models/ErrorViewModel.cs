namespace GestorAtividades.Models;

public class ErrorViewModel
{
    public string? RequestId { get; set; }
    public int Codigo { get; set; } = 500;

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

    public string Titulo => Codigo switch
    {
        400 => "Requisição inválida",
        403 => "Acesso negado",
        404 => "Página não encontrada",
        405 => "Ação não permitida",
        _ => "Algo deu errado"
    };

    public string Mensagem => Codigo switch
    {
        400 => "Não foi possível processar o formulário. Talvez a página tenha ficado aberta por muito tempo: recarregue e tente de novo.",
        403 => "Você não tem permissão para acessar esta página.",
        404 => "A página ou atividade que você procura não existe ou foi removida.",
        405 => "Esta ação não pode ser feita por este caminho.",
        _ => "Ocorreu um erro inesperado ao processar sua solicitação. Tente novamente em instantes."
    };
}
