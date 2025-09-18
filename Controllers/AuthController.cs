using Microsoft.AspNetCore.Mvc;
using RestaurangMVCLab2.DTOs;
using RestaurangMVCLab2.Services;

namespace RestaurangMVCLab2.Controllers
{
    public class AuthController : Controller
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        public IActionResult Index()
        {


            return View();
        }

        public async Task<IActionResult> Login(LoginDto model)
        {
            var result = await _authService.LoginAsync(model);

            if (result != null)
            {
                // DENNA rad kommer vi skriva:
                HttpContext.Session.SetString("JwtToken", result.AccessToken);
                return RedirectToAction("Index", "Admin");
            }

            return View(model); // Visa fel
        }
    }
}
