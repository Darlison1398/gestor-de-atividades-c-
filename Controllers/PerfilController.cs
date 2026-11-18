using GestorAtividades.Exceptions;
using GestorAtividades.Helper;
using GestorAtividades.Service;
using GestorAtividades.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestorAtividades.Controllers
{
    [Authorize]
    public class PerfilController : BaseController
    {
        private readonly UsuarioService _usuarioService;
        private readonly AtividadeService _atividadeService;

        public PerfilController(UsuarioService usuarioService, AtividadeService atividadeService)
        {
            _usuarioService = usuarioService;
            _atividadeService = atividadeService;
        }

        public async Task<IActionResult> Index()
        {
            var perfil = await _usuarioService.ObterPerfilAsync(UserId);
            if (perfil == null)
                return NotFound();

            perfil.Resumo = await _atividadeService.ObterResumoAsync(UserId);
            return View(perfil);
        }

        [HttpGet]
        public async Task<IActionResult> Editar()
        {
            var form = await _usuarioService.ObterParaEdicaoAsync(UserId);
            return form == null ? NotFound() : View(form);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(EditarPerfilViewModel form)
        {
            if (!ModelState.IsValid)
                return View(form);

            try
            {
                var usuario = await _usuarioService.EditarAsync(UserId, form);
                if (usuario == null)
                    return NotFound();

                // Renova o cookie para que nome e email exibidos reflitam a alteração.
                await HttpContext.SignInAsync(Autenticacao.Esquema, Autenticacao.CriarPrincipal(usuario));

                TempData["SuccessMessage"] = "Perfil atualizado com sucesso!";
                return RedirectToAction(nameof(Index));
            }
            catch (RegraNegocioException ex)
            {
                ModelState.AddModelError(nameof(form.Email), ex.Message);
                return View(form);
            }
        }
    }
}
