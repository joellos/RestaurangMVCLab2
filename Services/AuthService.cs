using RestaurangMVCLab2.DTOs;
using RestaurangMVCLab2.Models;
using System.Net;

namespace RestaurangMVCLab2.Services
{
    public class AuthService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AuthService> _logger;

        public AuthService(HttpClient httpClient, ILogger<AuthService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        // LOGIN - POST api/auth/login
        public async Task<ServiceResponse> LoginAsync(LoginDto loginDto)
        {
            try
            {
                _logger.LogInformation("Attempting login for user: {Username}", loginDto.Username);

                var response = await _httpClient.PostAsJsonAsync("auth/login", loginDto);

                if (response.IsSuccessStatusCode)
                {
                    var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponseDto>();
                    if (tokenResponse != null)
                    {
                        _logger.LogInformation("✅ Login successful for user: {Username}", loginDto.Username);
                        return ServiceResponse.Success(tokenResponse, "Login successful");
                    }
                    else
                    {
                        _logger.LogError("❌ Login response was empty");
                        return ServiceResponse.Failure("Login failed - empty response");
                    }
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _logger.LogWarning("🔒 Login failed for user: {Username} - Invalid credentials", loginDto.Username);
                    return ServiceResponse.Failure("Invalid username or password");
                }
                else if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("❌ Login failed for user: {Username} - Bad request: {Error}",
                        loginDto.Username, errorContent);
                    return ServiceResponse.Failure("Invalid login data");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("❌ Login failed for user: {Username} - Status: {StatusCode}, Error: {Error}",
                        loginDto.Username, response.StatusCode, errorContent);
                    return ServiceResponse.Failure($"Login failed: {response.StatusCode}");
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "🌐 HTTP error during login for user: {Username}", loginDto.Username);
                return ServiceResponse.Failure($"Network error: {ex.Message}");
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "⏱️ Timeout during login for user: {Username}", loginDto.Username);
                return ServiceResponse.Failure("Login request timed out");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 Unexpected error during login for user: {Username}", loginDto.Username);
                return ServiceResponse.Failure($"Unexpected error: {ex.Message}");
            }
        }

        // REFRESH TOKEN - POST api/auth/refresh (för framtida användning)
        public async Task<ServiceResponse> RefreshTokenAsync(string refreshToken)
        {
            try
            {
                _logger.LogInformation("Attempting to refresh token");

                var refreshRequest = new { RefreshToken = refreshToken };
                var response = await _httpClient.PostAsJsonAsync("auth/refresh", refreshRequest);

                if (response.IsSuccessStatusCode)
                {
                    var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponseDto>();
                    if (tokenResponse != null)
                    {
                        _logger.LogInformation("✅ Token refresh successful");
                        return ServiceResponse.Success(tokenResponse, "Token refreshed successfully");
                    }
                    else
                    {
                        return ServiceResponse.Failure("Token refresh failed - empty response");
                    }
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _logger.LogWarning("🔒 Token refresh failed - Invalid refresh token");
                    return ServiceResponse.Failure("Invalid refresh token");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("❌ Token refresh failed - Status: {StatusCode}, Error: {Error}",
                        response.StatusCode, errorContent);
                    return ServiceResponse.Failure($"Token refresh failed: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 Error during token refresh");
                return ServiceResponse.Failure($"Token refresh error: {ex.Message}");
            }
        }

        // LOGOUT - POST api/auth/logout (för framtida användning)
        public async Task<ServiceResponse> LogoutAsync(string token)
        {
            try
            {
                _logger.LogInformation("Attempting logout");

                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.PostAsync("auth/logout", null);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("✅ Logout successful");
                    return ServiceResponse.Success("Logged out successfully");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("❌ Logout failed - Status: {StatusCode}, Error: {Error}",
                        response.StatusCode, errorContent);
                    return ServiceResponse.Success("Logout completed (server error ignored)");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 Error during logout");
                // Even if logout fails on server, we consider it successful locally
                return ServiceResponse.Success("Logout completed (with errors)");
            }
        }

        // VALIDATE TOKEN - GET api/auth/validate (för framtida användning)
        public async Task<ServiceResponse> ValidateTokenAsync(string token)
        {
            try
            {
                _logger.LogInformation("Validating token");

                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.GetAsync("auth/validate");

                if (response.IsSuccessStatusCode)
                {
                    var validationResult = await response.Content.ReadFromJsonAsync<object>();
                    _logger.LogInformation("✅ Token is valid");
                    return ServiceResponse.Success(validationResult, "Token is valid");
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _logger.LogWarning("🔒 Token validation failed - Invalid token");
                    return ServiceResponse.Failure("Token is invalid or expired");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return ServiceResponse.Failure($"Token validation failed: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 Error during token validation");
                return ServiceResponse.Failure($"Token validation error: {ex.Message}");
            }
        }
    }
}