using AutoCare_Club_Api.Dtos.Schedules;
using AutoCare_Club_Api.Entities;

namespace AutoCare_Club_Api.Mappers
{
    public static class ScheduleMapper
    {
        public static ScheduleEntity CreateDtoToEntity(ScheduleCreateDto dto)
        {
            return new ScheduleEntity
            {
                DayOfWeek = dto.DayOfWeek,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                IsAvailable = dto.IsAvailable
            };
        }

        public static ScheduleEntity EditDtoToEntity(ScheduleEntity entity, ScheduleEditDto dto)
        {
            entity.DayOfWeek = dto.DayOfWeek;
            entity.StartTime = dto.StartTime;
            entity.EndTime = dto.EndTime;
            entity.IsAvailable = dto.IsAvailable;

            return entity;
        }

        public static ScheduleDto EntityToDto(ScheduleEntity entity)
        {
            return new ScheduleDto
            {
                Id = entity.Id,
                DayOfWeek = entity.DayOfWeek,
                StartTime = entity.StartTime,
                EndTime = entity.EndTime,
                IsAvailable = entity.IsAvailable
            };
        }

        public static List<ScheduleDto> ListEntityToListDto(IEnumerable<ScheduleEntity> entities)
        {
            return entities
                .Select(EntityToDto)
                .ToList();
        }
    }
}