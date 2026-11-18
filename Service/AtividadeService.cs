using GestorAtividades.Data;
using GestorAtividades.Helper;
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

            var nova = new Atividade
            {
                Titulo = atividade.Titulo,
                Descricao = DescricaoSanitizer.Sanitizar(atividade.Descricao),
                DataInicio = TimeHelper.BrazilToUtc(atividade.DataInicio),
                DataConclusao = TimeHelper.BrazilToUtc(atividade.DataConclusao),
                DataRegistro = DateTime.UtcNow,
                Status = StatusAtividade.Pendente,
                UserId = userId
            };

            _context.Atividades.Add(nova);
            await _context.SaveChangesAsync();
        }

        public async Task Deletar(int userId, int id)
        {
            var atividade = await _context.Atividades
                .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);

            if (atividade == null)
                throw new Exception("Atividade não encontrada ou usuário sem permissão.");

            _context.Atividades.Remove(atividade);
            await _context.SaveChangesAsync();
        }

        public async Task Editar(int userId, int id, Atividade newAtividade)
        {
            var atividade = await _context.Atividades.FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);

            if (atividade == null)
                throw new Exception("Atividade não encontrada, ou usuário sem permissão para editar");

            atividade.Titulo = newAtividade.Titulo;
            atividade.DataInicio = TimeHelper.BrazilToUtc(newAtividade.DataInicio);
            atividade.DataConclusao = TimeHelper.BrazilToUtc(newAtividade.DataConclusao);
            atividade.Status = newAtividade.Status;
            atividade.Descricao = DescricaoSanitizer.Sanitizar(newAtividade.Descricao);

            await _context.SaveChangesAsync();
        }

        /// <summary>Retorna as atividades com as datas já no horário de Brasília (somente leitura).</summary>
        public async Task<List<Atividade>> ListarPorUsuarioAsync(int userId)
        {
            var atividades = await _context.Atividades
                .Where(a => a.UserId == userId)
                .AsNoTracking()
                .ToListAsync();

            atividades.ForEach(ParaHorarioBrasil);
            return atividades;
        }

        /// <summary>Retorna a atividade com as datas já no horário de Brasília (somente leitura).</summary>
        public async Task<Atividade?> BuscarPorIdAsync(int userId, int atividadeId)
        {
            var atividade = await _context.Atividades
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == atividadeId && a.UserId == userId);

            if (atividade != null)
                ParaHorarioBrasil(atividade);

            return atividade;
        }

        // As entidades são carregadas com AsNoTracking, então alterar as datas aqui não afeta o banco.
        private static void ParaHorarioBrasil(Atividade atividade)
        {
            atividade.DataInicio = TimeHelper.UtcToBrazil(atividade.DataInicio);
            atividade.DataConclusao = TimeHelper.UtcToBrazil(atividade.DataConclusao);
            atividade.DataRegistro = TimeHelper.UtcToBrazil(atividade.DataRegistro);
        }
    }
}
