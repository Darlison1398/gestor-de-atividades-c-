using GestorAtividades.Data;
using GestorAtividades.Models;

namespace GestorAtividades.Tests.Infra;

/// <summary>Atalhos para montar cenários direto no banco.</summary>
public static class Dados
{
    public static string EmailUnico(string prefixo = "usuario") => $"{prefixo}.{Guid.NewGuid():N}@teste.com";

    public static async Task<Usuario> CriarUsuarioAsync(ApplicationDbContext contexto, string? email = null, string senha = "abc12!")
    {
        var usuario = new Usuario
        {
            Nome = "Usuário de Teste",
            Email = email ?? EmailUnico(),
            SenhaHash = BCrypt.Net.BCrypt.HashPassword(senha),
            DataRegistro = DateTime.UtcNow
        };
        contexto.Usuarios.Add(usuario);
        await contexto.SaveChangesAsync();
        return usuario;
    }

    /// <param name="venceEmDias">Dias a partir de hoje (Brasília); negativo = atrasada.</param>
    public static async Task<Atividade> CriarAtividadeAsync(ApplicationDbContext contexto, int userId, string titulo,
        int venceEmDias, StatusAtividade status = StatusAtividade.Pendente, string descricao = "<p>Descrição</p>")
    {
        var hoje = TimeHelper.HojeNoBrasil();
        var atividade = new Atividade
        {
            UserId = userId,
            Titulo = titulo,
            Descricao = descricao,
            Status = status,
            DataInicio = hoje.AddDays(Math.Min(venceEmDias, 0) - 1),
            DataConclusao = hoje.AddDays(venceEmDias),
            DataRegistro = DateTime.UtcNow
        };
        contexto.Atividades.Add(atividade);
        await contexto.SaveChangesAsync();
        return atividade;
    }
}
