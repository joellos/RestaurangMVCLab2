
using Microsoft.AspNetCore.Mvc;

namespace RestaurangMVCLab2.Controllers
{
    public class BookingPageController : Controller
    {
        // GET: /BookingPage
        // Detta är en proxy till React-appen
        public IActionResult Index()
        {
            // Om React-appen körs på en annan port (t.ex. 5173 för Vite)
            // kan vi redirecta dit, eller så kan vi servera den byggda React-appen

            // För utveckling: Redirect till Vite dev server
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
            {
                return Redirect("http://localhost:5173");
            }

            // För produktion: Servera byggd React-app från wwwroot
            return View();
        }
    }
}