using System.ComponentModel.DataAnnotations;

namespace RestaurangMVCLab2.DTOs
{
    public class UpdateMenuItemDto
    {
        [StringLength(100, MinimumLength = 2)]
        public string? Name { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [Range(0, 9999.99, ErrorMessage = "Price must be between 0 and 9999.99")]
        public decimal? Price { get; set; }

        [StringLength(50)]
        public string? Category { get; set; }

        [Url]
        public string? ImageUrl { get; set; }

        public bool? IsPopular { get; set; }
        public bool? IsAvailable { get; set; }
    }
}
