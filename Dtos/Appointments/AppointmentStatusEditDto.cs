using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using AutoCare_Club_Api.Entities;

namespace AutoCare_Club_Api.Dtos.Appointments
{
    public class AppointmentStatusEditDto
    {
        [Required]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public AppointmentStatus? Status { get; set; }
    }
}
