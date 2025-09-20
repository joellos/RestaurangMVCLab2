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

        // GET: /Auth/Index (redirects to login)
        public IActionResult Index()
        {
            return RedirectToAction("Login");
        }

        // GET: /Auth/Login
        public IActionResult Login()
        {
            // Kolla om användaren redan är inloggad
            var existingToken = HttpContext.Session.GetString("JwtToken");
            if (!string.IsNullOrEmpty(existingToken))
            {
                TempData["InfoMessage"] = "Du är redan inloggad.";
                return RedirectToAction("Index", "Admin");
            }

            return View();
        }

        // POST: /Auth/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginDto model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _authService.LoginAsync(model);

            if (result.Succeeded)
            {
                var tokenResponse = result.GetData<TokenResponseDto>();
                if (tokenResponse != null)
                {
                    // Spara JWT token i session
                    HttpContext.Session.SetString("JwtToken", tokenResponse.AccessToken);

                    // Spara även refresh token för framtida användning
                    HttpContext.Session.SetString("RefreshToken", tokenResponse.RefreshToken);

                    // Spara admin info för visning
                    HttpContext.Session.SetString("AdminName", tokenResponse.Administrator.Name);
                    HttpContext.Session.SetString("AdminUsername", tokenResponse.Administrator.Username);

                    TempData["SuccessMessage"] = $"Välkommen, {tokenResponse.Administrator.Name}!";
                    return RedirectToAction("Index", "Admin");
                }
                else
                {
                    ModelState.AddModelError("", "Inloggning misslyckades - ogiltig svarsdata.");
                    return View(model);
                }
            }
            else
            {
                // Visa felmeddelande från service
                ModelState.AddModelError("", result.Message);
                return View(model);
            }
        }

        // POST: /Auth/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            try
            {
                var token = HttpContext.Session.GetString("JwtToken");

                // Försök logga ut via API (även om det misslyckas, rensa vi session lokalt)
                if (!string.IsNullOrEmpty(token))
                {
                    var result = await _authService.LogoutAsync(token);
                    // Vi bryr oss inte om resultatet här - rensa session ändå
                }

                // Rensa alla session-data
                HttpContext.Session.Remove("JwtToken");
                HttpContext.Session.Remove("RefreshToken");
                HttpContext.Session.Remove("AdminName");
                HttpContext.Session.Remove("AdminUsername");

                // Alternative: HttpContext.Session.Clear(); för att rensa allt

                TempData["SuccessMessage"] = "Du har loggats ut.";
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                // Även vid fel, rensa session
                HttpContext.Session.Clear();
                TempData["InfoMessage"] = "Du har loggats ut (med fel).";
                return RedirectToAction("Index", "Home");
            }
        }

        // GET: /Auth/Logout (för direkta länkar)
        public async Task<IActionResult> LogoutGet()
        {
            return await Logout();
        }

        // GET: /Auth/Profile (för framtida användning)
        public IActionResult Profile()
        {
            var token = HttpContext.Session.GetString("JwtToken");
            if (string.IsNullOrEmpty(token))
            {
                TempData["ErrorMessage"] = "Du måste logga in för att se din profil.";
                return RedirectToAction("Login");
            }

            // Hämta admin-info från session
            var adminName = HttpContext.Session.GetString("AdminName") ?? "Okänd";
            var adminUsername = HttpContext.Session.GetString("AdminUsername") ?? "Okänd";

            ViewBag.AdminName = adminName;
            ViewBag.AdminUsername = adminUsername;

            return View();
        }

        // POST: /Auth/RefreshToken (för framtida användning)
        [HttpPost]
        public async Task<IActionResult> RefreshToken()
        {
            try
            {
                var refreshToken = HttpContext.Session.GetString("RefreshToken");
                if (string.IsNullOrEmpty(refreshToken))
                {
                    return Json(new { success = false, message = "No refresh token available" });
                }

                var result = await _authService.RefreshTokenAsync(refreshToken);

                if (result.Succeeded)
                {
                    var tokenResponse = result.GetData<TokenResponseDto>();
                    if (tokenResponse != null)
                    {
                        // Uppdatera tokens i session
                        HttpContext.Session.SetString("JwtToken", tokenResponse.AccessToken);
                        HttpContext.Session.SetString("RefreshToken", tokenResponse.RefreshToken);

                        return Json(new { success = true, message = "Token refreshed successfully" });
                    }
                }

                return Json(new { success = false, message = result.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Token refresh failed" });
            }
        }

        // GET: /Auth/CheckAuth (för AJAX-anrop)
        [HttpGet]
        public IActionResult CheckAuth()
        {
            var token = HttpContext.Session.GetString("JwtToken");
            var isAuthenticated = !string.IsNullOrEmpty(token);

            return Json(new
            {
                isAuthenticated = isAuthenticated,
                adminName = isAuthenticated ? HttpContext.Session.GetString("AdminName") : null,
                adminUsername = isAuthenticated ? HttpContext.Session.GetString("AdminUsername") : null
            });
        }
    }
}