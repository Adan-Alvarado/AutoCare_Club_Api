using System.ComponentModel.DataAnnotations;

namespace AutoCare_Club_Api.Dtos.Auth
{
    public class RefreshTokenDto
    {
        [Required]
        public string Token { get; set; }

        [Required]
        public string RefreshToken { get; set; }
    }
}