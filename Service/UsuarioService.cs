using GestorAtividades.Data;
using GestorAtividades.Exceptions;
using GestorAtividades.Helper;
using GestorAtividades.Models;
using GestorAtividades.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace GestorAtividades.Service
{
    /// <summary>
    /// Emails são gravados normalizados (minúsculas). As buscas comparam com ToLower() no banco
    /// para também encontrar contas antigas, cadastradas antes da normalização.
    /// </summary>
    public class UsuarioService
    {
        private readonly ApplicationDbContext _context;

        public UsuarioService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <returns>O usuário, ou null se email/senha não conferem.</returns>
        public async Task<Usuario?> AutenticarAsync(string? email, string? senha)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrEmpty(senha))
                return null;

            var emailNormalizado = Autenticacao.NormalizarEmail(email);
            var usuario = await _context.Usuarios
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email.ToLower() == emailNormalizado);

            if (usuario?.SenhaHash == null || !BCrypt.Net.BCrypt.Verify(senha, usuario.SenhaHash))
                return null;

            return usuario;
        }

        public async Task RegistrarAsync(RegistrarUsuarioViewModel form)
        {
            var email = Autenticacao.NormalizarEmail(form.Email);

            if (await EmailEmUsoAsync(email, ignorarUsuarioId: null))
                throw new RegraNegocioException("Este email já está registrado.");

            _context.Usuarios.Add(new Usuario
            {
                Nome = form.Nome.Trim(),
                Email = email,
                SenhaHash = BCrypt.Net.BCrypt.HashPassword(form.Senha),
                DataRegistro = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
        }

        public async Task<PerfilViewModel?> ObterPerfilAsync(int userId)
        {
            var usuario = await _context.Usuarios.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
            if (usuario == null)
                return null;

            return new PerfilViewModel
            {
                Nome = usuario.Nome,
                Email = usuario.Email,
                DataRegistro = TimeHelper.UtcToBrazil(usuario.DataRegistro)
            };
        }

        public async Task<EditarPerfilViewModel?> ObterParaEdicaoAsync(int userId)
        {
            var usuario = await _context.Usuarios.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
            if (usuario == null)
                return null;

            return new EditarPerfilViewModel { Nome = usuario.Nome, Email = usuario.Email };
        }

        /// <returns>O usuário atualizado (para renovar o cookie), ou null se não existe.</returns>
        public async Task<Usuario?> EditarAsync(int userId, EditarPerfilViewModel form)
        {
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == userId);
            if (usuario == null)
                return null;

            var email = Autenticacao.NormalizarEmail(form.Email);
            if (await EmailEmUsoAsync(email, ignorarUsuarioId: userId))
                throw new RegraNegocioException("Este email já está em uso por outra conta.");

            usuario.Nome = form.Nome.Trim();
            usuario.Email = email;

            if (!string.IsNullOrEmpty(form.Senha))
                usuario.SenhaHash = BCrypt.Net.BCrypt.HashPassword(form.Senha);

            await _context.SaveChangesAsync();
            return usuario;
        }

        private Task<bool> EmailEmUsoAsync(string emailNormalizado, int? ignorarUsuarioId) =>
            _context.Usuarios.AnyAsync(u =>
                u.Email.ToLower() == emailNormalizado &&
                (ignorarUsuarioId == null || u.Id != ignorarUsuarioId));
    }
}
