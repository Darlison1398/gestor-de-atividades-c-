using GestorAtividades.Data;
using GestorAtividades.Models;
using Microsoft.EntityFrameworkCore;

namespace GestorAtividades.Service
{
    public class AtividadeService
    {
        private readonly ApplicationDbContext _context;

        public AtividadeService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task Register(int userId, Atividade atividade)
        {

            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == userId);

            if (usuario == null)
                throw new Exception("Usuário não encontrado.");

            atividade.UserId = userId;
            atividade.Status = StatusAtividade.Pendente;
            atividade.DataRegistro = TimeHelper.NowInBrazil();
            _context.Atividades.Add(atividade);
            await _context.SaveChangesAsync();
            
        }
    }
}