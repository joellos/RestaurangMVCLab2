using Microsoft.AspNetCore.Mvc;

namespace RestaurangMVCLab2.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            // Kolla om användaren är inloggad
            var token = HttpContext.Session.GetString("JwtToken");
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Auth");
            }

            return View();
        }
    }
}