using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using GestorAtividades.Models;
using Microsoft.AspNetCore.Authorization;
using GestorAtividades.Data;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using GestorAtividades.Service;

namespace GestorAtividades.Controllers;

[Authorize] 
public class PagesController : BaseController
{
    private readonly ILogger<PagesController> _logger;
    private readonly ApplicationDbContext _context;
    private readonly AtividadeService _atividadeService;
    public PagesController(ILogger<PagesController> logger, ApplicationDbContext context, AtividadeService atividadeService)
    {
        _logger = logger;
        _context = context;
        _atividadeService = atividadeService;
    }

    public async Task<IActionResult> Main()
    {
        if (UserId == 0)
            return Unauthorized();

        var atividades = await _atividadeService
            .ListarPorUsuarioAsync(UserId);

        return View(atividades);
    }

    public IActionResult RegistrarAtividade()
    {
        return View();
    }

    public async Task<IActionResult> Atividade(int id)
    {
        if (UserId == 0)
            return Unauthorized();

        var atividade = await _atividadeService
            .BuscarPorIdAsync(UserId, id);

        if (atividade == null)
            return NotFound();

        return View(atividade);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeletarAtividade(int id)
    {
        try
        {
            if (UserId == 0)
                return Unauthorized();

            await _atividadeService.Deletar(UserId, id);
            TempData["SuccessMessage"] = "Atividade excluída com sucesso!";
            return RedirectToAction("Main", "Pages");
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction("Main");
        }
    }

    public async Task<IActionResult> EditarAtividade(int id)
    {
        if (UserId == 0)
            return Unauthorized();

        var atividade = await _atividadeService.BuscarPorIdAsync(UserId, id);
        if (atividade == null)
            return NotFound();

        return View("EditarAtividade", atividade);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateAtividade(int id, Atividade atividade)
    {
        if (UserId == 0)
            return Unauthorized();

        if (!ModelState.IsValid)
            return View("EditarAtividade", atividade);

        try
        {
            await _atividadeService.Editar(UserId, id, atividade);
            TempData["SuccessMessage"] = "Atividade editada com sucesso!";
            return RedirectToAction("Main");

        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction("Main");
        }
    }

        

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
