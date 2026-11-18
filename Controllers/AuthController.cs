using GestorAtividades.Data;
using GestorAtividades.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using GestorAtividades.Service;
using Microsoft.AspNetCore.Authorization;
using GestorAtividades.Dto;


namespace GestorAtividades.Controllers
{

    [Authorize(AuthenticationSchemes = "CookieAuth")]
    public class AuthController : BaseController
    {
        // Login, cadastro e recuperação de senha ficam na Home (partials _LoginForm, _RegisterForm, _ForgotPasswordForm).
        private const string HomeView = "~/Views/Home/Index.cshtml";

        private readonly ApplicationDbContext _context;

        private readonly UsuarioService _usuarioService;

        public AuthController(ApplicationDbContext context, UsuarioService usuarioService)
        {
            _context = context;
            _usuarioService = usuarioService;
        }

        // Destino do LoginPath do cookie: quem acessa uma página protegida sem login cai aqui.
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Main", "Pages");

            return RedirectToAction("Index", "Home", new { returnUrl });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string senha, string? returnUrl = null)
        {
            var user = await _context.Usuarios
                .FirstOrDefaultAsync(x => x.Email == email);

            if (user == null || user.SenhaHash == null || !BCrypt.Net.BCrypt.Verify(senha, user.SenhaHash))
            {
                ViewBag.Error = "Credenciais inválidas!";
                ViewBag.ReturnUrl = returnUrl;
                return View(HomeView);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Nome ?? string.Empty),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
            };

            var claimsIdentity = new ClaimsIdentity(claims, "CookieAuth");

            await HttpContext.SignInAsync("CookieAuth",
                new ClaimsPrincipal(claimsIdentity));

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return LocalRedirect(returnUrl);

            return RedirectToAction("Main", "Pages");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("CookieAuth");
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUsuario(Usuario usuario)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ActiveForm = "register";
                return View(HomeView, usuario);
            }

            try
            {
                await _usuarioService.Register(usuario);
                TempData["SuccessMessage"] = "Usuário registrado com sucesso!";
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                ViewBag.ActiveForm = "register";
                return View(HomeView, usuario);
            }
        }

        public async Task<IActionResult> Perfil()
        {
            try
            {
                if (UserId == 0)
                    return Unauthorized();

                var usuario = await _usuarioService.GetUsuarioByIdAsync(UserId);
                if (usuario == null)
                    return NotFound();

                return View(usuario);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Main", "Pages");
            }
        }

        public async Task<IActionResult> EditarUsuario()
        {
            try
            {
                if (UserId == 0)
                    return Unauthorized();

                var usuario = await _usuarioService.GetUsuarioByIdAsync(UserId);
                if (usuario == null)
                    return NotFound();

                var dto = new UsuarioDto
                {
                    Nome = usuario.Nome ?? string.Empty,
                    Email = usuario.Email ?? string.Empty
                };

                return View(dto);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction(nameof(Perfil));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateUsuario(UsuarioDto usuarioDto)
        {
            if (UserId == 0)
                return Unauthorized();

            if (!ModelState.IsValid)
                return View("EditarUsuario", usuarioDto);

            try
            {
                await _usuarioService.Editar(UserId, usuarioDto);
                TempData["SuccessMessage"] = "Perfil atualizado com sucesso!";
                return RedirectToAction(nameof(Perfil));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return View("EditarUsuario", usuarioDto);
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public IActionResult ForgotPassword(string email)
        {
            // aqui futuramente você:
            // - valida email
            // - gera token
            // - envia email

            ViewBag.ForgotMessage = "Se o email existir, enviaremos um link de recuperação.";
            ViewBag.ActiveForm = "forgot";
            return View(HomeView);
        }

    }
}
