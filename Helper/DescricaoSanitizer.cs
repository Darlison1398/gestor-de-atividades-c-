using Ganss.Xss;

namespace GestorAtividades.Helper;

/// <summary>
/// Remove do HTML gerado pelo editor (Quill) tudo o que pode executar script
/// (tags script, atributos on*, links javascript:, etc.).
/// Usado ao salvar e também ao exibir, para cobrir descrições gravadas antes da sanitização.
/// </summary>
public static class DescricaoSanitizer
{
    public static string Sanitizar(string? html)
    {
        if (string.IsNullOrWhiteSpace(html))
            return string.Empty;

        return new HtmlSanitizer().Sanitize(html);
    }
}
