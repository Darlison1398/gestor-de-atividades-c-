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
public class PagesController : Controller
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
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdString == null)
            return Unauthorized();

        int userId = int.Parse(userIdString);

        var atividades = await _context.Atividades
            .Where(a => a.UserId == userId)
            .ToListAsync();

        return View(atividades);
    }

    public IActionResult RegistrarAtividade()
    {
        return View();
    }

    public async Task<IActionResult> Atividade(int id)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdString == null)
            return Unauthorized();

        int userId = int.Parse(userIdString);

        var atividade = await _context.Atividades
            .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);
        if (atividade == null)
            return NotFound();

        atividade.DataInicio = atividade.DataInicio.ToLocalTime();
        atividade.DataConclusao = atividade.DataConclusao.ToLocalTime();

        return View(atividade);
    }

    public async Task<IActionResult> DeletarAtividade(int id)
    {
        try
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdString == null)
                return Unauthorized();

            int userId = int.Parse(userIdString);
            await _atividadeService.Deletar(userId, id);
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
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdString == null)
            return Unauthorized();

        int userId = int.Parse(userIdString);

        var atividade = await _context.Atividades
            .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);
        if (atividade == null)
            return NotFound();

        atividade.DataInicio = atividade.DataInicio.ToLocalTime();
        atividade.DataConclusao = atividade.DataConclusao.ToLocalTime();

        return View("EditarAtividade", atividade);
    }

    public async Task<IActionResult> UpdateAtividade(int id, Atividade atividade)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return View(atividade);
            }
            
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdString == null)
                return Unauthorized();

            int userId = int.Parse(userIdString);

            await _atividadeService.Editar(userId, id, atividade);
            TempData["SuccessMessage"] = "Atividade editada com sucesso!";
            return RedirectToAction("Main", "Pages");

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
