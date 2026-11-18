using GestorAtividades.Service;
using GestorAtividades.Tests.Infra;
using GestorAtividades.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace GestorAtividades.Tests.Servicos;

[Collection(BancoCollection.Nome)]
public class AtividadeServiceTests(BancoFixture banco)
{
    private static readonly DateOnly Hoje = TimeHelper.HojeNoBrasil();

    private static List<string> Titulos(PainelAtividadesViewModel painel) => painel.Atividades.Select(a => a.Titulo).ToList();

    [Fact]
    public async Task Criar_sanitiza_a_descricao_e_comeca_pendente()
    {
        await using var ctx = banco.CriarContexto();
        var usuario = await Dados.CriarUsuarioAsync(ctx);

        await new AtividadeService(ctx).CriarAsync(usuario.Id, new AtividadeFormViewModel
        {
            Titulo = "  Com espaços  ",
            Descricao = "<p>ok</p><script>alert(1)</script>",
            Status = StatusAtividade.Concluido, // ignorado na criação
            DataInicio = Hoje,
            DataConclusao = Hoje.AddDays(1)
        });

        var salva = await ctx.Atividades.AsNoTracking().SingleAsync(a => a.UserId == usuario.Id);
        Assert.Equal("Com espaços", salva.Titulo);
        Assert.DoesNotContain("<script", salva.Descricao);
        Assert.Equal(StatusAtividade.Pendente, salva.Status);
        Assert.Equal(Hoje, salva.DataInicio);
        Assert.Equal(DateTimeKind.Utc, salva.DataRegistro.Kind);
    }

    [Fact]
    public async Task Usuario_so_ve_as_proprias_atividades()
    {
        await using var ctx = banco.CriarContexto();
        var ana = await Dados.CriarUsuarioAsync(ctx);
        var bia = await Dados.CriarUsuarioAsync(ctx);
        var daBia = await Dados.CriarAtividadeAsync(ctx, bia.Id, "Atividade da Bia", 3);
        await Dados.CriarAtividadeAsync(ctx, ana.Id, "Atividade da Ana", 3);
        var servico = new AtividadeService(ctx);

        var painel = await servico.ListarAsync(ana.Id, new FiltroAtividades());

        Assert.Equal(["Atividade da Ana"], Titulos(painel));
        Assert.Equal(1, painel.Resumo.Total);
        Assert.Null(await servico.ObterAsync(ana.Id, daBia.Id));
        Assert.Null(await servico.ObterParaEdicaoAsync(ana.Id, daBia.Id));
    }

    [Fact]
    public async Task Nao_altera_atividade_de_outro_usuario()
    {
        await using var ctx = banco.CriarContexto();
        var ana = await Dados.CriarUsuarioAsync(ctx);
        var bia = await Dados.CriarUsuarioAsync(ctx);
        var daBia = await Dados.CriarAtividadeAsync(ctx, bia.Id, "Atividade da Bia", 3);
        var servico = new AtividadeService(ctx);
        var form = new AtividadeFormViewModel { Titulo = "Invadida", Descricao = "<p>x</p>", DataInicio = Hoje, DataConclusao = Hoje };

        Assert.False(await servico.EditarAsync(ana.Id, daBia.Id, form));
        Assert.False(await servico.AlterarStatusAsync(ana.Id, daBia.Id, StatusAtividade.Concluido));
        Assert.False(await servico.ExcluirAsync(ana.Id, daBia.Id));

        var intacta = await ctx.Atividades.AsNoTracking().SingleAsync(a => a.Id == daBia.Id);
        Assert.Equal("Atividade da Bia", intacta.Titulo);
        Assert.Equal(StatusAtividade.Pendente, intacta.Status);
    }

    [Fact]
    public async Task Ordem_padrao_poe_abertas_primeiro_e_depois_o_prazo_mais_proximo()
    {
        await using var ctx = banco.CriarContexto();
        var u = await Dados.CriarUsuarioAsync(ctx);
        await Dados.CriarAtividadeAsync(ctx, u.Id, "Concluída antiga", -10, StatusAtividade.Concluido);
        await Dados.CriarAtividadeAsync(ctx, u.Id, "Vence em 5", 5);
        await Dados.CriarAtividadeAsync(ctx, u.Id, "Atrasada", -2);
        await Dados.CriarAtividadeAsync(ctx, u.Id, "Vence hoje", 0, StatusAtividade.EmTeste);

        var painel = await new AtividadeService(ctx).ListarAsync(u.Id, new FiltroAtividades());

        Assert.Equal(["Atrasada", "Vence hoje", "Vence em 5", "Concluída antiga"], Titulos(painel));
    }

