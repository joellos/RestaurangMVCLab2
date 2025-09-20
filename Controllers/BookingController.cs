// Controllers/BookingController.cs
using Microsoft.AspNetCore.Mvc;
using RestaurangMVCLab2.DTOs;
using RestaurangMVCLab2.Services;

namespace RestaurangMVCLab2.Controllers
{
    public class BookingController : Controller
    {
        private readonly BookingService _bookingService;

        public BookingController(BookingService bookingService)
        {
            _bookingService = bookingService;
        }

        private bool IsAdminAuthenticated()
        {
            var token = HttpContext.Session.GetString("JwtToken");
            if (string.IsNullOrEmpty(token)) return false;

            _bookingService.SetAuthToken(token);
            return true;
        }

        // GET: /Booking
        public async Task<IActionResult> Index(DateTime? date)
        {
            if (!IsAdminAuthenticated())
            {
                TempData["ErrorMessage"] = "Du måste logga in som administratör.";
                return RedirectToAction("Login", "Auth");
            }

            var result = date.HasValue
                ? await _bookingService.GetBookingsByDateAsync(date.Value)
                : await _bookingService.GetAllBookingsAsync();

            if (result.Succeeded)
            {
                var bookings = result.GetData<List<BookingResponseDto>>();
                ViewBag.SelectedDate = date;
                ViewBag.SuccessMessage = result.Message;
                return View(bookings);
            }

            TempData["ErrorMessage"] = result.Message;
            return View(new List<BookingResponseDto>());
        }

        // GET: /Booking/Details/5
        public async Task<IActionResult> Details(int id)
        {
            if (!IsAdminAuthenticated()) return RedirectToAction("Login", "Auth");

            var result = await _bookingService.GetBookingByIdAsync(id);
            if (!result.Succeeded)
            {
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction("Index");
            }

            var booking = result.GetData<BookingResponseDto>();
            return View(booking);
        }

        // GET: /Booking/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            if (!IsAdminAuthenticated()) return RedirectToAction("Login", "Auth");

            var result = await _bookingService.GetBookingByIdAsync(id);
            if (!result.Succeeded)
            {
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction("Index");
            }

            var booking = result.GetData<BookingResponseDto>();
            var updateDto = new UpdateBookingDto
            {
                BookingDateTime = booking.BookingDateTime,
                NumberOfGuests = booking.NumberOfGuests,
                SpecialRequests = booking.SpecialRequests
            };

            ViewBag.BookingId = id;
            ViewBag.Booking = booking;
            return View(updateDto);
        }

        // POST: /Booking/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdateBookingDto updateDto)
        {
            if (!IsAdminAuthenticated()) return RedirectToAction("Login", "Auth");

            if (!ModelState.IsValid)
            {
                ViewBag.BookingId = id;
                return View(updateDto);
            }

            var result = await _bookingService.UpdateBookingAsync(id, updateDto);

            TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"] = result.Message;
            return RedirectToAction("Index");
        }

        // POST: /Booking/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (!IsAdminAuthenticated()) return RedirectToAction("Login", "Auth");

            var result = await _bookingService.DeleteBookingAsync(id);
            TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"] = result.Message;
            return RedirectToAction("Index");
        }
    }
}
