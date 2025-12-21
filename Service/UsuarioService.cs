using GestorAtividades.Models;
using GestorAtividades.Data;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using GestorAtividades.Dto;

namespace GestorAtividades.Service
{
    public class UsuarioService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public UsuarioService(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task Register(Usuario model)
        {
            if (await _context.Usuarios.AnyAsync(u => u.Email == model.Email))
                throw new Exception("Email já está registrado!");

            var user = new Usuario
            {
                Nome = model.Nome,
                Email = model.Email,
                SenhaHash = BCrypt.Net.BCrypt.HashPassword(model.Senha),
                DataRegistro = TimeHelper.NowInBrazil()
            };
            _context.Usuarios.Add(user);
            await _context.SaveChangesAsync();
        }

        public async Task<Usuario> GetUsuarioByIdAsync(int userId)
        {
            return await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task Editar(int userId, UsuarioDto usuarioDto)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (usuario == null)
                throw new Exception("Usuário não encontrado");

            usuario.Nome = usuarioDto.Nome;
            usuario.Email = usuarioDto.Email;
            if (!string.IsNullOrWhiteSpace(usuarioDto.Senha))
            {
                usuario.SenhaHash = BCrypt.Net.BCrypt.HashPassword(usuarioDto.Senha);
            }
            await _context.SaveChangesAsync();

        }


    }
}