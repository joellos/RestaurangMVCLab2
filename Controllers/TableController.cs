using Microsoft.AspNetCore.Mvc;
using RestaurangMVCLab2.DTOs;
using RestaurangMVCLab2.Services;

namespace RestaurangMVCLab2.Controllers
{
    public class TableController : Controller
    {
        private readonly TableService _tableService;

        public TableController(TableService tableService)
        {
            _tableService = tableService;
        }

        // Helper method för admin-autentisering (samma pattern som BookingController)
        private bool IsAdminAuthenticated()
        {
            var token = HttpContext.Session.GetString("JwtToken");
            if (string.IsNullOrEmpty(token)) return false;

            _tableService.SetAuthToken(token);
            return true;
        }

        // GET: Table/Index
        public async Task<IActionResult> Index()
        {
            if (!IsAdminAuthenticated())
            {
                TempData["ErrorMessage"] = "Du måste logga in som administratör.";
                return RedirectToAction("Login", "Auth");
            }

            var result = await _tableService.GetAllTablesAsync();

            if (result.Succeeded)
            {
                var tables = result.GetData<List<TableResponseDto>>() ?? new List<TableResponseDto>();
                ViewBag.SuccessMessage = result.Message;
                return View(tables);
            }
            else
            {
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction("Index", "Home");
            }
        }

        // GET: Table/Create
        public IActionResult Create()
        {
            if (!IsAdminAuthenticated())
            {
                TempData["ErrorMessage"] = "Du måste logga in som administratör.";
                return RedirectToAction("Login", "Auth");
            }

            return View();
        }

        // POST: Table/Create
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

            if (result.Succeeded)
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

        // GET: Table/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            if (!IsAdminAuthenticated())
            {
                TempData["ErrorMessage"] = "Du måste logga in som administratör.";
                return RedirectToAction("Login", "Auth");
            }

            var result = await _tableService.GetTableByIdAsync(id);
            if (!result.Succeeded)
            {
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction(nameof(Index));
            }

            var table = result.GetData<TableResponseDto>();
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

        // POST: Table/Edit/5
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

                // Hämta nuvarande table-info igen för visning vid fel
                var currentTableResult = await _tableService.GetTableByIdAsync(id);
                if (currentTableResult.Succeeded)
                {
                    ViewBag.CurrentTable = currentTableResult.GetData<TableResponseDto>();
                }

                return View(model);
            }

            var result = await _tableService.UpdateTableAsync(id, model);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = result.Message;
                return RedirectToAction(nameof(Index));
            }
            else
            {
                TempData["ErrorMessage"] = result.Message;
                ViewBag.TableId = id;

                // Hämta nuvarande table-info igen för visning vid fel
                var currentTableResult = await _tableService.GetTableByIdAsync(id);
                if (currentTableResult.Succeeded)
                {
                    ViewBag.CurrentTable = currentTableResult.GetData<TableResponseDto>();
                }

                return View(model);
            }
        }

        // POST: Table/Delete/5
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

            TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        // POST: Table/ToggleActive/5
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

            TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"] = result.Message;
            return RedirectToAction(nameof(Index));
        }
    }
}