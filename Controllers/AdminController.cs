// Controllers/AdminController.cs - ENKEL FIX
using Microsoft.AspNetCore.Mvc;
using RestaurangMVCLab2.Services;


namespace RestaurangMVCLab2.Controllers
{
    public class AdminController : Controller
    {
        private readonly MenuService _menuService;
        private readonly TableService _tableService;
        // Lägg till BookingService när du har skapat den

        public AdminController(MenuService menuService, TableService tableService)
        {
            _menuService = menuService;
            _tableService = tableService;
        }

        public async Task<IActionResult> Index()
        {
            // Kolla om användaren är inloggad
            var token = HttpContext.Session.GetString("JwtToken");
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                // Sätt auth token
                _tableService.SetAuthToken(token);

                // Hämta LIVE data från API:erna
                var tables = await _tableService.GetAllTablesAsync();
                var menuItems = await _menuService.GetAllMenuItemsForAdminAsync(token); // Hämta ALLA rätter för admin

                // Beräkna statistik
                var stats = new
                {
                    TotalTables = tables.Count,
                    ActiveTables = tables.Count(t => t.IsActive),
                    InactiveTables = tables.Count(t => !t.IsActive),
                    TotalCapacity = tables.Sum(t => t.Capacity),

                    TotalMenuItems = menuItems.Count,
                    AvailableMenuItems = menuItems.Count(m => m.IsAvailable),
                    PopularMenuItems = menuItems.Count(m => m.IsPopular),

                    // Lägg till bokningsstats när du har BookingService
                    TotalBookings = 0, // await _bookingService.GetAllBookingsAsync().Count
                    TodayBookings = 0
                };

                ViewBag.Statistics = stats;
                return View();
            }
            catch (Exception ex)
            {
                // Fallback om API inte fungerar
                ViewBag.Statistics = new
                {
                    TotalTables = 0,
                    ActiveTables = 0,
                    InactiveTables = 0,
                    TotalCapacity = 0,
                    TotalMenuItems = 0,
                    AvailableMenuItems = 0,
                    PopularMenuItems = 0,
                    TotalBookings = 0,
                    TodayBookings = 0
                };
                ViewBag.ErrorMessage = "Kunde inte hämta statistik: " + ex.Message;
                return View();
            }
        }
    }
}