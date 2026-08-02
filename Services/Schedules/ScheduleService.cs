using AutoCare_Club.Api.Constants;
using AutoCare_Club.Api.Database;
using AutoCare_Club_Api.Dtos.Common;
using AutoCare_Club_Api.Dtos.Schedules;
using AutoCare_Club_Api.Entities;
using AutoCare_Club_Api.Mappers;
using Microsoft.EntityFrameworkCore;

namespace AutoCare_Club_Api.Services.Schedules
{
    public class ScheduleService : IScheduleService
    {
        private readonly AutoCareDbContext _context;

        public ScheduleService(
            AutoCareDbContext context)
        {
            _context = context;
        }
        public async Task<ResponseDto<ScheduleActionResponseDto>> CreateAsync(ScheduleCreateDto dto)
        {
            if (dto.StartTime >= dto.EndTime)
            {
                return new ResponseDto<ScheduleActionResponseDto>
                {
                    StatusCode = HttpStatusCode.BAD_REQUEST,
                    Status = false,
                    Message = "La hora de inicio debe ser menor que la hora final."
                };
            }

            var overlaps = await _context.Schedules.AnyAsync(schedule =>
                schedule.DayOfWeek == dto.DayOfWeek &&
                schedule.StartTime < dto.EndTime &&
                schedule.EndTime > dto.StartTime);

            if (overlaps)
            {
                return new ResponseDto<ScheduleActionResponseDto>
                {
                    StatusCode = HttpStatusCode.CONFLICT,
                    Status = false,
                    Message = "El horario se cruza con otro horario existente."
                };
            }

            try
            {
                var schedule = ScheduleMapper.CreateDtoToEntity(dto);

                await _context.Schedules.AddAsync(schedule);
                await _context.SaveChangesAsync();

                return new ResponseDto<ScheduleActionResponseDto>
                {
                    StatusCode = HttpStatusCode.CREATED,
                    Status = true,
                    Message = "Horario creado correctamente.",
                    Data = new ScheduleActionResponseDto
                    {
                        Id = schedule.Id
                    }
                };
            }
            catch
            {
                return InternalServerErrorResponse();
            }
        }

        public async Task<ResponseDto<ScheduleActionResponseDto>> DeleteAsync(string id)
        {
            var schedule = await _context.Schedules
                .FirstOrDefaultAsync(schedule => schedule.Id == id);

            if (schedule is null)
            {
                return new ResponseDto<ScheduleActionResponseDto>
                {
                    StatusCode = HttpStatusCode.NOT_FOUND,
                    Status = false,
                    Message = "No se encontró el horario."
                };
            }

            try
            {
                _context.Schedules.Remove(schedule);
                await _context.SaveChangesAsync();

                return new ResponseDto<ScheduleActionResponseDto>
                {
                    StatusCode = HttpStatusCode.OK,
                    Status = true,
                    Message = "Horario eliminado correctamente.",
                    Data = new ScheduleActionResponseDto
                    {
                        Id = id
                    }
                };
            }
            catch
            {
                return InternalServerErrorResponse();
            }
        }

        public async Task<ResponseDto<ScheduleActionResponseDto>> EditAsync(string id, ScheduleEditDto dto)
        {
            var schedule = await _context.Schedules
                .FirstOrDefaultAsync(schedule => schedule.Id == id);

            if (schedule is null)
            {
                return new ResponseDto<ScheduleActionResponseDto>
                {
                    StatusCode = HttpStatusCode.NOT_FOUND,
                    Status = false,
                    Message = "No se encontró el horario."
                };
            }

            if (dto.StartTime >= dto.EndTime)
            {
                return new ResponseDto<ScheduleActionResponseDto>
                {
                    StatusCode = HttpStatusCode.BAD_REQUEST,
                    Status = false,
                    Message = "La hora de inicio debe ser menor que la hora final."
                };
            }

            var overlaps = await _context.Schedules.AnyAsync(existing =>
                existing.Id != id &&
                existing.DayOfWeek == dto.DayOfWeek &&
                existing.StartTime < dto.EndTime &&
                existing.EndTime > dto.StartTime);

            if (overlaps)
            {
                return new ResponseDto<ScheduleActionResponseDto>
                {
                    StatusCode = HttpStatusCode.CONFLICT,
                    Status = false,
                    Message = "El horario se cruza con otro horario existente."
                };
            }

            try
            {
                ScheduleMapper.EditDtoToEntity(schedule, dto);
                await _context.SaveChangesAsync();

                return new ResponseDto<ScheduleActionResponseDto>
                {
                    StatusCode = HttpStatusCode.OK,
                    Status = true,
                    Message = "Horario actualizado correctamente.",
                    Data = new ScheduleActionResponseDto
                    {
                        Id = schedule.Id
                    }
                };
            }
            catch
            {
                return InternalServerErrorResponse();
            }
        }

