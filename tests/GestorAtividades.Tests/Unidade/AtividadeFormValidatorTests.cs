using FluentValidation.TestHelper;
using GestorAtividades.Validators;
using GestorAtividades.ViewModels;

namespace GestorAtividades.Tests.Unidade;

public class AtividadeFormValidatorTests
{
    private readonly AtividadeFormValidator _validator = new();
    private static readonly DateOnly Hoje = TimeHelper.HojeNoBrasil();

    private static AtividadeFormViewModel Valido(int id = 0) => new()
    {
        Id = id,
        Titulo = "Preparar apresentação",
        Descricao = "<p>Montar os slides</p>",
        DataInicio = Hoje,
        DataConclusao = Hoje.AddDays(2)
    };

    [Fact]
    public void Formulario_valido_passa()
    {
        _validator.TestValidate(Valido()).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Inicio_e_conclusao_no_mesmo_dia_sao_permitidos()
    {
        var form = Valido();
        form.DataConclusao = form.DataInicio;

        _validator.TestValidate(form).ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData("abcd")]
    public void Titulo_vazio_ou_curto_e_recusado(string titulo)
    {
        var form = Valido();
        form.Titulo = titulo;

        _validator.TestValidate(form).ShouldHaveValidationErrorFor(f => f.Titulo);
    }

    [Theory]
    [InlineData("")]
    [InlineData("<p><br></p>")]
    [InlineData("<p>&nbsp;</p>")]
    [InlineData("<script>alert(1)</script>")] // seria removido ao salvar, deixando a descrição vazia
    public void Descricao_sem_texto_e_recusada(string descricao)
    {
        var form = Valido();
        form.Descricao = descricao;

        _validator.TestValidate(form).ShouldHaveValidationErrorFor(f => f.Descricao)
            .WithErrorMessage("A descrição é obrigatória.");
    }

    [Fact]
    public void Conclusao_antes_do_inicio_e_recusada()
    {
        var form = Valido();
        form.DataConclusao = form.DataInicio.AddDays(-1);

        _validator.TestValidate(form).ShouldHaveValidationErrorFor(f => f.DataConclusao)
            .WithErrorMessage("A data de conclusão não pode ser anterior à data de início.");
    }

    [Fact]
    public void Datas_em_branco_sao_recusadas()
    {
        var form = Valido();
        form.DataInicio = default;
        form.DataConclusao = default;

        var resultado = _validator.TestValidate(form);
        resultado.ShouldHaveValidationErrorFor(f => f.DataInicio).WithErrorMessage("Informe a data de início.");
        resultado.ShouldHaveValidationErrorFor(f => f.DataConclusao).WithErrorMessage("Informe a data prevista de conclusão.");
    }

    [Fact]
    public void Na_criacao_o_inicio_nao_pode_ser_antes_de_hoje()
    {
        var form = Valido();
        form.DataInicio = Hoje.AddDays(-1);

        _validator.TestValidate(form).ShouldHaveValidationErrorFor(f => f.DataInicio)
            .WithErrorMessage("A data de início não pode ser anterior a hoje.");
    }

    [Fact]
    public void Na_edicao_datas_passadas_sao_permitidas()
    {
        var form = Valido(id: 42);
        form.DataInicio = Hoje.AddDays(-30);
        form.DataConclusao = Hoje.AddDays(-20);
        form.Status = StatusAtividade.Concluido;

        _validator.TestValidate(form).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Status_fora_do_enum_e_recusado()
    {
        var form = Valido(id: 42);
        form.Status = (StatusAtividade)99;

        _validator.TestValidate(form).ShouldHaveValidationErrorFor(f => f.Status);
    }
}
