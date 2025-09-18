// Services/TableService.cs
using RestaurangMVCLab2.DTOs;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;


namespace RestaurantMVCLab2.Services
{
    public class TableService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        public TableService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        // Sätt JWT token för admin-anrop
        public void SetAuthToken(string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }

        // Hämta alla bord (admin)
        public async Task<List<TableResponseDto>> GetAllTablesAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/tables");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<List<TableResponseDto>>(json, _jsonOptions) ?? new List<TableResponseDto>();
                }

                return new List<TableResponseDto>();
            }
            catch (Exception)
            {
                return new List<TableResponseDto>();
            }
        }

        // Hämta aktiva bord (admin)
        public async Task<List<TableResponseDto>> GetActiveTablesAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/tables/active");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<List<TableResponseDto>>(json, _jsonOptions) ?? new List<TableResponseDto>();
                }

                return new List<TableResponseDto>();
            }
            catch (Exception)
            {
                return new List<TableResponseDto>();
            }
        }

        // Hämta specifikt bord (admin)
        public async Task<TableResponseDto?> GetTableByIdAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/tables/{id}");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<TableResponseDto>(json, _jsonOptions);
                }

                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        // Skapa nytt bord (admin)
        public async Task<(bool Success, string Message, TableResponseDto? Table)> CreateTableAsync(CreateTableDto createDto)
        {
            try
            {
                var json = JsonSerializer.Serialize(createDto, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("api/tables", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    var table = JsonSerializer.Deserialize<TableResponseDto>(responseJson, _jsonOptions);
                    return (true, "Bord skapat!", table);
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return (false, $"Kunde inte skapa bord: {response.StatusCode}", null);
                }
            }
            catch (Exception ex)
            {
                return (false, $"Fel: {ex.Message}", null);
            }
        }

        // Uppdatera bord (admin)
        public async Task<(bool Success, string Message, TableResponseDto? Table)> UpdateTableAsync(int id, UpdateTableDto updateDto)
        {
            try
            {
                var json = JsonSerializer.Serialize(updateDto, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync($"api/tables/{id}", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    var table = JsonSerializer.Deserialize<TableResponseDto>(responseJson, _jsonOptions);
                    return (true, "Bord uppdaterat!", table);
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return (false, "Bordet hittades inte.", null);
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return (false, $"Kunde inte uppdatera bord: {response.StatusCode}", null);
                }
            }
            catch (Exception ex)
            {
                return (false, $"Fel: {ex.Message}", null);
            }
        }

        // Ta bort bord (admin)
        public async Task<(bool Success, string Message)> DeleteTableAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/tables/{id}");

                if (response.IsSuccessStatusCode)
                {
                    return (true, "Bord borttaget!");
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return (false, "Bordet hittades inte.");
                }
                else
                {
                    return (false, $"Kunde inte ta bort bord: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                return (false, $"Fel: {ex.Message}");
            }
        }

        // Toggle aktivt/inaktivt (admin)
        public async Task<(bool Success, string Message, TableResponseDto? Table)> ToggleActiveAsync(int id)
        {
            try
            {
                var response = await _httpClient.PutAsync($"api/tables/{id}/toggle-active", null);

                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    var table = JsonSerializer.Deserialize<TableResponseDto>(responseJson, _jsonOptions);
                    var status = table?.IsActive == true ? "aktiverat" : "inaktiverat";
                    return (true, $"Bord {status}!", table);
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return (false, "Bordet hittades inte.", null);
                }
                else
                {
                    return (false, $"Kunde inte ändra status: {response.StatusCode}", null);
                }
            }
            catch (Exception ex)
            {
                return (false, $"Fel: {ex.Message}", null);
            }
        }
    }
}
