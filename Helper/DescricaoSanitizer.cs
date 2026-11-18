using System.Net;
using System.Text.RegularExpressions;
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

    /// <summary>
    /// Texto puro do HTML (sem tags, entidades decodificadas), opcionalmente truncado com "…".
    /// Sanitiza antes: o conteúdo de &lt;script&gt;/&lt;style&gt; de descrições antigas não vira texto.
    /// </summary>
    public static string ParaTextoPlano(string? html, int? maximo = null)
    {
        var seguro = Sanitizar(html);
        if (seguro.Length == 0)
            return string.Empty;

        // Quebras de bloco viram espaço para não grudar palavras de parágrafos diferentes.
        var semTags = Regex.Replace(seguro, @"<\s*(br|/p|/li|/h\d|/div)[^>]*>", " ", RegexOptions.IgnoreCase);
        semTags = Regex.Replace(semTags, "<[^>]*>", string.Empty);
        var texto = Regex.Replace(WebUtility.HtmlDecode(semTags), @"\s+", " ").Trim();

        if (maximo is int max && texto.Length > max)
            texto = texto[..max].TrimEnd() + "…";

        return texto;
    }
}
