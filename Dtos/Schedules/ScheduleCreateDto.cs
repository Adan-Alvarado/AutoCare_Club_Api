using System.ComponentModel.DataAnnotations;

namespace AutoCare_Club_Api.Dtos.Schedules
{
    public class ScheduleCreateDto
    {
        [Display(Name = "Día de la semana")]
        [EnumDataType(typeof(DayOfWeek), ErrorMessage = "El día de la semana no es válido")]
        public DayOfWeek DayOfWeek { get; set; }

        [Display(Name = "Hora de inicio")]
        public TimeOnly StartTime { get; set; }

        [Display(Name = "Hora final")]
        public TimeOnly EndTime { get; set; }

        [Display(Name = "Disponible")]
        public bool IsAvailable { get; set; } = true;
    }
}