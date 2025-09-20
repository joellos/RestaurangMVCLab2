using RestaurangMVCLab2.DTOs;
using System.Reflection.Metadata.Ecma335;

namespace RestaurangMVCLab2.Services
{
    public class MenuService
    {
        private readonly HttpClient _httpClient;

        public MenuService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<MenuItemResponseDto>> GetMenuItemsAsync()
        {
            var response = await _httpClient.GetFromJsonAsync<List<MenuItemResponseDto>>("menuitem");

            return response ?? new List<MenuItemResponseDto>();
        }

        public async Task<List<MenuItemResponseDto>> SearchMenuItemsAsync(string searchTerm)
        {
            var response = await _httpClient.GetFromJsonAsync<List<MenuItemResponseDto>>($"menuitem/search?term={searchTerm}");

            return response ?? new List<MenuItemResponseDto>();
        }

        public async Task<List<MenuItemResponseDto>> GetMenuItemByCategoryAsync(string category)
        {
            var response = await _httpClient.GetFromJsonAsync<List<MenuItemResponseDto>>($"menuitem/category/{category}");
            return response ?? new List<MenuItemResponseDto>();
        }

        public async Task<List<string>> GetCategoriesAsync()
        {
            var response = await _httpClient.GetFromJsonAsync<List<string>>($"menuitem/categories");
            return response ?? new List<string>();
        }

        public async Task<List<MenuItemResponseDto>> GetPopularDishes()
        {
            var response = await _httpClient.GetFromJsonAsync<List<MenuItemResponseDto>>($"menuitem/popular");
            return response ?? new List<MenuItemResponseDto>();
             
        }
        // Admin metoder - kräver JWT token
        public async Task<MenuItemResponseDto?> CreateMenuItemAsync(CreateMenuItemDto dto, string jwtToken)
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwtToken);

            var response = await _httpClient.PostAsJsonAsync("menuitem", dto);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<MenuItemResponseDto>();
            }
            return null;
        }

        public async Task<MenuItemResponseDto?> UpdateMenuItemAsync(int id, UpdateMenuItemDto dto, string jwtToken)
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwtToken);

            var response = await _httpClient.PutAsJsonAsync($"menuitem/{id}", dto);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<MenuItemResponseDto>();
            }
            return null;
        }

        public async Task<bool> DeleteMenuItemAsync(int id, string jwtToken)
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwtToken);

            var response = await _httpClient.DeleteAsync($"menuitem/{id}");
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> TogglePopularAsync(int id, string jwtToken)
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwtToken);

            var response = await _httpClient.PutAsync($"menuitem/{id}/toggle-popular", null);
            return response.IsSuccessStatusCode;
        }

        public async Task<MenuItemResponseDto?> GetMenuItemByIdAsync(int id, string jwtToken)
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwtToken);

            var response = await _httpClient.GetFromJsonAsync<MenuItemResponseDto>($"menuitem/{id}");
            return response;
        }

        public async Task<List<MenuItemResponseDto>> GetAllMenuItemsForAdminAsync(string jwtToken)
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwtToken);

            var response = await _httpClient.GetFromJsonAsync<List<MenuItemResponseDto>>("menuitem/admin/all");

            return response ?? new List<MenuItemResponseDto>();
        }

    }
}
