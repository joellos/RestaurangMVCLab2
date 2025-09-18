using System.ComponentModel.DataAnnotations;

namespace RestaurangMVCLab2.DTOs
{
    public class CreateMenuItemDto
    {
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Range(0, 9999.99, ErrorMessage = "Price must be between 0 and 9999.99")]
        public decimal Price { get; set; }

        [Required]
        [StringLength(50)]
        public string Category { get; set; } = string.Empty;

        [Url]
        public string? ImageUrl { get; set; }

        public bool IsPopular { get; set; } = false;
    }
}