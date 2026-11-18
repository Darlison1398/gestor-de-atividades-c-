namespace GestorAtividades.Exceptions;

/// <summary>
/// Violação de regra de negócio cuja mensagem pode ser mostrada ao usuário
/// (ex.: email já cadastrado). Qualquer outra exceção é tratada como erro interno.
/// </summary>
public class RegraNegocioException : Exception
{
    public RegraNegocioException(string mensagem) : base(mensagem) { }
}
