namespace AutoCare_Club_Api.Dtos.Technicians
{
    public class TechnicianDto
    {
         public string UserId { get; set; } =
            string.Empty;

        public string FirstName { get; set; } =
            string.Empty;

        public string LastName { get; set; } =
            string.Empty;

        public string Email { get; set; } =
            string.Empty;

        public string Specialty { get; set; } =
            string.Empty;

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}