using GestorAtividades.Data;
using GestorAtividades.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;        
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using GestorAtividades.Service;
using Microsoft.AspNetCore.Authorization;
using GestorAtividades.Dto;


namespace GestorAtividades.Controllers 
{

    [Authorize(AuthenticationSchemes = "CookieAuth")]
    public class AuthController : BaseController
    {

        private readonly ApplicationDbContext _context;

        private readonly UsuarioService _usuarioService;

        public AuthController(ApplicationDbContext context, UsuarioService usuarioService)
        {
            _context = context;
            _usuarioService = usuarioService;
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
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
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUsuario(Usuario usuario)
        {
            if (!ModelState.IsValid)
                return View("Home", usuario);

            try
            {
                await _usuarioService.Register(usuario);
                TempData["SuccessMessage"] = "Usuário registrado com sucesso!";
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return View("Home");
            }
        }

        [Authorize(AuthenticationSchemes = "CookieAuth")]
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
                return View("Login");
            }
        }

        [Authorize(AuthenticationSchemes = "CookieAuth")]
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
                    Nome = usuario.Nome,
                    Email = usuario.Email
                };

                return View(dto);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return View("Login");
            }
            
        }

        [Authorize(AuthenticationSchemes = "CookieAuth")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateUsuario(UsuarioDto usuarioDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return View("EditarUsuario", usuarioDto);
             
                if (UserId == 0)
                    return Unauthorized();

                await _usuarioService.Editar(UserId, usuarioDto);
                TempData["SuccessMessage"] = "Perfil atualizado com sucesso!";
                return RedirectToAction(nameof(Perfil));
                
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return View("Login");
            }
        }


        public async Task<IActionResult> ForgotPassword(string email)
        {
            // aqui futuramente você:
            // - valida email
            // - gera token
            // - envia email

            ViewBag.ForgotMessage = "Se o email existir, enviaremos um link de recuperação.";
            return View("~/Views/Home/Index.cshtml");
        }

    }
}