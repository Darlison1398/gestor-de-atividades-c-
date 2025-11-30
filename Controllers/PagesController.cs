using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using GestorAtividades.Models;
using Microsoft.AspNetCore.Authorization;

namespace GestorAtividades.Controllers;

[Authorize] 
public class PagesController : Controller
{
    private readonly ILogger<PagesController> _logger;

    public PagesController(ILogger<PagesController> logger)
    {
        _logger = logger;
    }

    public IActionResult Main()
    {
        return View();
    }

    public IActionResult RegistrarAtividade()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
