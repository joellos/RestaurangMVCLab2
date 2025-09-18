using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using RestaurangMVCLab2.DTOs;
using RestaurangMVCLab2.Models;
using RestaurangMVCLab2.Services;

namespace RestaurangMVCLab2.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly MenuService _menuService; 

 
        public HomeController(ILogger<HomeController> logger, MenuService menuService )
        {
            _logger = logger;
            _menuService = menuService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                // 1. Hämta populära rätter (inga villkor behövs)
                var popularDishes = await _menuService.GetPopularDishes();

                // 2. Skicka till view
                return View(popularDishes);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Hemma sidan kunde ej laddas, försök senare.";
                return View(new List<MenuItemResponseDto>());
            }
        }

        public IActionResult Privacy()
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
