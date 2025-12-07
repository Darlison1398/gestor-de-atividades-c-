using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using GestorAtividades.Models;
using Microsoft.AspNetCore.Authorization;
using GestorAtividades.Data;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace GestorAtividades.Controllers;

[Authorize] 
public class PagesController : Controller
{
    private readonly ILogger<PagesController> _logger;
    private readonly ApplicationDbContext _context;
    public PagesController(ILogger<PagesController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
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
        var atividade = await _context.Atividades
            .FirstOrDefaultAsync(x => x.Id == id);

        if (atividade == null)
            return NotFound();

        return View(atividade);
    }

        

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
