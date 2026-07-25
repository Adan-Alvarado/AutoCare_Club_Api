namespace AutoCare_Club.Api.Dtos.Vehicle
{
    public class VehicleDto
    {
        public string Id { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public int Year { get; set; }
        public string LicensePlate { get; set; } = string.Empty;
        public string VehicleType { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
