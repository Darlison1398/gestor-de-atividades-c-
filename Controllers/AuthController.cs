using GestorAtividades.Exceptions;
using GestorAtividades.Helper;
using GestorAtividades.Service;
using GestorAtividades.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestorAtividades.Controllers
{
    /// <summary>
    /// Login, cadastro, recuperação de senha e logout. Os formulários ficam na Home
    /// (partials _LoginForm, _RegisterForm, _ForgotPasswordForm); ViewBag.ActiveForm diz qual reabrir.
    /// </summary>
    public class AuthController : BaseController
    {
        private const string HomeView = "~/Views/Home/Index.cshtml";

        private readonly UsuarioService _usuarioService;

        public AuthController(UsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
        }

        // Destino do LoginPath do cookie: quem acessa uma página protegida sem login cai aqui.
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Atividades");

            return RedirectToAction("Index", "Home", new { returnUrl });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel form)
        {
            var usuario = await _usuarioService.AutenticarAsync(form.Email, form.Senha);

            if (usuario == null)
            {
                ViewBag.Error = "Email ou senha inválidos.";
                ViewBag.ReturnUrl = form.ReturnUrl;
                return View(HomeView);
            }

            await HttpContext.SignInAsync(Autenticacao.Esquema, Autenticacao.CriarPrincipal(usuario));

            if (!string.IsNullOrEmpty(form.ReturnUrl) && Url.IsLocalUrl(form.ReturnUrl))
                return LocalRedirect(form.ReturnUrl);

            return RedirectToAction("Index", "Atividades");
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registrar(RegistrarUsuarioViewModel form)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _usuarioService.RegistrarAsync(form);
                    TempData["SuccessMessage"] = "Conta criada com sucesso! Faça login para começar.";
                    return RedirectToAction("Index", "Home");
                }
                catch (RegraNegocioException ex)
                {
                    ModelState.AddModelError(nameof(form.Email), ex.Message);
                }
            }

            ViewBag.ActiveForm = "register";
            return View(HomeView, form);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public IActionResult EsqueciSenha(string email)
        {
            // TODO: gerar token, enviar email com link de redefinição.
            ViewBag.ForgotMessage = "Se o email existir, enviaremos um link de recuperação.";
            ViewBag.ActiveForm = "forgot";
            return View(HomeView);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(Autenticacao.Esquema);
            return RedirectToAction("Index", "Home");
        }
    }
}
