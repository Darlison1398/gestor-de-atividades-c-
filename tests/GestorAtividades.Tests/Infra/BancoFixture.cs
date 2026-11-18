using GestorAtividades.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Testcontainers.PostgreSql;

namespace GestorAtividades.Tests.Infra;

/// <summary>
/// Sobe um PostgreSQL descartável (Docker, mesma versão do servidor de desenvolvimento) uma vez
/// para todos os testes da coleção "Banco" e aplica as migrations do projeto.
/// Cada teste cria os próprios usuários (emails únicos), então os testes não interferem entre si.
/// </summary>
public sealed class BancoFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:16-alpine").Build();

    public string ConnectionString => _container.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        await using var contexto = CriarContexto();
        await contexto.Database.MigrateAsync();
    }

    public Task DisposeAsync() => _container.DisposeAsync().AsTask();

    public ApplicationDbContext CriarContexto() => CriarContexto(ConnectionString);

    public static ApplicationDbContext CriarContexto(string connectionString) =>
        new(new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql(connectionString).Options);

    /// <summary>Cria um banco vazio no mesmo contêiner (ex.: para testar migrations isoladamente).</summary>
    public async Task<string> CriarBancoVazioAsync()
    {
        var nome = "teste_" + Guid.NewGuid().ToString("N")[..12];
        await using (var conexao = new NpgsqlConnection(ConnectionString))
        {
            await conexao.OpenAsync();
            await using var comando = new NpgsqlCommand($"CREATE DATABASE \"{nome}\"", conexao);
            await comando.ExecuteNonQueryAsync();
        }

        return new NpgsqlConnectionStringBuilder(ConnectionString) { Database = nome }.ConnectionString;
    }
}

[CollectionDefinition(Nome)]
public sealed class BancoCollection : ICollectionFixture<BancoFixture>
{
    public const string Nome = "Banco";
}
