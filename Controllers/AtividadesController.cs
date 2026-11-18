using GestorAtividades.Helper;
using GestorAtividades.Service;
using GestorAtividades.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestorAtividades.Controllers
{
    /// <summary>CRUD das atividades do usuário logado. Ações que alteram dados são POST + antiforgery.</summary>
    [Authorize]
    public class AtividadesController : BaseController
    {
        private readonly AtividadeService _atividadeService;

        public AtividadesController(AtividadeService atividadeService)
        {
            _atividadeService = atividadeService;
        }

        private static readonly string[] Situacoes = ["atrasadas", "hoje", "semana"];
        private static readonly string[] Ordens = ["prazo", "recentes", "titulo"];

        /// <summary>Quadro (Kanban, padrão) ou lista, com busca e filtros pela query string.</summary>
        public async Task<IActionResult> Index([FromQuery] FiltroAtividades filtro)
        {
            // Valores desconhecidos na URL viram o padrão, sem erro.
            ModelState.Clear();
            filtro.Visao = filtro.Visao == FiltroAtividades.VisaoLista ? FiltroAtividades.VisaoLista : null;
            filtro.Situacao = Situacoes.Contains(filtro.Situacao) ? filtro.Situacao : null;
            filtro.Ordem = Ordens.Contains(filtro.Ordem) && filtro.Ordem != "prazo" ? filtro.Ordem : null;
            filtro.Q = string.IsNullOrWhiteSpace(filtro.Q) ? null : filtro.Q.Trim();
            if (filtro.Status is StatusAtividade status && !Enum.IsDefined(status))
                filtro.Status = null;

            return View(await _atividadeService.ListarAsync(UserId, filtro));
        }

        public async Task<IActionResult> Detalhes(int id)
        {
            var atividade = await _atividadeService.ObterAsync(UserId, id);
            return atividade == null ? NotFound() : View(atividade);
        }

        [HttpGet]
        public IActionResult Criar()
        {
            var hoje = TimeHelper.HojeNoBrasil();

            return View(new AtividadeFormViewModel
            {
                DataInicio = hoje,
                DataConclusao = hoje.AddDays(1)
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Criar(AtividadeFormViewModel form)
        {
            form.Id = 0;
            if (!ModelState.IsValid)
                return View(form);

            await _atividadeService.CriarAsync(UserId, form);
            TempData["SuccessMessage"] = "Atividade criada com sucesso!";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var form = await _atividadeService.ObterParaEdicaoAsync(UserId, id);
            return form == null ? NotFound() : View(form);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, AtividadeFormViewModel form)
        {
            form.Id = id;
            if (!ModelState.IsValid)
                return View(form);

            if (!await _atividadeService.EditarAsync(UserId, id, form))
                return NotFound();

            TempData["SuccessMessage"] = "Atividade atualizada com sucesso!";
            return RedirectToAction(nameof(Detalhes), new { id });
        }

        /// <summary>
        /// Muda só o status. Chamado pelo quadro via fetch (arrastar card / menu "Mover para"),
        /// que envia o cabeçalho X-Requested-With: fetch e recebe 204; sem JavaScript, o formulário
        /// do menu faz um POST normal e volta para a página de origem.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AlterarStatus(int id, StatusAtividade status, string? returnUrl)
        {
            // Valor inválido no binding deixa status = Pendente (padrão) com erro no ModelState.
            if (!ModelState.IsValid || !Enum.IsDefined(status))
                return BadRequest();

            if (!await _atividadeService.AlterarStatusAsync(UserId, id, status))
                return NotFound();

            if (Request.Headers.XRequestedWith == "fetch")
                return NoContent();

            TempData["SuccessMessage"] = $"Atividade movida para {status.GetDisplayName()}.";
            return !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)
                ? LocalRedirect(returnUrl)
                : RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Excluir(int id)
        {
            if (!await _atividadeService.ExcluirAsync(UserId, id))
                return NotFound();

            TempData["SuccessMessage"] = "Atividade excluída com sucesso!";
            return RedirectToAction(nameof(Index));
        }
    }
}