        public async Task<ResponseDto<List<ScheduleDto>>> GetAllAsync()
        {
            var schedules = await _context.Schedules
                .AsNoTracking()
                .OrderBy(schedule => schedule.DayOfWeek)
                .ThenBy(schedule => schedule.StartTime)
                .ToListAsync();

            return new ResponseDto<List<ScheduleDto>>
            {
                StatusCode = HttpStatusCode.OK,
                Status = true,
                Message = "Horarios encontrados correctamente.",
                Data = ScheduleMapper.ListEntityToListDto(schedules)
            };
        }

        public async Task<ResponseDto<List<ScheduleAvailabilityDto>>> GetAvailableAsync(string serviceId, DateOnly date)
        {
            var service = await _context.Services
       .AsNoTracking()
       .FirstOrDefaultAsync(service =>service.Id == serviceId && service.IsActive);

            if (service is null)
            {
                return new ResponseDto<List<ScheduleAvailabilityDto>>
                {
                    StatusCode = HttpStatusCode.NOT_FOUND,
                    Status = false,
                    Message = "No se encontró el servicio."
                };
            }

            var currentDateTime = DateTime.Now;
            var today = DateOnly.FromDateTime(currentDateTime);

            if (date < today)
            {
                return new ResponseDto<List<ScheduleAvailabilityDto>>
                {
                    StatusCode = HttpStatusCode.BAD_REQUEST,
                    Status = false,
                    Message =
                        "No se pueden consultar horarios de una fecha pasada."
                };
            }

            if (service.DurationMinutes <= 0)
            {
                return new ResponseDto<List<ScheduleAvailabilityDto>>
                {
                    StatusCode =
                        HttpStatusCode.INTERNAL_SERVER_ERROR,
                    Status = false,
                    Message =
                        "El servicio no tiene una duración válida."
                };
            }

            var schedules = await _context.Schedules
                .AsNoTracking()
                .Where(schedule =>schedule.DayOfWeek == date.DayOfWeek && schedule.IsAvailable)
                .OrderBy(schedule => schedule.StartTime)
                .ToListAsync();

            var reservedAppointments =
                await _context.Appointments
                    .AsNoTracking()
                    .Where(appointment => appointment.AppointmentDate == date && appointment.Status != AppointmentStatus.Cancelled)
                    .Select(appointment => new
                    {
                        appointment.StartTime,
                        appointment.EndTime
                    })
                    .ToListAsync();

            var availableSlots =
                new List<ScheduleAvailabilityDto>();

            foreach (var schedule in schedules)
            {
                var slotStart =
                    date.ToDateTime(schedule.StartTime);

                var scheduleEnd =
                    date.ToDateTime(schedule.EndTime);

                while (slotStart.AddMinutes(
                    service.DurationMinutes) <= scheduleEnd)
                {
                    var slotEnd = slotStart.AddMinutes(
                        service.DurationMinutes);

                    var slotStartTime =
                        TimeOnly.FromDateTime(slotStart);

                    var slotEndTime =
                        TimeOnly.FromDateTime(slotEnd);

                    var isReserved = reservedAppointments.Any(
                        appointment =>
                            appointment.StartTime < slotEndTime &&
                            appointment.EndTime > slotStartTime);

                    if (slotStart > currentDateTime &&
                        !isReserved)
                    {
                        availableSlots.Add(
                            new ScheduleAvailabilityDto
                            {
                                ServiceId = service.Id,
                                Date = date,
                                StartTime = slotStartTime,
                                EndTime = slotEndTime
                            });
                    }

                    slotStart = slotEnd;
                }
            }

            return new ResponseDto<List<ScheduleAvailabilityDto>>
            {
                StatusCode = HttpStatusCode.OK,
                Status = true,
                Message =
                    "Horarios disponibles encontrados correctamente.",
                Data = availableSlots
            };
        }

        public async Task<ResponseDto<ScheduleDto>> GetOneAsync(string id)
        {
            var schedule = await _context.Schedules
                .AsNoTracking()
                .FirstOrDefaultAsync(schedule => schedule.Id == id);

            if (schedule is null)
            {
                return new ResponseDto<ScheduleDto>
                {
                    StatusCode = HttpStatusCode.NOT_FOUND,
                    Status = false,
                    Message = "No se encontró el horario."
                };
            }

            return new ResponseDto<ScheduleDto>
            {
                StatusCode = HttpStatusCode.OK,
                Status = true,
                Message = "Horario encontrado correctamente.",
                Data = ScheduleMapper.EntityToDto(schedule)
            };
        }

        private static ResponseDto<ScheduleActionResponseDto> InternalServerErrorResponse()
        {
            return new ResponseDto<ScheduleActionResponseDto>
            {
                StatusCode = HttpStatusCode.INTERNAL_SERVER_ERROR,
                Status = false,
                Message = "Ocurrió un error interno en el servidor."
            };
        }
    }
}