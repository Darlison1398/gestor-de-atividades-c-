using GestorAtividades.Helper;

namespace GestorAtividades.Tests.Unidade;

public class DescricaoSanitizerTests
{
    [Theory]
    [InlineData("<p>ok</p><script>alert(1)</script>", "<script")]
    [InlineData("<img src=x onerror=\"alert(1)\">", "onerror")]
    [InlineData("<a href=\"javascript:alert(1)\">x</a>", "javascript:")]
    [InlineData("<p onclick=\"roubar()\">x</p>", "onclick")]
    [InlineData("<iframe src=\"https://exemplo.com\"></iframe>", "<iframe")]
    public void Remove_o_que_pode_executar_script(string html, string proibido)
    {
        var limpo = DescricaoSanitizer.Sanitizar(html);

        Assert.DoesNotContain(proibido, limpo, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Mantem_a_formatacao_do_editor()
    {
        const string html = "<h3>Objetivo</h3><p><strong>negrito</strong> <em>itálico</em> <u>sublinhado</u></p>" +
                            "<ol><li>um</li></ol><ul><li>dois</li></ul><p><a href=\"https://exemplo.com\">link</a></p>";

        var limpo = DescricaoSanitizer.Sanitizar(html);

        foreach (var tag in new[] { "<h3>", "<strong>", "<em>", "<u>", "<ol>", "<ul>", "<li>", "href=\"https://exemplo.com\"" })
            Assert.Contains(tag, limpo);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Vazio_vira_texto_vazio(string? html)
    {
        Assert.Equal(string.Empty, DescricaoSanitizer.Sanitizar(html));
        Assert.Equal(string.Empty, DescricaoSanitizer.ParaTextoPlano(html));
    }

    [Fact]
    public void Texto_plano_separa_blocos_e_decodifica_entidades()
    {
        var texto = DescricaoSanitizer.ParaTextoPlano("<h3>Objetivo</h3><p>Fazer&nbsp;A &amp; B</p><ul><li>um</li><li>dois</li></ul>");

        Assert.Equal("Objetivo Fazer A & B um dois", texto);
    }

    [Fact]
    public void Texto_plano_descarta_o_conteudo_de_scripts()
    {
        Assert.Equal("texto", DescricaoSanitizer.ParaTextoPlano("<p>texto</p><script>alert(1)</script><style>p{}</style>"));
    }

    [Fact]
    public void Texto_plano_trunca_com_reticencias()
    {
        var texto = DescricaoSanitizer.ParaTextoPlano("<p>" + new string('a', 50) + "</p>", 10);

        Assert.Equal(new string('a', 10) + "…", texto);
    }

    [Fact]
    public void Paragrafo_vazio_do_editor_nao_tem_texto()
    {
        Assert.Equal(string.Empty, DescricaoSanitizer.ParaTextoPlano("<p><br></p>"));
    }
}
