using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PIM_WebApplication.Services;
using PIM_WebApplication.ViewModels;

namespace PIM_WebApplication.Controllers
{
    public class ContaController : Controller
    {
        private readonly IUsuarioService _usuarioService;

        public ContaController(IUsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var cliente = await _usuarioService.AutenticarAsync(model.Email, model.Senha);
            if (cliente == null)
            {
                ModelState.AddModelError(string.Empty, "E-mail ou senha incorretos.");
                return View(model);
            }

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, cliente.IdCliente.ToString()),
                new(ClaimTypes.Name, cliente.Nome),
                new(ClaimTypes.Email, cliente.Email),
                new(ClaimTypes.Role, cliente.IsAdmin ? "Admin" : "Cliente")
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            TempData["MensagemToast"] = $"Bem-vindo de volta, {cliente.Nome}!";

            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            {
                return Redirect(model.ReturnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Registrar()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            return View(new RegistroViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registrar(RegistroViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var (sucesso, mensagem, novoCliente) = await _usuarioService.RegistrarAsync(
                model.Nome,
                model.Email,
                model.Senha,
                model.Telefone);

            if (!sucesso)
            {
                ModelState.AddModelError(string.Empty, mensagem);
                return View(model);
            }

            TempData["MensagemToast"] = "Conta criada com sucesso! Faça seu login para continuar.";
            return RedirectToAction(nameof(Login));
        }

        [HttpPost]
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();
            TempData["MensagemToast"] = "Você encerrou sua sessão com sucesso.";
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult AcessoNegado()
        {
            return View();
        }
    }
}
