using GestorAtividades.Tests.Infra;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql;

namespace GestorAtividades.Tests.Servicos;

/// <summary>
/// A migration DatasAtividadeSemHora converte timestamp (UTC) em date usando o fuso de Brasília.
/// Uma atividade às 22h30 de 17/11 em Brasília está gravada como 01h30 de 18/11 em UTC e deve virar 17/11.
/// </summary>
[Collection(BancoCollection.Nome)]
public class MigrationDatasTests(BancoFixture banco)
{
    private const string MigrationAnterior = "20251202005221_AjusteTimestampComTimezone";

    [Fact]
    public async Task Converte_horarios_noturnos_para_o_dia_de_Brasilia()
    {
        var conexao = await banco.CriarBancoVazioAsync();
        await using var ctx = BancoFixture.CriarContexto(conexao);
        var migrator = ctx.GetService<IMigrator>();
        await migrator.MigrateAsync(MigrationAnterior);

        await using (var npg = new NpgsqlConnection(conexao))
        {
            await npg.OpenAsync();
            await using var cmd = new NpgsqlCommand("""
                INSERT INTO "Usuarios" ("Nome", "Email", "SenhaHash", "DataRegistro") VALUES ('Teste', 'm@t.com', 'x', now());
                INSERT INTO "Atividades" ("Titulo", "Descricao", "Status", "UserId", "DataRegistro", "DataInicio", "DataConclusao")
                VALUES ('Noturna', 'x', 'Pendente', (SELECT "Id" FROM "Usuarios"), now(),
                        '2026-11-18 01:30:00+00',   -- 22h30 de 17/11 em Brasília
                        '2026-11-20 14:00:00+00');  -- 11h de 20/11 em Brasília
                """, npg);
            await cmd.ExecuteNonQueryAsync();
        }

        await migrator.MigrateAsync();

        var atividade = await ctx.Atividades.AsNoTracking().SingleAsync();
        Assert.Equal(new DateOnly(2026, 11, 17), atividade.DataInicio);
        Assert.Equal(new DateOnly(2026, 11, 20), atividade.DataConclusao);
    }

    [Fact]
    public async Task Down_volta_para_timestamp_a_meia_noite_de_Brasilia()
    {
        var conexao = await banco.CriarBancoVazioAsync();
        await using var ctx = BancoFixture.CriarContexto(conexao);
        var migrator = ctx.GetService<IMigrator>();
        await migrator.MigrateAsync();

        await using var npg = new NpgsqlConnection(conexao);
        await npg.OpenAsync();
        await using (var cmd = new NpgsqlCommand("""
            INSERT INTO "Usuarios" ("Nome", "Email", "SenhaHash", "DataRegistro") VALUES ('Teste', 'm@t.com', 'x', now());
            INSERT INTO "Atividades" ("Titulo", "Descricao", "Status", "UserId", "DataRegistro", "DataInicio", "DataConclusao")
            VALUES ('Data', 'x', 'Pendente', (SELECT "Id" FROM "Usuarios"), now(), '2026-11-17', '2026-11-20');
            """, npg))
            await cmd.ExecuteNonQueryAsync();

        await migrator.MigrateAsync(MigrationAnterior);

        await using var consulta = new NpgsqlCommand("""SELECT "DataInicio" AT TIME ZONE 'UTC' FROM "Atividades" """, npg);
        var inicioUtc = (DateTime)(await consulta.ExecuteScalarAsync())!;
        Assert.Equal(new DateTime(2026, 11, 17, 3, 0, 0), inicioUtc); // 00h em Brasília = 03h UTC
    }
}
