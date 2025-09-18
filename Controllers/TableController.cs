// Controllers/Admin/TableController.cs
using Microsoft.AspNetCore.Mvc;
using RestaurangMVCLab2.DTOs;

using RestaurantMVCLab2.Services;

namespace RestaurantMVCLab2.Controllers
{
 
    public class TableController : Controller
    {
        private readonly TableService _tableService;

        public TableController(TableService tableService)
        {
            _tableService = tableService;
        }

        // Kontrollera admin-status och sätt JWT token
        private bool IsAdminAuthenticated()
        {
            var token = HttpContext.Session.GetString("JwtToken");
            if (string.IsNullOrEmpty(token))
            {
                return false;
            }

            _tableService.SetAuthToken(token);
            return true;
        }

        // GET: Admin/Table
        public async Task<IActionResult> Index()
        {
            if (!IsAdminAuthenticated())
            {
                TempData["ErrorMessage"] = "Du måste logga in som administratör.";
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                var tables = await _tableService.GetAllTablesAsync();

                // Lägg till success message från andra actions
                if (TempData.ContainsKey("SuccessMessage"))
                {
                    ViewBag.SuccessMessage = TempData["SuccessMessage"];
                }

                return View(tables);
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Kunde inte hämta bordslista.";
                return RedirectToAction("Index", "Home");
            }
        }

        // GET: Admin/Table/Create
        public IActionResult Create()
        {
            if (!IsAdminAuthenticated())
            {
                TempData["ErrorMessage"] = "Du måste logga in som administratör.";
                return RedirectToAction("Login", "Auth");
            }

            return View();
        }

        // POST: Admin/Table/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateTableDto model)
        {
            if (!IsAdminAuthenticated())
            {
                TempData["ErrorMessage"] = "Du måste logga in som administratör.";
                return RedirectToAction("Login", "Auth");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _tableService.CreateTableAsync(model);

            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
                return RedirectToAction(nameof(Index));
            }
            else
            {
                TempData["ErrorMessage"] = result.Message;
                return View(model);
            }
        }

        // GET: Admin/Table/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            if (!IsAdminAuthenticated())
            {
                TempData["ErrorMessage"] = "Du måste logga in som administratör.";
                return RedirectToAction("Login", "Auth");
            }

            var table = await _tableService.GetTableByIdAsync(id);
            if (table == null)
            {
                TempData["ErrorMessage"] = "Bordet hittades inte.";
                return RedirectToAction(nameof(Index));
            }

            // Konvertera till UpdateDto för redigering
            var updateDto = new UpdateTableDto
            {
                TableNumber = table.TableNumber,
                Capacity = table.Capacity,
                IsActive = table.IsActive
            };

            ViewBag.TableId = id;
            ViewBag.CurrentTable = table;
            return View(updateDto);
        }

        // POST: Admin/Table/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdateTableDto model)
        {
            if (!IsAdminAuthenticated())
            {
                TempData["ErrorMessage"] = "Du måste logga in som administratör.";
                return RedirectToAction("Login", "Auth");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.TableId = id;
                return View(model);
            }

            var result = await _tableService.UpdateTableAsync(id, model);

            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
                return RedirectToAction(nameof(Index));
            }
            else
            {
                TempData["ErrorMessage"] = result.Message;
                ViewBag.TableId = id;
                return View(model);
            }
        }

        // POST: Admin/Table/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (!IsAdminAuthenticated())
            {
                TempData["ErrorMessage"] = "Du måste logga in som administratör.";
                return RedirectToAction("Login", "Auth");
            }

            var result = await _tableService.DeleteTableAsync(id);

            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
            }
            else
            {
                TempData["ErrorMessage"] = result.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/Table/ToggleActive/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            if (!IsAdminAuthenticated())
            {
                TempData["ErrorMessage"] = "Du måste logga in som administratör.";
                return RedirectToAction("Login", "Auth");
            }

            var result = await _tableService.ToggleActiveAsync(id);

            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
            }
            else
            {
                TempData["ErrorMessage"] = result.Message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
