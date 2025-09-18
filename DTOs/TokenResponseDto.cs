namespace RestaurangMVCLab2.DTOs
{

        public class TokenResponseDto
        {
            public string AccessToken { get; set; } = string.Empty;
            public string RefreshToken { get; set; } = string.Empty;
            public DateTime ExpiresAt { get; set; }
            public string TokenType { get; set; } = "Bearer";

            // Administrator info
            public AdministratorInfoDto Administrator { get; set; } = null!;
        }

        public class AdministratorInfoDto
        {
            public Guid Id { get; set; }
            public string Username { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
        }
}


