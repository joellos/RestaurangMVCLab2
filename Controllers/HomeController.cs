using Microsoft.AspNetCore.Mvc;
using RestaurangMVCLab2.DTOs;
using RestaurangMVCLab2.Models;
using RestaurangMVCLab2.Services;
using System.Diagnostics;

namespace RestaurangMVCLab2.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly MenuService _menuService;

        public HomeController(ILogger<HomeController> logger, MenuService menuService)
        {
            _logger = logger;
            _menuService = menuService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                // Hämta populära rätter för startsidan
                var result = await _menuService.GetPopularDishesAsync();

                List<MenuItemResponseDto> popularItems;
                if (result.Succeeded)
                {
                    popularItems = result.GetData<List<MenuItemResponseDto>>() ?? new List<MenuItemResponseDto>();
                    _logger.LogInformation("Successfully loaded {Count} popular dishes for home page", popularItems.Count);
                }
                else
                {
                    _logger.LogWarning("Failed to load popular dishes: {Message}", result.Message);
                    popularItems = new List<MenuItemResponseDto>();
                    ViewBag.WarningMessage = "Kunde inte hämta populära rätter för tillfället.";
                }

                // Begränsa till max 6 rätter för startsidan
                var displayItems = popularItems.Take(6).ToList();

                return View(displayItems);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading home page");
                ViewBag.ErrorMessage = "Startsidan kunde inte laddas korrekt.";
                return View(new List<MenuItemResponseDto>());
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        // Om kontakt-sida behövs i framtiden
        public IActionResult Contact()
        {
            return View();
        }

        // Om om oss-sida behövs i framtiden
        public IActionResult About()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}