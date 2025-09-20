using RestaurangMVCLab2.DTOs;
using RestaurangMVCLab2.Models;
using System.Net.Http.Headers;
using System.Text.Json;

namespace RestaurangMVCLab2.Services
{
    public class MenuService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<MenuService> _logger;

        public MenuService(HttpClient httpClient, ILogger<MenuService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        // AUTH METHOD - same pattern som BookingService
        public void SetAuthToken(string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
            _logger.LogInformation("🔑 JWT token set for MenuService");
        }

        // GET ALL MENU ITEMS (public) - api/menuitem
        public async Task<ServiceResponse> GetMenuItemsAsync()
        {
            try
            {
                _logger.LogInformation("Fetching all menu items from API");

                var response = await _httpClient.GetFromJsonAsync<List<MenuItemResponseDto>>("menuitem");
                var menuItems = response ?? new List<MenuItemResponseDto>();

                _logger.LogInformation($"Successfully fetched {menuItems.Count} menu items");
                return ServiceResponse.Success(menuItems, "Menu items loaded successfully");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error when fetching menu items");
                return ServiceResponse.Failure($"Network error: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error when fetching menu items");
                return ServiceResponse.Failure($"Unexpected error: {ex.Message}");
            }
        }

        // SEARCH MENU ITEMS - api/menuitem/search?term={searchTerm}
        public async Task<ServiceResponse> SearchMenuItemsAsync(string searchTerm)
        {
            try
            {
                _logger.LogInformation($"Searching menu items with term: {searchTerm}");

                var response = await _httpClient.GetFromJsonAsync<List<MenuItemResponseDto>>($"menuitem/search?term={searchTerm}");
                var menuItems = response ?? new List<MenuItemResponseDto>();

                _logger.LogInformation($"Found {menuItems.Count} menu items for search term: {searchTerm}");
                return ServiceResponse.Success(menuItems, $"Found {menuItems.Count} items matching '{searchTerm}'");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error searching menu items with term: {searchTerm}");
                return ServiceResponse.Failure($"Search failed: {ex.Message}");
            }
        }

        // GET BY CATEGORY - api/menuitem/category/{category}
        public async Task<ServiceResponse> GetMenuItemByCategoryAsync(string category)
        {
            try
            {
                _logger.LogInformation($"Fetching menu items for category: {category}");

                var response = await _httpClient.GetFromJsonAsync<List<MenuItemResponseDto>>($"menuitem/category/{category}");
                var menuItems = response ?? new List<MenuItemResponseDto>();

                _logger.LogInformation($"Found {menuItems.Count} menu items in category: {category}");
                return ServiceResponse.Success(menuItems, $"Found {menuItems.Count} items in category '{category}'");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching menu items for category: {category}");
                return ServiceResponse.Failure($"Failed to load category: {ex.Message}");
            }
        }

        // GET CATEGORIES - api/menuitem/categories
        public async Task<ServiceResponse> GetCategoriesAsync()
        {
            try
            {
                _logger.LogInformation("Fetching menu categories");

                var response = await _httpClient.GetFromJsonAsync<List<string>>("menuitem/categories");
                var categories = response ?? new List<string>();

                _logger.LogInformation($"Successfully fetched {categories.Count} categories");
                return ServiceResponse.Success(categories, "Categories loaded successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching categories");
                return ServiceResponse.Failure($"Failed to load categories: {ex.Message}");
            }
        }

        // GET POPULAR DISHES - api/menuitem/popular
        public async Task<ServiceResponse> GetPopularDishesAsync()
        {
            try
            {
                _logger.LogInformation("Fetching popular dishes");

                var response = await _httpClient.GetFromJsonAsync<List<MenuItemResponseDto>>("menuitem/popular");
                var menuItems = response ?? new List<MenuItemResponseDto>();

                _logger.LogInformation($"Successfully fetched {menuItems.Count} popular dishes");
                return ServiceResponse.Success(menuItems, "Popular dishes loaded successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching popular dishes");
                return ServiceResponse.Failure($"Failed to load popular dishes: {ex.Message}");
            }
        }

        // ADMIN: GET ALL MENU ITEMS INCLUDING INACTIVE - api/menuitem/admin/all
        public async Task<ServiceResponse> GetAllMenuItemsForAdminAsync()
        {
            try
            {
                _logger.LogInformation("Fetching all menu items for admin (including inactive)");

                var response = await _httpClient.GetFromJsonAsync<List<MenuItemResponseDto>>("menuitem/admin/all");
                var menuItems = response ?? new List<MenuItemResponseDto>();

                _logger.LogInformation($"Successfully fetched {menuItems.Count} menu items for admin");
                return ServiceResponse.Success(menuItems, "All menu items loaded successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all menu items for admin");
                return ServiceResponse.Failure($"Failed to load menu items: {ex.Message}");
            }
        }

        // ADMIN: GET MENU ITEM BY ID - api/menuitem/{id}
        public async Task<ServiceResponse> GetMenuItemByIdAsync(int id)
        {
            try
            {
                _logger.LogInformation($"Fetching menu item with ID: {id}");

                var response = await _httpClient.GetFromJsonAsync<MenuItemResponseDto>($"menuitem/{id}");
                if (response == null)
                {
                    return ServiceResponse.Failure($"Menu item with ID {id} not found");
                }

                _logger.LogInformation($"Successfully fetched menu item: {response.Name}");
                return ServiceResponse.Success(response, "Menu item loaded successfully");
            }
            catch (HttpRequestException ex) when (ex.Message.Contains("404"))
            {
                return ServiceResponse.Failure($"Menu item with ID {id} not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching menu item {id}");
                return ServiceResponse.Failure($"Error fetching menu item: {ex.Message}");
            }
        }

        // ADMIN: CREATE MENU ITEM - POST api/menuitem
        public async Task<ServiceResponse> CreateMenuItemAsync(CreateMenuItemDto dto)
        {
            try
            {
                _logger.LogInformation($"Creating menu item: {dto.Name}");

                var response = await _httpClient.PostAsJsonAsync("menuitem", dto);

                if (response.IsSuccessStatusCode)
                {
                    var createdItem = await response.Content.ReadFromJsonAsync<MenuItemResponseDto>();
                    _logger.LogInformation($"Successfully created menu item: {createdItem?.Name}");
                    return ServiceResponse.Success(createdItem, "Menu item created successfully");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Failed to create menu item: {response.StatusCode} - {errorContent}");
                    return ServiceResponse.Failure($"Failed to create menu item: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating menu item: {dto.Name}");
                return ServiceResponse.Failure($"Error creating menu item: {ex.Message}");
            }
        }

        // ADMIN: UPDATE MENU ITEM - PUT api/menuitem/{id}
        public async Task<ServiceResponse> UpdateMenuItemAsync(int id, UpdateMenuItemDto dto)
        {
            try
            {
                _logger.LogInformation($"Updating menu item {id}");

                var response = await _httpClient.PutAsJsonAsync($"menuitem/{id}", dto);

                if (response.IsSuccessStatusCode)
                {
                    var updatedItem = await response.Content.ReadFromJsonAsync<MenuItemResponseDto>();
                    _logger.LogInformation($"Successfully updated menu item: {updatedItem?.Name}");
                    return ServiceResponse.Success(updatedItem, "Menu item updated successfully");
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return ServiceResponse.Failure($"Menu item with ID {id} not found");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return ServiceResponse.Failure($"Failed to update menu item: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating menu item {id}");
                return ServiceResponse.Failure($"Error updating menu item: {ex.Message}");
            }
        }

        // ADMIN: DELETE MENU ITEM - DELETE api/menuitem/{id}
        public async Task<ServiceResponse> DeleteMenuItemAsync(int id)
        {
            try
            {
                _logger.LogInformation($"Deleting menu item {id}");

                var response = await _httpClient.DeleteAsync($"menuitem/{id}");

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"Successfully deleted menu item {id}");
                    return ServiceResponse.Success("Menu item deleted successfully");
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return ServiceResponse.Failure($"Menu item with ID {id} not found");
                }
                else
                {
                    return ServiceResponse.Failure($"Failed to delete menu item: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting menu item {id}");
                return ServiceResponse.Failure($"Error deleting menu item: {ex.Message}");
            }
        }

        // ADMIN: TOGGLE POPULAR - PUT api/menuitem/{id}/toggle-popular
        public async Task<ServiceResponse> TogglePopularAsync(int id)
        {
            try
            {
                _logger.LogInformation($"Toggling popular status for menu item {id}");

                var response = await _httpClient.PutAsync($"menuitem/{id}/toggle-popular", null);

                if (response.IsSuccessStatusCode)
                {
                    var updatedItem = await response.Content.ReadFromJsonAsync<MenuItemResponseDto>();
                    var status = updatedItem?.IsPopular == true ? "popular" : "not popular";
                    _logger.LogInformation($"Successfully toggled popular status for menu item {id} to {status}");
                    return ServiceResponse.Success(updatedItem, $"Menu item is now {status}");
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return ServiceResponse.Failure($"Menu item with ID {id} not found");
                }
                else
                {
                    return ServiceResponse.Failure($"Failed to toggle popular status: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error toggling popular status for menu item {id}");
                return ServiceResponse.Failure($"Error toggling popular status: {ex.Message}");
            }
        }
    }
}