    [Fact]
    public async Task Busca_ignora_maiusculas_e_trata_curingas_como_texto()
    {
        await using var ctx = banco.CriarContexto();
        var u = await Dados.CriarUsuarioAsync(ctx);
        await Dados.CriarAtividadeAsync(ctx, u.Id, "Revisar CONTRATO", 3);
        await Dados.CriarAtividadeAsync(ctx, u.Id, "Desconto de 50% no plano", 3);
        await Dados.CriarAtividadeAsync(ctx, u.Id, "Outra coisa", 3);
        var servico = new AtividadeService(ctx);

        Assert.Equal(["Revisar CONTRATO"], Titulos(await servico.ListarAsync(u.Id, new FiltroAtividades { Q = "contrato" })));
        Assert.Equal(["Desconto de 50% no plano"], Titulos(await servico.ListarAsync(u.Id, new FiltroAtividades { Q = "50%" })));
        // "%" e "_" são literais: "%" só encontra o título que tem "%"; sem escape, ambos casariam com tudo.
        Assert.Equal(["Desconto de 50% no plano"], Titulos(await servico.ListarAsync(u.Id, new FiltroAtividades { Q = "%" })));
        Assert.Empty((await servico.ListarAsync(u.Id, new FiltroAtividades { Q = "_" })).Atividades);
    }

    [Theory]
    [InlineData("atrasadas", new[] { "Atrasada" })]
    [InlineData("hoje", new[] { "Vence hoje" })]
    [InlineData("semana", new[] { "Vence hoje", "Vence em 7" })]
    public async Task Filtros_de_prazo_ignoram_concluidas(string situacao, string[] esperado)
    {
        await using var ctx = banco.CriarContexto();
        var u = await Dados.CriarUsuarioAsync(ctx);
        await Dados.CriarAtividadeAsync(ctx, u.Id, "Atrasada", -1);
        await Dados.CriarAtividadeAsync(ctx, u.Id, "Atrasada mas concluída", -1, StatusAtividade.Concluido);
        await Dados.CriarAtividadeAsync(ctx, u.Id, "Vence hoje", 0);
        await Dados.CriarAtividadeAsync(ctx, u.Id, "Vence hoje mas concluída", 0, StatusAtividade.Concluido);
        await Dados.CriarAtividadeAsync(ctx, u.Id, "Vence em 7", 7);
        await Dados.CriarAtividadeAsync(ctx, u.Id, "Vence em 8", 8);

        var painel = await new AtividadeService(ctx).ListarAsync(u.Id, new FiltroAtividades { Situacao = situacao });

        Assert.Equal(esperado, Titulos(painel));
    }

    [Fact]
    public async Task Filtro_de_status_vale_so_na_lista()
    {
        await using var ctx = banco.CriarContexto();
        var u = await Dados.CriarUsuarioAsync(ctx);
        await Dados.CriarAtividadeAsync(ctx, u.Id, "Em teste", 3, StatusAtividade.EmTeste);
        await Dados.CriarAtividadeAsync(ctx, u.Id, "Pendente", 3);
        var servico = new AtividadeService(ctx);

        var lista = await servico.ListarAsync(u.Id, new FiltroAtividades { Visao = "lista", Status = StatusAtividade.EmTeste });
        var quadro = await servico.ListarAsync(u.Id, new FiltroAtividades { Status = StatusAtividade.EmTeste });

        Assert.Equal(["Em teste"], Titulos(lista));
        Assert.Equal(2, quadro.Atividades.Count);
    }

