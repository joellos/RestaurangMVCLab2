using System.ComponentModel.DataAnnotations;

namespace RestaurangMVCLab2.DTOs
{
        public class LoginDto
        {
            [Required]
            [StringLength(50, MinimumLength = 3)]
            public string Username { get; set; } = string.Empty;

            [Required]
            [StringLength(100, MinimumLength = 6)]
            public string Password { get; set; } = string.Empty;
        }
}


