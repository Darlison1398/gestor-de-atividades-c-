using GestorAtividades.Data;
using GestorAtividades.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;        
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using GestorAtividades.Service;


namespace GestorAtividades.Controllers 
{
    public class AuthController : Controller
    {

        private readonly ApplicationDbContext _context;

        private readonly UsuarioService _usuarioService;

        public AuthController(ApplicationDbContext context, UsuarioService usuarioService)
        {
            _context = context;
            _usuarioService = usuarioService;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string senha)
        {
            var user = await _context.Usuarios
                .FirstOrDefaultAsync(x => x.Email == email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(senha, user.SenhaHash))
            {
                ViewBag.Error = "Credenciais inválidas!";
                return View();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Nome),
                new Claim(ClaimTypes.Email, user.Email),
            };

            var claimsIdentity = new ClaimsIdentity(claims, "CookieAuth");

            await HttpContext.SignInAsync("CookieAuth",
                new ClaimsPrincipal(claimsIdentity));

            return RedirectToAction("Main", "Pages");
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("CookieAuth");
            return RedirectToAction("Login");
        }

        public IActionResult CreateUser()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateUsuario(Usuario usuario)
        {
            if (!ModelState.IsValid)
                return View("CreateUser", usuario);

            try
            {
                await _usuarioService.Register(usuario);
                TempData["SuccessMessage"] = "Usuário registrado com sucesso!";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                //ModelState.AddModelError("", ex.Message);
                return View("CreateUser");
            }
        }

    }
}