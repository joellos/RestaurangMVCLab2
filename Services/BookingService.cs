using RestaurangMVCLab2.Models;
using System.Net.Http.Headers;
using System.Net;
using RestaurangMVCLab2.DTOs;
using System.Text.Json;

namespace RestaurangMVCLab2.Services
{
    public class BookingService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<BookingService> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public BookingService(HttpClient httpClient, ILogger<BookingService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;

            // Viktigt: Konfigurera JSON-deserialisering korrekt
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        // AUTH METHOD - same pattern som TableService
        public void SetAuthToken(string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
            _logger.LogInformation("🔑 JWT token set for BookingService");
        }

        // GET ALL BOOKINGS - api/Booking
        public async Task<ServiceResponse> GetAllBookingsAsync()
        {
            try
            {
                _logger.LogInformation("Fetching all bookings from API");

                var response = await _httpClient.GetAsync("Booking");
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"API Error: {response.StatusCode} - {errorContent}");
                    return ServiceResponse.Failure($"API returned {response.StatusCode}: {response.ReasonPhrase}");
                }

                // Läs JSON som sträng först för debugging
                var jsonString = await response.Content.ReadAsStringAsync();
                _logger.LogDebug($"API Response JSON: {jsonString}");

                // Deserialisera med JsonSerializer istället för ReadFromJsonAsync
                var bookings = JsonSerializer.Deserialize<List<BookingResponseDto>>(jsonString, _jsonOptions);

                if (bookings == null)
                {
                    _logger.LogWarning("Deserialization returned null");
                    return ServiceResponse.Failure("Failed to deserialize bookings");
                }

                // Logga för debugging
                foreach (var booking in bookings)
                {
                    _logger.LogInformation($"Booking {booking.Id}: Customer={booking.Customer?.Name ?? "NULL"}, Table={booking.Table?.TableNumber ?? 0}");
                }

                _logger.LogInformation($"Successfully fetched {bookings.Count} bookings");
                return ServiceResponse.Success(bookings, "Bookings loaded successfully");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error when fetching bookings");
                return ServiceResponse.Failure($"Network error: {ex.Message}");
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "JSON deserialization error");
                return ServiceResponse.Failure($"Data format error: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error when fetching bookings");
                return ServiceResponse.Failure($"Unexpected error: {ex.Message}");
            }
        }

        // GET BOOKING BY ID - api/Booking/{id}
        public async Task<ServiceResponse> GetBookingByIdAsync(int id)
        {
            try
            {
                _logger.LogInformation($"Fetching booking with ID: {id}");

                var response = await _httpClient.GetAsync($"Booking/{id}");
                if (response.StatusCode == HttpStatusCode.NotFound)
                    return ServiceResponse.Failure($"Booking with ID {id} not found");

                if (!response.IsSuccessStatusCode)
                    return ServiceResponse.Failure($"API returned {response.StatusCode}: {response.ReasonPhrase}");

                var jsonString = await response.Content.ReadAsStringAsync();
                var booking = JsonSerializer.Deserialize<BookingResponseDto>(jsonString, _jsonOptions);

                if (booking == null)
                    return ServiceResponse.Failure("Failed to deserialize booking");

                return ServiceResponse.Success(booking, "Booking loaded successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching booking {id}");
                return ServiceResponse.Failure($"Error fetching booking: {ex.Message}");
            }
        }

        // UPDATE BOOKING - PUT api/Booking/{id}
        public async Task<ServiceResponse> UpdateBookingAsync(int id, UpdateBookingDto updateDto)
        {
            try
            {
                _logger.LogInformation($"Updating booking {id}");

                var response = await _httpClient.PutAsJsonAsync($"Booking/{id}", updateDto);

                if (response.StatusCode == HttpStatusCode.NotFound)
                    return ServiceResponse.Failure($"Booking with ID {id} not found");

                if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return ServiceResponse.Failure($"Validation error: {errorContent}");
                }

                if (!response.IsSuccessStatusCode)
                    return ServiceResponse.Failure($"API returned {response.StatusCode}: {response.ReasonPhrase}");

                var jsonString = await response.Content.ReadAsStringAsync();
                var updatedBooking = JsonSerializer.Deserialize<BookingResponseDto>(jsonString, _jsonOptions);

                return ServiceResponse.Success(updatedBooking, "Booking updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating booking {id}");
                return ServiceResponse.Failure($"Error updating booking: {ex.Message}");
            }
        }

        // DELETE BOOKING - DELETE api/Booking/{id}
        public async Task<ServiceResponse> DeleteBookingAsync(int id)
        {
            try
            {
                _logger.LogInformation($"Deleting booking {id}");

                var response = await _httpClient.DeleteAsync($"Booking/{id}");

                if (response.StatusCode == HttpStatusCode.NotFound)
                    return ServiceResponse.Failure($"Booking with ID {id} not found");

                if (!response.IsSuccessStatusCode)
                    return ServiceResponse.Failure($"API returned {response.StatusCode}: {response.ReasonPhrase}");

                return ServiceResponse.Success("Booking deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting booking {id}");
                return ServiceResponse.Failure($"Error deleting booking: {ex.Message}");
            }
        }

        // GET BOOKINGS BY DATE - api/Booking/by-date?date=2025-01-20
        public async Task<ServiceResponse> GetBookingsByDateAsync(DateTime date)
        {
            try
            {
                var dateString = date.ToString("yyyy-MM-dd");
                _logger.LogInformation($"Fetching bookings for date: {dateString}");

                var response = await _httpClient.GetAsync($"Booking/by-date?date={dateString}");
                if (!response.IsSuccessStatusCode)
                    return ServiceResponse.Failure($"API returned {response.StatusCode}: {response.ReasonPhrase}");

                var jsonString = await response.Content.ReadAsStringAsync();
                var bookings = JsonSerializer.Deserialize<List<BookingResponseDto>>(jsonString, _jsonOptions);

                return ServiceResponse.Success(bookings ?? new List<BookingResponseDto>(),
                    $"Found {bookings?.Count ?? 0} bookings for {date:yyyy-MM-dd}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching bookings for date {date:yyyy-MM-dd}");
                return ServiceResponse.Failure($"Error fetching bookings: {ex.Message}");
            }
        }

        // GET BOOKINGS BY DATE RANGE - api/Booking/date-range?startDate=&endDate=
        public async Task<ServiceResponse> GetBookingsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                var startString = startDate.ToString("yyyy-MM-dd");
                var endString = endDate.ToString("yyyy-MM-dd");
                _logger.LogInformation($"Fetching bookings from {startString} to {endString}");

                var response = await _httpClient.GetAsync($"Booking/date-range?startDate={startString}&endDate={endString}");
                if (!response.IsSuccessStatusCode)
                    return ServiceResponse.Failure($"API returned {response.StatusCode}: {response.ReasonPhrase}");

                var jsonString = await response.Content.ReadAsStringAsync();
                var bookings = JsonSerializer.Deserialize<List<BookingResponseDto>>(jsonString, _jsonOptions);

                return ServiceResponse.Success(bookings ?? new List<BookingResponseDto>(),
                    $"Found {bookings?.Count ?? 0} bookings between {startDate:yyyy-MM-dd} and {endDate:yyyy-MM-dd}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching bookings for date range");
                return ServiceResponse.Failure($"Error fetching bookings: {ex.Message}");
            }
        }
    }
}