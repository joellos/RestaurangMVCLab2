using RestaurangMVCLab2.DTOs;
using RestaurangMVCLab2.Models;
using System.Net.Http.Headers;
using System.Net;
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

        // AUTH METHOD - same pattern som BookingService
        public void SetAuthToken(string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
            _logger.LogInformation("🔑 JWT token set for TableService");
        }

        // GET ALL TABLES (admin) - api/tables
        public async Task<ServiceResponse> GetAllTablesAsync()
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

                var response = await _httpClient.GetAsync("tables");
                _logger.LogInformation("📡 API Response: {StatusCode}", response.StatusCode);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("❌ API Error: {StatusCode} - {Error}", response.StatusCode, errorContent);
                    return ServiceResponse.Failure($"API returned {response.StatusCode}: {response.ReasonPhrase}");
                }

                var json = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("✅ API returned data length: {Length}", json.Length);

                var tables = JsonSerializer.Deserialize<List<TableResponseDto>>(json, _jsonOptions);
                _logger.LogInformation("🎯 Deserialized {Count} tables", tables?.Count ?? 0);

                return ServiceResponse.Success(tables ?? new List<TableResponseDto>(),
                    $"Successfully loaded {tables?.Count ?? 0} tables");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "🌐 HTTP Request failed - API might not be running");
                return ServiceResponse.Failure($"Network error: {ex.Message}");
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "⏱️ Request timeout - API took too long to respond");
                return ServiceResponse.Failure("Request timeout - API took too long to respond");
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "🔧 JSON Deserialization failed");
                return ServiceResponse.Failure("Failed to parse API response");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 Unexpected error in GetAllTablesAsync");
                return ServiceResponse.Failure($"Unexpected error: {ex.Message}");
            }
        }

        // GET ACTIVE TABLES (admin) - api/tables/active
        public async Task<ServiceResponse> GetActiveTablesAsync()
        {
            try
            {
                _logger.LogInformation("🔍 Fetching active tables...");
                var response = await _httpClient.GetAsync("tables/active");
                _logger.LogInformation("📡 Active tables response: {StatusCode}", response.StatusCode);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return ServiceResponse.Failure($"API returned {response.StatusCode}: {response.ReasonPhrase}");
                }

                var json = await response.Content.ReadAsStringAsync();
                var tables = JsonSerializer.Deserialize<List<TableResponseDto>>(json, _jsonOptions);
                _logger.LogInformation("✅ Got {Count} active tables", tables?.Count ?? 0);

                return ServiceResponse.Success(tables ?? new List<TableResponseDto>(),
                    $"Found {tables?.Count ?? 0} active tables");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 Error in GetActiveTablesAsync");
                return ServiceResponse.Failure($"Error fetching active tables: {ex.Message}");
            }
        }

        // GET TABLE BY ID (admin) - api/tables/{id}
        public async Task<ServiceResponse> GetTableByIdAsync(int id)
        {
            try
            {
                _logger.LogInformation("🔍 Fetching table {TableId}...", id);
                var response = await _httpClient.GetAsync($"tables/{id}");

                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return ServiceResponse.Failure($"Table with ID {id} not found");
                }

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return ServiceResponse.Failure($"API returned {response.StatusCode}: {response.ReasonPhrase}");
                }

                var json = await response.Content.ReadAsStringAsync();
                var table = JsonSerializer.Deserialize<TableResponseDto>(json, _jsonOptions);
                _logger.LogInformation("✅ Got table {TableNumber}", table?.TableNumber);

                return ServiceResponse.Success(table, "Table loaded successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 Error getting table {TableId}", id);
                return ServiceResponse.Failure($"Error fetching table: {ex.Message}");
            }
        }

        // CREATE TABLE (admin) - POST api/tables
        public async Task<ServiceResponse> CreateTableAsync(CreateTableDto createDto)
        {
            try
            {
                _logger.LogInformation("🆕 Creating table {TableNumber} with capacity {Capacity}",
                    createDto.TableNumber, createDto.Capacity);

                var json = JsonSerializer.Serialize(createDto, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("tables", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    var table = JsonSerializer.Deserialize<TableResponseDto>(responseJson, _jsonOptions);
                    _logger.LogInformation("✅ Created table {TableNumber}", table?.TableNumber);
                    return ServiceResponse.Success(table, "Table created successfully");
                }
                else if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return ServiceResponse.Failure($"Validation error: {errorContent}");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("❌ Failed to create table: {StatusCode} - {Error}",
                        response.StatusCode, errorContent);
                    return ServiceResponse.Failure($"Failed to create table: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 Error creating table");
                return ServiceResponse.Failure($"Error creating table: {ex.Message}");
            }
        }

        // UPDATE TABLE (admin) - PUT api/tables/{id}
        public async Task<ServiceResponse> UpdateTableAsync(int id, UpdateTableDto updateDto)
        {
            try
            {
                _logger.LogInformation("🔄 Updating table {TableId}", id);

                var json = JsonSerializer.Serialize(updateDto, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync($"tables/{id}", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    var table = JsonSerializer.Deserialize<TableResponseDto>(responseJson, _jsonOptions);
                    _logger.LogInformation("✅ Updated table {TableNumber}", table?.TableNumber);
                    return ServiceResponse.Success(table, "Table updated successfully");
                }
                else if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return ServiceResponse.Failure($"Table with ID {id} not found");
                }
                else if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return ServiceResponse.Failure($"Validation error: {errorContent}");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return ServiceResponse.Failure($"Failed to update table: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 Error updating table {TableId}", id);
                return ServiceResponse.Failure($"Error updating table: {ex.Message}");
            }
        }

        // DELETE TABLE (admin) - DELETE api/tables/{id}
        public async Task<ServiceResponse> DeleteTableAsync(int id)
        {
            try
            {
                _logger.LogInformation("🗑️ Deleting table {TableId}", id);

                var response = await _httpClient.DeleteAsync($"tables/{id}");

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("✅ Deleted table {TableId}", id);
                    return ServiceResponse.Success("Table deleted successfully");
                }
                else if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return ServiceResponse.Failure($"Table with ID {id} not found");
                }
                else if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return ServiceResponse.Failure($"Cannot delete table: {errorContent}");
                }
                else
                {
                    return ServiceResponse.Failure($"Failed to delete table: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 Error deleting table {TableId}", id);
                return ServiceResponse.Failure($"Error deleting table: {ex.Message}");
            }
        }

        // TOGGLE ACTIVE/INACTIVE (admin) - PUT api/tables/{id}/toggle-active
        public async Task<ServiceResponse> ToggleActiveAsync(int id)
        {
            try
            {
                _logger.LogInformation("🔄 Toggling active status for table {TableId}", id);

                var response = await _httpClient.PutAsync($"tables/{id}/toggle-active", null);

                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    var table = JsonSerializer.Deserialize<TableResponseDto>(responseJson, _jsonOptions);
                    var status = table?.IsActive == true ? "activated" : "deactivated";
                    _logger.LogInformation("✅ Table {TableId} {Status}", id, status);
                    return ServiceResponse.Success(table, $"Table {status} successfully");
                }
                else if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return ServiceResponse.Failure($"Table with ID {id} not found");
                }
                else
                {
                    return ServiceResponse.Failure($"Failed to toggle table status: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 Error toggling status for table {TableId}", id);
                return ServiceResponse.Failure($"Error toggling table status: {ex.Message}");
            }
        }
    }
}