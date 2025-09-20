using Microsoft.AspNetCore.Mvc;
using RestaurangMVCLab2.Services;
using RestaurangMVCLab2.DTOs;

namespace RestaurangMVCLab2.Controllers
{
    public class AdminController : Controller
    {
        private readonly MenuService _menuService;
        private readonly TableService _tableService;
        private readonly BookingService _bookingService;

        public AdminController(MenuService menuService, TableService tableService, BookingService bookingService)
        {
            _menuService = menuService;
            _tableService = tableService;
            _bookingService = bookingService;
        }

        // Helper method för admin-autentisering (samma pattern som andra controllers)
        private bool IsAdminAuthenticated()
        {
            var token = HttpContext.Session.GetString("JwtToken");
            if (string.IsNullOrEmpty(token)) return false;

            // Sätt auth token för alla services
            _menuService.SetAuthToken(token);
            _tableService.SetAuthToken(token);
            _bookingService.SetAuthToken(token);

            return true;
        }

        public async Task<IActionResult> Index()
        {
            // Kolla om användaren är inloggad
            if (!IsAdminAuthenticated())
            {
                TempData["ErrorMessage"] = "Du måste logga in som administratör.";
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                // Hämta data från alla services parallellt för bättre prestanda
                var tablesTask = _tableService.GetAllTablesAsync();
                var menuItemsTask = _menuService.GetAllMenuItemsForAdminAsync();
                var bookingsTask = _bookingService.GetAllBookingsAsync();

                await Task.WhenAll(tablesTask, menuItemsTask, bookingsTask);

                var tablesResult = await tablesTask;
                var menuItemsResult = await menuItemsTask;
                var bookingsResult = await bookingsTask;

                // Hantera resultaten
                var tables = new List<TableResponseDto>();
                var menuItems = new List<MenuItemResponseDto>();
                var bookings = new List<BookingResponseDto>();
                var errors = new List<string>();

                if (tablesResult.Succeeded)
                {
                    tables = tablesResult.GetData<List<TableResponseDto>>() ?? new List<TableResponseDto>();
                }
                else
                {
                    errors.Add($"Tables: {tablesResult.Message}");
                }

                if (menuItemsResult.Succeeded)
                {
                    menuItems = menuItemsResult.GetData<List<MenuItemResponseDto>>() ?? new List<MenuItemResponseDto>();
                }
                else
                {
                    errors.Add($"Menu: {menuItemsResult.Message}");
                }

                if (bookingsResult.Succeeded)
                {
                    bookings = bookingsResult.GetData<List<BookingResponseDto>>() ?? new List<BookingResponseDto>();
                }
                else
                {
                    errors.Add($"Bookings: {bookingsResult.Message}");
                }

                // Beräkna statistik
                var stats = new
                {
                    // Table statistics
                    TotalTables = tables.Count,
                    ActiveTables = tables.Count(t => t.IsActive),
                    InactiveTables = tables.Count(t => !t.IsActive),
                    TotalCapacity = tables.Sum(t => t.Capacity),

                    // Menu statistics
                    TotalMenuItems = menuItems.Count,
                    AvailableMenuItems = menuItems.Count(m => m.IsAvailable),
                    PopularMenuItems = menuItems.Count(m => m.IsPopular),
                    UnavailableMenuItems = menuItems.Count(m => !m.IsAvailable),

                    // Booking statistics
                    TotalBookings = bookings.Count,
                    TodayBookings = bookings.Count(b => b.BookingDateTime.Date == DateTime.Today),
                    UpcomingBookings = bookings.Count(b => b.BookingDateTime > DateTime.Now),
                    PastBookings = bookings.Count(b => b.BookingDateTime < DateTime.Now),
                    TotalGuests = bookings.Sum(b => b.NumberOfGuests),

                    // Additional statistics
                    AverageTableCapacity = tables.Any() ? Math.Round(tables.Average(t => t.Capacity), 1) : 0,
                    AverageMenuPrice = menuItems.Any() ? Math.Round(menuItems.Average(m => m.Price), 2) : 0,
                    MostPopularCategory = menuItems
                        .GroupBy(m => m.Category)
                        .OrderByDescending(g => g.Count())
                        .FirstOrDefault()?.Key ?? "Ingen",

                    // Current status
                    TablesWithBookings = tables.Count(t => t.CurrentBookingsCount > 0),
                    EmptyTables = tables.Count(t => t.CurrentBookingsCount == 0 && t.IsActive)
                };

                ViewBag.Statistics = stats;

                // Visa eventuella fel som varningar
                if (errors.Any())
                {
                    ViewBag.WarningMessage = $"Vissa data kunde inte hämtas: {string.Join(", ", errors)}";
                }

                // Visa admin-info
                ViewBag.AdminName = HttpContext.Session.GetString("AdminName") ?? "Administratör";
                ViewBag.AdminUsername = HttpContext.Session.GetString("AdminUsername") ?? "admin";

                return View();
            }
            catch (Exception ex)
            {
                // Fallback om allt går fel
                ViewBag.Statistics = new
                {
                    TotalTables = 0,
                    ActiveTables = 0,
                    InactiveTables = 0,
                    TotalCapacity = 0,
                    TotalMenuItems = 0,
                    AvailableMenuItems = 0,
                    PopularMenuItems = 0,
                    UnavailableMenuItems = 0,
                    TotalBookings = 0,
                    TodayBookings = 0,
                    UpcomingBookings = 0,
                    PastBookings = 0,
                    TotalGuests = 0,
                    AverageTableCapacity = 0,
                    AverageMenuPrice = 0,
                    MostPopularCategory = "Ingen",
                    TablesWithBookings = 0,
                    EmptyTables = 0
                };

                ViewBag.ErrorMessage = $"Kunde inte hämta dashboard-data: {ex.Message}";
                ViewBag.AdminName = HttpContext.Session.GetString("AdminName") ?? "Administratör";
                ViewBag.AdminUsername = HttpContext.Session.GetString("AdminUsername") ?? "admin";

                return View();
            }
        }