    [Fact]
    public async Task Resumo_conta_todas_as_atividades_independente_do_filtro()
    {
        await using var ctx = banco.CriarContexto();
        var u = await Dados.CriarUsuarioAsync(ctx);
        await Dados.CriarAtividadeAsync(ctx, u.Id, "Atrasada", -3);
        await Dados.CriarAtividadeAsync(ctx, u.Id, "Hoje", 0, StatusAtividade.EmDesenvolvimento);
        await Dados.CriarAtividadeAsync(ctx, u.Id, "Semana", 4, StatusAtividade.EmTeste);
        await Dados.CriarAtividadeAsync(ctx, u.Id, "Distante", 30);
        await Dados.CriarAtividadeAsync(ctx, u.Id, "Feita", -5, StatusAtividade.Concluido);

        var resumo = (await new AtividadeService(ctx).ListarAsync(u.Id, new FiltroAtividades { Q = "nada-casa" })).Resumo;

        Assert.Equal(5, resumo.Total);
        Assert.Equal(4, resumo.Abertas);
        Assert.Equal(1, resumo.Concluidas);
        Assert.Equal(1, resumo.Atrasadas);
        Assert.Equal(1, resumo.VencemHoje);
        Assert.Equal(2, resumo.VencemNaSemana);
        Assert.Equal(20, resumo.TaxaConclusao);
        Assert.Equal(2, resumo.PorStatus[StatusAtividade.Pendente]);
        Assert.Equal(1, resumo.PorStatus[StatusAtividade.EmTeste]);
    }

    [Fact]
    public async Task Resumo_de_usuario_sem_atividades_e_zerado()
    {
        await using var ctx = banco.CriarContexto();
        var u = await Dados.CriarUsuarioAsync(ctx);

        var resumo = await new AtividadeService(ctx).ObterResumoAsync(u.Id);

        Assert.Equal(0, resumo.Total);
        Assert.Equal(0, resumo.TaxaConclusao);
        Assert.All(Enum.GetValues<StatusAtividade>(), s => Assert.Equal(0, resumo.PorStatus[s]));
    }

    [Fact]
    public async Task Editar_e_alterar_status_do_proprio_usuario()
    {
        await using var ctx = banco.CriarContexto();
        var u = await Dados.CriarUsuarioAsync(ctx);
        var atividade = await Dados.CriarAtividadeAsync(ctx, u.Id, "Original", 3);
        var servico = new AtividadeService(ctx);

        var form = await servico.ObterParaEdicaoAsync(u.Id, atividade.Id);
        Assert.NotNull(form);
        form.Titulo = "Editada";
        form.Descricao = "<p>nova</p><img src=x onerror=alert(1)>";
        form.DataInicio = Hoje.AddDays(-40); // edição aceita datas passadas
        form.DataConclusao = Hoje.AddDays(-30);
        Assert.True(await servico.EditarAsync(u.Id, atividade.Id, form));
        Assert.True(await servico.AlterarStatusAsync(u.Id, atividade.Id, StatusAtividade.Concluido));

        var vm = await servico.ObterAsync(u.Id, atividade.Id);
        Assert.NotNull(vm);
        Assert.Equal("Editada", vm.Titulo);
        Assert.DoesNotContain("onerror", vm.DescricaoHtml);
        Assert.Equal(Hoje.AddDays(-30), vm.DataConclusao);
        Assert.Equal(StatusAtividade.Concluido, vm.Status);
        Assert.Equal("Concluída", vm.Prazo.Texto);
    }

    [Fact]
    public async Task Descricoes_antigas_nao_sanitizadas_saem_limpas_na_leitura()
    {
        await using var ctx = banco.CriarContexto();
        var u = await Dados.CriarUsuarioAsync(ctx);
        var antiga = await Dados.CriarAtividadeAsync(ctx, u.Id, "Gravada antes da sanitização", 3,
            descricao: "<p>texto</p><script>alert(1)</script>");
        var servico = new AtividadeService(ctx);

        var vm = await servico.ObterAsync(u.Id, antiga.Id);
        var form = await servico.ObterParaEdicaoAsync(u.Id, antiga.Id);

        Assert.DoesNotContain("<script", vm!.DescricaoHtml);
        Assert.Equal("texto", vm.Resumo);
        Assert.DoesNotContain("<script", form!.Descricao);
    }

    [Fact]
    public async Task Excluir_remove_a_atividade()
    {
        await using var ctx = banco.CriarContexto();
        var u = await Dados.CriarUsuarioAsync(ctx);
        var atividade = await Dados.CriarAtividadeAsync(ctx, u.Id, "Para excluir", 3);

        Assert.True(await new AtividadeService(ctx).ExcluirAsync(u.Id, atividade.Id));
        Assert.False(await ctx.Atividades.AnyAsync(a => a.Id == atividade.Id));
    }
}
