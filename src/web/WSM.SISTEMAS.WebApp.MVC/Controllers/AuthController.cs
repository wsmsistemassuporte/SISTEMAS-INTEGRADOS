using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WSM.SISTEMAS.WebApp.MVC.Models.Auth;
using WSM.SISTEMAS.WebApp.MVC.Services;

namespace WSM.SISTEMAS.WebApp.MVC.Controllers
{
    public class AuthController : MainController
    {
        private readonly IAutenticarService _autenticarApiClient;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAutenticarService autenticarApiClient,ILogger<AuthController> logger)
        {
            _autenticarApiClient = autenticarApiClient;
            _logger = logger;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null,CancellationToken cancellationToken=default)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            ViewData["ReturnUrl"] = returnUrl;

            var result = await _autenticarApiClient.LoginAsync(model.Email, model.Password, cancellationToken);
            if (!result.Succeeded)
            {
                AdicionarErros(result);
                return View(model);
            }

            await CookieAuthService.SignInAsync(HttpContext, result, model.RememberMe, _logger);

            if (string.IsNullOrEmpty(returnUrl)) return RedirectToAction("Index", "Home");

            return LocalRedirect(returnUrl);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }

            return View(new RegisterViewModel());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _autenticarApiClient.RegisterAsync(
                model.Email,
                model.Password,
                model.ConfirmPassword,
                model.Nome,
                cancellationToken);

            if (!result.Succeeded)
            {
                AdicionarErros(result);
                return View(model);
            }

            await CookieAuthService.SignInAsync(HttpContext, result, isPersistent: false, _logger);

            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout(CancellationToken cancellationToken)
        {
            var accessToken = CookieAuthService.GetJwt(User);
            var refreshToken = CookieAuthService.GetRefreshToken(User);

            if (!string.IsNullOrWhiteSpace(accessToken) || !string.IsNullOrWhiteSpace(refreshToken))
            {
                await _autenticarApiClient.LogoutAsync(
                    accessToken ?? string.Empty,
                    refreshToken ?? string.Empty,
                    cancellationToken);
            }

            await CookieAuthService.SignOutAsync(HttpContext);

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

    }
}
