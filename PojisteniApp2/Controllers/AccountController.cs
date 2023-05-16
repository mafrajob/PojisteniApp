using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PojisteniApp2.Models;
using System.Security.Claims;
using static System.Net.WebRequestMethods;

namespace PojisteniApp2.Controllers
{
    // Based on tutorial:	https://www.itnetwork.cz/csharp/asp-net-core/zaklady/prihlaseni-a-registrace-v-aspnet-core-mvc
    //						https://www.itnetwork.cz/csharp/asp-net-core/zaklady/registrace-v-aspnet-core-mvc

    public class AccountController : Controller
	{
		private readonly UserManager<IdentityUser> _userManager;
		private readonly SignInManager<IdentityUser> _signInManager;
        private readonly INotyfService _notyf;

        public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, INotyfService notyf)
		{
			_userManager = userManager;
			_signInManager = signInManager;
            _notyf = notyf;
        }

        [HttpGet]
        public IActionResult Register(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                var user = new IdentityUser { UserName = model.Email, Email = model.Email };
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    _notyf.Success($"Registrován uživatel<br>{user.UserName}");
                    return RedirectToLocal(returnUrl);
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            _notyf.Error("Chyba registrace");
            return View(model);
        }

		[HttpGet]
		public IActionResult Login(string? returnUrl = null)
		{
			ViewData["ReturnUrl"] = returnUrl;
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
		{
			ViewData["ReturnUrl"] = returnUrl;
			if (ModelState.IsValid)
			{
				var validationResult = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
				if (validationResult.Succeeded)
				{
                    _notyf.Success($"Přihlášen uživatel<br>{model.Email}");
                    return RedirectToLocal(returnUrl);
				}
				else
				{
					ModelState.AddModelError(string.Empty, "Neplatné přihlašovací údaje");
                    _notyf.Error("Chyba přihlášení");
                    return View(model);
				}
			}

            // Back to login form if invalid values provided
            _notyf.Error("Chyba přihlášení");
            return View(model);
		}

		public async Task<IActionResult> Logout()
		{
			await _signInManager.SignOutAsync();
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            string message = (userId != null) ? string.Format($"Odhlášen uživatel<br>{await _userManager.FindByIdAsync(userId)}") : "Uživatel odhlášen";
            _notyf.Success(message);
            return RedirectToAction("Index", "Home");
		}

		private IActionResult RedirectToLocal(string? returnUrl)
		{
			if (Url.IsLocalUrl(returnUrl))
			{
				return Redirect(returnUrl);
			}
			else
			{
				return RedirectToAction("Index", "Home");
			}
		}
    }
}