using Microsoft.AspNetCore.Mvc;
using RestaurangMVCLab2.DTOs;
using RestaurangMVCLab2.Models;
using RestaurangMVCLab2.Services;

namespace RestaurangMVCLab2.Controllers
{
    public class MenuController : Controller
    {
        private readonly MenuService _menuService;

        public MenuController(MenuService menuService)
        {
            _menuService = menuService;
        }

        // Helper method för admin-autentisering (samma pattern som BookingController)
        private bool IsAdminAuthenticated()
        {
            var token = HttpContext.Session.GetString("JwtToken");
            if (string.IsNullOrEmpty(token)) return false;

            _menuService.SetAuthToken(token);
            return true;
        }

        // GET: /Menu
        public async Task<IActionResult> Index(string? search, string? category)
        {
            try
            {
                ServiceResponse result;

                // Bestäm vilken metod som ska anropas
                if (!string.IsNullOrEmpty(search))
                {
                    // Om det finns sökterm - använd search
                    result = await _menuService.SearchMenuItemsAsync(search);
                }
                else if (!string.IsNullOrEmpty(category))
                {
                    // Om det finns kategori - filtrera på kategori  
                    result = await _menuService.GetMenuItemByCategoryAsync(category);
                }
                else
                {
                    // Annars - visa alla (eller alla för admin)
                    if (IsAdminAuthenticated())
                    {
                        result = await _menuService.GetAllMenuItemsForAdminAsync();
                    }
                    else
                    {
                        result = await _menuService.GetMenuItemsAsync();
                    }
                }

                List<MenuItemResponseDto> menuItems;
                if (result.Succeeded)
                {
                    menuItems = result.GetData<List<MenuItemResponseDto>>() ?? new List<MenuItemResponseDto>();
                    ViewBag.SuccessMessage = result.Message;
                }
                else
                {
                    menuItems = new List<MenuItemResponseDto>();
                    ViewBag.ErrorMessage = result.Message;
                }

                // Hämta kategorier för dropdown
                var categoriesResult = await _menuService.GetCategoriesAsync();
                if (categoriesResult.Succeeded)
                {
                    ViewBag.Categories = categoriesResult.GetData<List<string>>() ?? new List<string>();
                }

                // Skicka data till vyn
                ViewBag.CurrentSearch = search;
                ViewBag.CurrentCategory = category;
                return View(menuItems);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Menyn kunde ej laddas, försök senare.";
                return View(new List<MenuItemResponseDto>());
            }
        }

        // CREATE - GET
        public async Task<IActionResult> Create()
        {
            if (!IsAdminAuthenticated())
            {
                TempData["ErrorMessage"] = "Du måste logga in som administratör.";
                return RedirectToAction("Login", "Auth");
            }

            // Hämta kategorier för dropdown
            var categoriesResult = await _menuService.GetCategoriesAsync();
            if (categoriesResult.Succeeded)
            {
                ViewBag.Categories = categoriesResult.GetData<List<string>>() ?? new List<string>();
            }

            return View();
        }

        // CREATE - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateMenuItemDto dto)
        {
            if (!IsAdminAuthenticated())
            {
                TempData["ErrorMessage"] = "Du måste logga in som administratör.";
                return RedirectToAction("Login", "Auth");
            }

            if (!ModelState.IsValid)
            {
                // Hämta kategorier igen för dropdown vid fel
                var categoriesResult = await _menuService.GetCategoriesAsync();
                if (categoriesResult.Succeeded)
                {
                    ViewBag.Categories = categoriesResult.GetData<List<string>>() ?? new List<string>();
                }
                return View(dto);
            }

            var result = await _menuService.CreateMenuItemAsync(dto);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = result.Message;
                return RedirectToAction("Index");
            }
            else
            {
                TempData["ErrorMessage"] = result.Message;
                // Hämta kategorier igen för dropdown vid fel
                var categoriesResult = await _menuService.GetCategoriesAsync();
                if (categoriesResult.Succeeded)
                {
                    ViewBag.Categories = categoriesResult.GetData<List<string>>() ?? new List<string>();
                }
                return View(dto);
            }
        }

        // EDIT - GET
        public async Task<IActionResult> Edit(int id)
        {
            if (!IsAdminAuthenticated())
            {
                TempData["ErrorMessage"] = "Du måste logga in som administratör.";
                return RedirectToAction("Login", "Auth");
            }

            var result = await _menuService.GetMenuItemByIdAsync(id);
            if (!result.Succeeded)
            {
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction("Index");
            }

            var menuItem = result.GetData<MenuItemResponseDto>();
            if (menuItem == null)
            {
                TempData["ErrorMessage"] = "Rätten kunde inte hittas.";
                return RedirectToAction("Index");
            }

            var updateDto = new UpdateMenuItemDto
            {
                Name = menuItem.Name,
                Description = menuItem.Description,
                Price = menuItem.Price,
                Category = menuItem.Category,
                ImageUrl = menuItem.ImageUrl,
                IsPopular = menuItem.IsPopular,
                IsAvailable = menuItem.IsAvailable
            };

            ViewBag.MenuItemId = id;

            // Hämta kategorier för dropdown
            var categoriesResult = await _menuService.GetCategoriesAsync();
            if (categoriesResult.Succeeded)
            {
                ViewBag.Categories = categoriesResult.GetData<List<string>>() ?? new List<string>();
            }

            return View(updateDto);
        }

        // EDIT - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdateMenuItemDto dto)
        {
            if (!IsAdminAuthenticated())
            {
                TempData["ErrorMessage"] = "Du måste logga in som administratör.";
                return RedirectToAction("Login", "Auth");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.MenuItemId = id;
                // Hämta kategorier igen för dropdown vid fel
                var categoriesResult = await _menuService.GetCategoriesAsync();
                if (categoriesResult.Succeeded)
                {
                    ViewBag.Categories = categoriesResult.GetData<List<string>>() ?? new List<string>();
                }
                return View(dto);
            }

            var result = await _menuService.UpdateMenuItemAsync(id, dto);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = result.Message;
                return RedirectToAction("Index");
            }
            else
            {
                TempData["ErrorMessage"] = result.Message;
                ViewBag.MenuItemId = id;
                // Hämta kategorier igen för dropdown vid fel
                var categoriesResult = await _menuService.GetCategoriesAsync();
                if (categoriesResult.Succeeded)
                {
                    ViewBag.Categories = categoriesResult.GetData<List<string>>() ?? new List<string>();
                }
                return View(dto);
            }
        }

        // DELETE
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (!IsAdminAuthenticated())
            {
                TempData["ErrorMessage"] = "Du måste logga in som administratör.";
                return RedirectToAction("Login", "Auth");
            }

            var result = await _menuService.DeleteMenuItemAsync(id);

            TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"] = result.Message;
            return RedirectToAction("Index");
        }

        // TOGGLE POPULAR
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TogglePopular(int id)
        {
            if (!IsAdminAuthenticated())
            {
                TempData["ErrorMessage"] = "Du måste logga in som administratör.";
                return RedirectToAction("Login", "Auth");
            }

            var result = await _menuService.TogglePopularAsync(id);

            TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"] = result.Message;
            return RedirectToAction("Index");
        }
    }
}