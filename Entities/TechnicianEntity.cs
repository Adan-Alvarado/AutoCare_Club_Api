using AutoCare_Club.Api.Entities;

namespace AutoCare_Club_Api.Entities
{
    public class TechnicianEntity
    {
        public string UserId { get; set; } =
            string.Empty;

        public string Specialty { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } =DateTime.UtcNow;

        public UserEntity User { get; set; } = null!;
    }
}