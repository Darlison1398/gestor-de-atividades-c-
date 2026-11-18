using System.Security.Claims;
using GestorAtividades.Models;

namespace GestorAtividades.Helper;

public static class Autenticacao
{
    public const string Esquema = "CookieAuth";

    /// <summary>Monta o usuário logado (claims) gravado no cookie. Usado no login e após editar o perfil.</summary>
    public static ClaimsPrincipal CriarPrincipal(Usuario usuario)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new(ClaimTypes.Name, usuario.Nome),
            new(ClaimTypes.Email, usuario.Email),
        };

        return new ClaimsPrincipal(new ClaimsIdentity(claims, Esquema));
    }

    public static string NormalizarEmail(string? email) => (email ?? string.Empty).Trim().ToLowerInvariant();
}
