using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using GestorAtividades.Models;

namespace GestorAtividades.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Atividades");

        return View();
    }

    public IActionResult Autor()
    {
        return View();
    }

    // URL antiga da página "Sobre o autor".
    public IActionResult Privacy() => RedirectToActionPermanent(nameof(Autor));

    public IActionResult Sobre()
    {
        return View();
    }

    /// <summary>
    /// Página de erro amigável. Recebe exceções (UseExceptionHandler, fora de Development)
    /// e códigos de status como 404/400 (UseStatusCodePagesWithReExecute).
    /// </summary>
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Erro(int? codigo)
    {
        var status = codigo ?? 500;
        Response.StatusCode = status;

        return View("Error", new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
            Codigo = status
        });
    }
}
