using RestaurangMVCLab2.DTOs;  
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace RestaurangMVCLab2.Services  
{
    public class TableService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly ILogger<TableService> _logger;

        public TableService(HttpClient httpClient, ILogger<TableService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
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

            _logger.LogInformation("🔑 JWT token set for TableService");
        }

        // Hämta alla bord (admin)
        public async Task<List<TableResponseDto>> GetAllTablesAsync()
        {
            try
            {
                _logger.LogInformation("🔍 Attempting to fetch tables from API...");
                _logger.LogInformation("🌐 API URL: {BaseAddress}tables", _httpClient.BaseAddress);

                // Logga headers
                var authHeader = _httpClient.DefaultRequestHeaders.Authorization;
                if (authHeader != null)
                {
                    _logger.LogInformation("🔑 Authorization header: {Scheme} {Parameter}",
                        authHeader.Scheme, authHeader.Parameter?.Substring(0, Math.Min(20, authHeader.Parameter.Length)) + "...");
                }
                else
                {
                    _logger.LogWarning("⚠️ NO Authorization header set!");
                }

                // VIKTIGT: Använd bara "tables" inte "api/tables" - BaseAddress innehåller redan "/api/"
                var response = await _httpClient.GetAsync("tables");

                _logger.LogInformation("📡 API Response: {StatusCode}", response.StatusCode);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation("✅ API returned data length: {Length}", json.Length);
                    _logger.LogInformation("📄 First 200 chars: {JsonPreview}",
                        json.Length > 200 ? json.Substring(0, 200) + "..." : json);

                    var tables = JsonSerializer.Deserialize<List<TableResponseDto>>(json, _jsonOptions);
                    _logger.LogInformation("🎯 Deserialized {Count} tables", tables?.Count ?? 0);

                    return tables ?? new List<TableResponseDto>();
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("❌ API Error: {StatusCode} - {Error}",
                        response.StatusCode, errorContent);

                    return new List<TableResponseDto>();
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "🌐 HTTP Request failed - API might not be running at https://localhost:7135");
                return new List<TableResponseDto>();
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "⏱️ Request timeout - API took too long to respond");
                return new List<TableResponseDto>();
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "🔧 JSON Deserialization failed");
                return new List<TableResponseDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 Unexpected error in GetAllTablesAsync");
                return new List<TableResponseDto>();
            }
        }

        // Hämta aktiva bord (admin)
        public async Task<List<TableResponseDto>> GetActiveTablesAsync()
        {
            try
            {
                _logger.LogInformation("🔍 Fetching active tables...");
                var response = await _httpClient.GetAsync("tables/active");  // ← ÄNDRAT från "api/tables/active"

                _logger.LogInformation("📡 Active tables response: {StatusCode}", response.StatusCode);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var tables = JsonSerializer.Deserialize<List<TableResponseDto>>(json, _jsonOptions);
                    _logger.LogInformation("✅ Got {Count} active tables", tables?.Count ?? 0);
                    return tables ?? new List<TableResponseDto>();
                }

                return new List<TableResponseDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 Error in GetActiveTablesAsync");
                return new List<TableResponseDto>();
            }
        }

        // Hämta specifikt bord (admin)
        public async Task<TableResponseDto?> GetTableByIdAsync(int id)
        {
            try
            {
                _logger.LogInformation("🔍 Fetching table {TableId}...", id);
                var response = await _httpClient.GetAsync($"tables/{id}");  // ← ÄNDRAT från "api/tables/{id}"

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var table = JsonSerializer.Deserialize<TableResponseDto>(json, _jsonOptions);
                    _logger.LogInformation("✅ Got table {TableNumber}", table?.TableNumber);
                    return table;
                }

                _logger.LogWarning("❌ Table {TableId} not found: {StatusCode}", id, response.StatusCode);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 Error getting table {TableId}", id);
                return null;
            }
        }

        // Skapa nytt bord (admin)
        public async Task<(bool Success, string Message, TableResponseDto? Table)> CreateTableAsync(CreateTableDto createDto)
        {
            try
            {
                _logger.LogInformation("🆕 Creating table {TableNumber} with capacity {Capacity}",
                    createDto.TableNumber, createDto.Capacity);

                var json = JsonSerializer.Serialize(createDto, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("tables", content);  // ← ÄNDRAT från "api/tables"

                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    var table = JsonSerializer.Deserialize<TableResponseDto>(responseJson, _jsonOptions);
                    _logger.LogInformation("✅ Created table {TableNumber}", table?.TableNumber);
                    return (true, "Bord skapat!", table);
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("❌ Failed to create table: {StatusCode} - {Error}",
                        response.StatusCode, errorContent);
                    return (false, $"Kunde inte skapa bord: {response.StatusCode}", null);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 Error creating table");
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

                var response = await _httpClient.PutAsync($"tables/{id}", content);  // ← ÄNDRAT från "api/tables/{id}"

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
                var response = await _httpClient.DeleteAsync($"tables/{id}");  // ← ÄNDRAT från "api/tables/{id}"

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
                var response = await _httpClient.PutAsync($"tables/{id}/toggle-active", null);  // ← ÄNDRAT från "api/tables/{id}/toggle-active"

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