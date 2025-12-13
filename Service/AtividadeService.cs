using GestorAtividades.Data;
using GestorAtividades.Models;
using Microsoft.AspNetCore.Http.HttpResults;
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

            atividade.DataInicio = TimeHelper.BrazilToUtc(atividade.DataInicio);
            atividade.DataConclusao = TimeHelper.BrazilToUtc(atividade.DataConclusao);
            atividade.UserId = userId;
            atividade.Status = StatusAtividade.Pendente;
            _context.Atividades.Add(atividade);
            await _context.SaveChangesAsync();  
        }

        public async Task Deletar (int userId, int id)
        {
            var atividade = await _context.Atividades
                .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);

            if (atividade == null)
                throw new Exception("Atividade não encontrada ou usuário sem permissão.");

            _context.Atividades.Remove(atividade);
            await _context.SaveChangesAsync();
        }

        public async Task Editar (int userId, int id, Atividade newAtividade)
        {
            var atividade = await _context.Atividades.FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);

            if (atividade == null) 
                throw new Exception("Atividade não encontrada, ou usuário sem permissão para editar");

            atividade.Titulo = newAtividade.Titulo;
            atividade.DataInicio = TimeHelper.BrazilToUtc(newAtividade.DataInicio);
            atividade.DataConclusao = TimeHelper.BrazilToUtc(newAtividade.DataConclusao);
            atividade.Status = newAtividade.Status;
            atividade.Descricao = newAtividade.Descricao;

            await _context.SaveChangesAsync();
            
        }

    }
}