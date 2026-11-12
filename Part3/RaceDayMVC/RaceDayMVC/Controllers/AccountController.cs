
using Microsoft.AspNetCore.Mvc;
using RaceDayMVC.Services;
using RaceDayMVC.ViewModels;

namespace RaceDayMVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly AuthApiService _authService;

        public AccountController(AuthApiService authService)
        {
            _authService = authService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (HttpContext.Session.GetInt32("UserID") != null)
                return RedirectToAction("Index", "Home");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _authService.LoginAsync(
                model.Email,
                model.Password);

            if (!result.Success)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Login failed. Check your email and password.");

                return View(model);
            }

            string? role = HttpContext.Session.GetString("Role");

            if (role == "Organiser")
                return RedirectToAction("Index", "Organiser");

            if (role == "Participant")
                return RedirectToAction("Index", "Participant");

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _authService.RegisterAsync(
                model.FirstName,
                model.LastName,
                model.Email,
                model.Password,
                model.RoleID,
                model.PhoneNumber);

            if (!result.Success)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Registration failed. This email may already be registered.");

                return View(model);
            }

            TempData["SuccessMessage"] =
                "Registration successful! You can now log in.";

            return RedirectToAction("Login");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _authService.LogoutAsync();

            return RedirectToAction("Login");
        }
    }
}
