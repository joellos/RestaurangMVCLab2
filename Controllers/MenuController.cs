using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using RestaurangMVCLab2.DTOs;
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

        public async Task<IActionResult> Index(string? search, string? category)
        {
            try
            {
                List<MenuItemResponseDto> menuItems;
                // Bestäm vilken metod som ska anropas
                if (!string.IsNullOrEmpty(search))
                {
                    // Om det finns sökterm - använd search
                    menuItems = await _menuService.SearchMenuItemsAsync(search);
                }
                else if (!string.IsNullOrEmpty(category))
                {
                    // Om det finns kategori - filtrera på kategori  
                    menuItems = await _menuService.GetMenuItemByCategoryAsync(category);
                }
                else
                {
                    // Annars - visa alla (som förut)
                    menuItems = await _menuService.GetMenuItemsAsync();
                }
                // Hämta kategorier för dropdown
                var categories = await _menuService.GetCategoriesAsync();
                // Skicka data till vyn
                ViewBag.Categories = categories;
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

        // ============ ADMIN METHODS - LÄGG TILL DESSA ============

        // CREATE - GET
        public async Task<IActionResult> Create()
        {
            if (!IsAdmin()) return RedirectToAction("Index");

            var categories = await _menuService.GetCategoriesAsync();
            ViewBag.Categories = categories;
            return View();
        }

        // CREATE - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateMenuItemDto dto)
        {
            if (!IsAdmin()) return RedirectToAction("Index");

            if (!ModelState.IsValid)
            {
                var categories = await _menuService.GetCategoriesAsync();
                ViewBag.Categories = categories;
                return View(dto);
            }

            try
            {
                var token = HttpContext.Session.GetString("JwtToken");
                var result = await _menuService.CreateMenuItemAsync(dto, token!);

                if (result != null)
                {
                    TempData["SuccessMessage"] = "Rätten har lagts till!";
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError("", "Kunde inte lägga till rätten.");
                    var categories = await _menuService.GetCategoriesAsync();
                    ViewBag.Categories = categories;
                    return View(dto);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Ett fel uppstod: " + ex.Message);
                var categories = await _menuService.GetCategoriesAsync();
                ViewBag.Categories = categories;
                return View(dto);
            }
        }

        // EDIT - GET
        public async Task<IActionResult> Edit(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Index");

            try
            {
                var token = HttpContext.Session.GetString("JwtToken");
                var menuItem = await _menuService.GetMenuItemByIdAsync(id, token!);
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
                var categories = await _menuService.GetCategoriesAsync();
                ViewBag.Categories = categories;

                return View(updateDto);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Kunde inte hämta rätten.";
                return RedirectToAction("Index");
            }
        }

        // EDIT - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdateMenuItemDto dto)
        {
            if (!IsAdmin()) return RedirectToAction("Index");

            if (!ModelState.IsValid)
            {
                ViewBag.MenuItemId = id;
                var categories = await _menuService.GetCategoriesAsync();
                ViewBag.Categories = categories;
                return View(dto);
            }

            try
            {
                var token = HttpContext.Session.GetString("JwtToken");
                var result = await _menuService.UpdateMenuItemAsync(id, dto, token!);

                if (result != null)
                {
                    TempData["SuccessMessage"] = "Rätten har uppdaterats!";
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError("", "Kunde inte uppdatera rätten.");
                    ViewBag.MenuItemId = id;
                    var categories = await _menuService.GetCategoriesAsync();
                    ViewBag.Categories = categories;
                    return View(dto);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Ett fel uppstod: " + ex.Message);
                ViewBag.MenuItemId = id;
                var categories = await _menuService.GetCategoriesAsync();
                ViewBag.Categories = categories;
                return View(dto);
            }
        }

        // DELETE
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Index");

            try
            {
                var token = HttpContext.Session.GetString("JwtToken");
                var success = await _menuService.DeleteMenuItemAsync(id, token!);

                if (success)
                    TempData["SuccessMessage"] = "Rätten har tagits bort!";
                else
                    TempData["ErrorMessage"] = "Kunde inte ta bort rätten.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Ett fel uppstod: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        // TOGGLE POPULAR
        [HttpPost]
        public async Task<IActionResult> TogglePopular(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Index");

            try
            {
                var token = HttpContext.Session.GetString("JwtToken");
                var success = await _menuService.TogglePopularAsync(id, token!);

                if (success)
                    TempData["SuccessMessage"] = "Populärstatus har ändrats!";
                else
                    TempData["ErrorMessage"] = "Kunde inte ändra populärstatus.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Ett fel uppstod: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        // Helper method
        private bool IsAdmin()
        {
            var token = HttpContext.Session.GetString("JwtToken");
            return !string.IsNullOrEmpty(token);
        }
    }
}