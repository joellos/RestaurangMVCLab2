using RestaurangMVCLab2.DTOs;

namespace RestaurangMVCLab2.Services
{
    public class AuthService
    {
        private readonly HttpClient _httpClient;

        public AuthService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }


        public async Task<TokenResponseDto?> LoginAsync(LoginDto loginDto)
        {
            // Skicka LoginDto till auth/login endpoint
            var response = await _httpClient.PostAsJsonAsync("auth/login", loginDto);

            // Kolla om det gick bra
            if (response.IsSuccessStatusCode)
            {
                // Läs svaret som TokenResponseDto
                return await response.Content.ReadFromJsonAsync<TokenResponseDto>();
            }

            return null; // Om login misslyckades
        }


    }
}
