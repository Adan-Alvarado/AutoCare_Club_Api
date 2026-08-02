using AutoCare_Club_Api.Dtos.Appointments;
using AutoCare_Club_Api.Entities;

namespace AutoCare_Club_Api.Mappers
{
    public static class AppointmentMapper
    {
       public static AppointmentEntity CreateDtoToEntity(
            AppointmentCreateDto dto,
            string userId,
            TimeOnly endTime)
        {
            return new AppointmentEntity
            {
                UserId = userId,
                VehicleId = dto.VehicleId,
                ServiceId = dto.ServiceId,
                AppointmentDate = dto.AppointmentDate,
                StartTime = dto.StartTime,
                EndTime = endTime,
                Notes = dto.Notes,
                Status = AppointmentStatus.Pending
            };
        }

        public static AppointmentEntity EditDtoToEntity(
            AppointmentEntity entity,
            AppointmentEditDto dto,
            TimeOnly endTime)
        {
            entity.VehicleId = dto.VehicleId;
            entity.ServiceId = dto.ServiceId;
            entity.TechnicianId =
                string.IsNullOrWhiteSpace(dto.TechnicianId)
                    ? null
                    : dto.TechnicianId;
            entity.AppointmentDate = dto.AppointmentDate;
            entity.StartTime = dto.StartTime;
            entity.EndTime = endTime;
            entity.Status = dto.Status;
            entity.Notes = dto.Notes;

            return entity;
        }

        public static AppointmentDto EntityToDto(
            AppointmentEntity entity)
        {
            return new AppointmentDto
            {
                Id = entity.Id,
                UserId = entity.UserId,
                VehicleId = entity.VehicleId,
                ServiceId = entity.ServiceId,
                TechnicianId = entity.TechnicianId,
                AppointmentDate = entity.AppointmentDate,
                StartTime = entity.StartTime,
                EndTime = entity.EndTime,
                Status = entity.Status.ToString(),
                Notes = entity.Notes,
                CreatedAt = entity.CreatedAt
            };
        }

        public static List<AppointmentDto> ListEntityToListDto(
            IEnumerable<AppointmentEntity> entities)
        {
            return entities
                .Select(EntityToDto)
                .ToList();
        } 
    }
}