        // GET: /Admin/Stats (för AJAX-anrop till dashboard-data)
        [HttpGet]
        public async Task<IActionResult> Stats()
        {
            if (!IsAdminAuthenticated())
            {
                return Json(new { success = false, message = "Not authenticated" });
            }

            try
            {
                // Samma logik som Index men returnera JSON
                var tablesTask = _tableService.GetAllTablesAsync();
                var menuItemsTask = _menuService.GetAllMenuItemsForAdminAsync();
                var bookingsTask = _bookingService.GetAllBookingsAsync();

                await Task.WhenAll(tablesTask, menuItemsTask, bookingsTask);

                var tablesResult = await tablesTask;
                var menuItemsResult = await menuItemsTask;
                var bookingsResult = await bookingsTask;

                var tables = tablesResult.Succeeded ?
                    tablesResult.GetData<List<TableResponseDto>>() ?? new List<TableResponseDto>() :
                    new List<TableResponseDto>();

                var menuItems = menuItemsResult.Succeeded ?
                    menuItemsResult.GetData<List<MenuItemResponseDto>>() ?? new List<MenuItemResponseDto>() :
                    new List<MenuItemResponseDto>();

                var bookings = bookingsResult.Succeeded ?
                    bookingsResult.GetData<List<BookingResponseDto>>() ?? new List<BookingResponseDto>() :
                    new List<BookingResponseDto>();

                var stats = new
                {
                    tables = new
                    {
                        total = tables.Count,
                        active = tables.Count(t => t.IsActive),
                        inactive = tables.Count(t => !t.IsActive),
                        totalCapacity = tables.Sum(t => t.Capacity)
                    },
                    menu = new
                    {
                        total = menuItems.Count,
                        available = menuItems.Count(m => m.IsAvailable),
                        popular = menuItems.Count(m => m.IsPopular)
                    },
                    bookings = new
                    {
                        total = bookings.Count,
                        today = bookings.Count(b => b.BookingDateTime.Date == DateTime.Today),
                        upcoming = bookings.Count(b => b.BookingDateTime > DateTime.Now)
                    }
                };

                return Json(new { success = true, data = stats });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}