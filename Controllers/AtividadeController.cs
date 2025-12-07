using GestorAtividades.Data;
using GestorAtividades.Models;
using GestorAtividades.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GestorAtividades.Controllers
{
    public class AtividadeController : Controller
    {
        private readonly AtividadeService _atividadeService;
        private readonly ApplicationDbContext _context;

        public AtividadeController(AtividadeService atividadeService, ApplicationDbContext context)
        {
            _atividadeService = atividadeService;
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CriarAtividade (Atividade atividade)
        {
            foreach (var e in ModelState.Values.SelectMany(v => v.Errors))
            {
                Console.WriteLine(e.ErrorMessage);
            }

            try
            {
                var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userIdString == null)
                    return Unauthorized();

                if (!ModelState.IsValid)
                    return View("~/Views/Pages/RegistrarAtividade.cshtml", atividade);


                int userId = int.Parse(userIdString);
                await _atividadeService.Register(userId, atividade);
                TempData["SuccessMessage"] = "Atividade criada com sucesso!";
                return RedirectToAction("Main", "Pages");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("RegistrarAtividade", "Pages");
            }
        }
        
    }
}