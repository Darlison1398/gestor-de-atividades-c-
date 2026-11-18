using GestorAtividades.Data;
using GestorAtividades.Helper;
using GestorAtividades.Models;
using GestorAtividades.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace GestorAtividades.Service
{
    /// <summary>
    /// Toda consulta filtra por UserId: um usuário só enxerga e altera as próprias atividades.
    /// Datas de início/conclusão são DateOnly e passam direto; DataRegistro é UTC no banco e Brasília na saída.
    /// </summary>
    public class AtividadeService
    {
        private readonly ApplicationDbContext _context;

        public AtividadeService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>Atividades filtradas e ordenadas + resumo de todas as atividades do usuário.</summary>
        public async Task<PainelAtividadesViewModel> ListarAsync(int userId, FiltroAtividades filtro)
        {
            var hoje = TimeHelper.HojeNoBrasil();
            var query = _context.Atividades.AsNoTracking().Where(a => a.UserId == userId);

            if (!string.IsNullOrWhiteSpace(filtro.Q))
            {
                var termo = "%" + EscaparLike(filtro.Q.Trim()) + "%";
                query = query.Where(a => EF.Functions.ILike(a.Titulo, termo, @"\"));
            }

            // No quadro cada status é uma coluna, então o filtro de status só vale na lista.
            if (!filtro.EhQuadro && filtro.Status is StatusAtividade status)
                query = query.Where(a => a.Status == status);

            var limiteSemana = hoje.AddDays(Prazo.DiasEmBreve);
            query = filtro.Situacao switch
            {
                "atrasadas" => query.Where(a => a.Status != StatusAtividade.Concluido && a.DataConclusao < hoje),
                "hoje" => query.Where(a => a.Status != StatusAtividade.Concluido && a.DataConclusao == hoje),
                "semana" => query.Where(a => a.Status != StatusAtividade.Concluido && a.DataConclusao >= hoje && a.DataConclusao <= limiteSemana),
                _ => query
            };

            query = filtro.Ordem switch
            {
                "recentes" => query.OrderByDescending(a => a.DataRegistro),
                "titulo" => query.OrderBy(a => a.Titulo),
                // Padrão: não concluídas primeiro, depois o prazo mais próximo.
                _ => query.OrderBy(a => a.Status == StatusAtividade.Concluido).ThenBy(a => a.DataConclusao).ThenBy(a => a.Titulo)
            };

            var atividades = await query.ToListAsync();

            return new PainelAtividadesViewModel
            {
                Filtro = filtro,
                Resumo = await ObterResumoAsync(userId),
                Atividades = atividades.Select(a => ParaViewModel(a, hoje)).ToList()
            };
        }

        /// <summary>Contagens de todas as atividades do usuário (uma consulta agrupada por status).</summary>
        public async Task<ResumoAtividades> ObterResumoAsync(int userId)
        {
            var hoje = TimeHelper.HojeNoBrasil();
            var limiteSemana = hoje.AddDays(Prazo.DiasEmBreve);

            var grupos = await _context.Atividades
                .Where(a => a.UserId == userId)
                .GroupBy(a => a.Status)
                .Select(g => new
                {
                    Status = g.Key,
                    Total = g.Count(),
                    Atrasadas = g.Count(a => a.DataConclusao < hoje),
                    Hoje = g.Count(a => a.DataConclusao == hoje),
                    Semana = g.Count(a => a.DataConclusao >= hoje && a.DataConclusao <= limiteSemana)
                })
                .ToListAsync();

            var abertas = grupos.Where(g => g.Status != StatusAtividade.Concluido).ToList();

            return new ResumoAtividades
            {
                Total = grupos.Sum(g => g.Total),
                Concluidas = grupos.Where(g => g.Status == StatusAtividade.Concluido).Sum(g => g.Total),
                Atrasadas = abertas.Sum(g => g.Atrasadas),
                VencemHoje = abertas.Sum(g => g.Hoje),
                VencemNaSemana = abertas.Sum(g => g.Semana),
                PorStatus = Enum.GetValues<StatusAtividade>()
                    .ToDictionary(s => s, s => grupos.FirstOrDefault(g => g.Status == s)?.Total ?? 0)
            };
        }

        public async Task<AtividadeViewModel?> ObterAsync(int userId, int id)
        {
            var atividade = await BuscarAsync(userId, id);
            return atividade == null ? null : ParaViewModel(atividade, TimeHelper.HojeNoBrasil());
        }

        public async Task<AtividadeFormViewModel?> ObterParaEdicaoAsync(int userId, int id)
        {
            var atividade = await BuscarAsync(userId, id);
            if (atividade == null)
                return null;

            return new AtividadeFormViewModel
            {
                Id = atividade.Id,
                Titulo = atividade.Titulo,
                Descricao = DescricaoSanitizer.Sanitizar(atividade.Descricao),
                Status = atividade.Status,
                DataInicio = atividade.DataInicio,
                DataConclusao = atividade.DataConclusao
            };
        }

        public async Task CriarAsync(int userId, AtividadeFormViewModel form)
        {
            _context.Atividades.Add(new Atividade
            {
                Titulo = form.Titulo.Trim(),
                Descricao = DescricaoSanitizer.Sanitizar(form.Descricao),
                DataInicio = form.DataInicio,
                DataConclusao = form.DataConclusao,
                DataRegistro = DateTime.UtcNow,
                Status = StatusAtividade.Pendente,
                UserId = userId
            });

            await _context.SaveChangesAsync();
        }

        /// <returns>false se a atividade não existe ou não pertence ao usuário.</returns>
        public async Task<bool> EditarAsync(int userId, int id, AtividadeFormViewModel form)
        {
            var atividade = await _context.Atividades
                .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);

            if (atividade == null)
                return false;

            atividade.Titulo = form.Titulo.Trim();
            atividade.Descricao = DescricaoSanitizer.Sanitizar(form.Descricao);
            atividade.Status = form.Status;
            atividade.DataInicio = form.DataInicio;
            atividade.DataConclusao = form.DataConclusao;

            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>Usado pelo quadro (arrastar entre colunas e menu "Mover para").</summary>
        /// <returns>false se a atividade não existe ou não pertence ao usuário.</returns>
        public async Task<bool> AlterarStatusAsync(int userId, int id, StatusAtividade status)
        {
            var alteradas = await _context.Atividades
                .Where(a => a.Id == id && a.UserId == userId)
                .ExecuteUpdateAsync(s => s.SetProperty(a => a.Status, status));

            return alteradas > 0;
        }

        /// <returns>false se a atividade não existe ou não pertence ao usuário.</returns>
        public async Task<bool> ExcluirAsync(int userId, int id)
        {
            var removidas = await _context.Atividades
                .Where(a => a.Id == id && a.UserId == userId)
                .ExecuteDeleteAsync();

            return removidas > 0;
        }

        private Task<Atividade?> BuscarAsync(int userId, int id) =>
            _context.Atividades
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);

        // Escapa os curingas do LIKE para que "%" e "_" digitados na busca sejam literais.
        private static string EscaparLike(string texto) =>
            texto.Replace(@"\", @"\\").Replace("%", @"\%").Replace("_", @"\_");

        private static AtividadeViewModel ParaViewModel(Atividade atividade, DateOnly hoje) => new()
        {
            Id = atividade.Id,
            Titulo = atividade.Titulo,
            DescricaoHtml = DescricaoSanitizer.Sanitizar(atividade.Descricao),
            Resumo = DescricaoSanitizer.ParaTextoPlano(atividade.Descricao, 160),
            Status = atividade.Status,
            DataInicio = atividade.DataInicio,
            DataConclusao = atividade.DataConclusao,
            DataRegistro = TimeHelper.UtcToBrazil(atividade.DataRegistro),
            Prazo = Prazo.Calcular(atividade.DataConclusao, atividade.Status, hoje)
        };
    }
}
