using AutoCare_Club.Api.Constants;
using AutoCare_Club.Api.Database;
using AutoCare_Club.Api.Entities;
using AutoCare_Club_Api.Dtos.Appointments;
using AutoCare_Club_Api.Dtos.Common;
using AutoCare_Club_Api.Entities;
using AutoCare_Club_Api.Mappers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AutoCare_Club_Api.Services.Appointments
{
    public class AppointmentService : IAppointmentService
    {
        private readonly AutoCareDbContext _context;
        private readonly UserManager<UserEntity> _userManager;

        public AppointmentService(
            AutoCareDbContext context,
            UserManager<UserEntity> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        public async Task<ResponseDto<AppointmentActionResponseDto>> CancelAsync(string id, string userId, bool canManage)
        {
            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(appointment =>
                    appointment.Id == id);

            if (appointment is null ||
                (!canManage &&
                 appointment.UserId != userId))
            {
                return Error<AppointmentActionResponseDto>(
                    HttpStatusCode.NOT_FOUND,
                    "No se encontró la cita.");
            }

            if (appointment.Status ==
                AppointmentStatus.Completed)
            {
                return Error<AppointmentActionResponseDto>(
                    HttpStatusCode.BAD_REQUEST,
                    "Una cita completada no puede cancelarse.");
            }

            if (appointment.Status ==
                AppointmentStatus.Cancelled)
            {
                return SuccessAction(
                    HttpStatusCode.OK,
                    "La cita ya se encontraba cancelada.",
                    appointment.Id);
            }

            appointment.Status =
                AppointmentStatus.Cancelled;

            try
            {
                await _context.SaveChangesAsync();

                return SuccessAction(
                    HttpStatusCode.OK,
                    "Cita cancelada correctamente.",
                    appointment.Id);
            }
            catch
            {
                return InternalServerError();
            }
        }

        public async Task<ResponseDto<AppointmentActionResponseDto>> CreateAsync(string userId, AppointmentCreateDto dto)
        {
            var validation = await ValidateSlotAsync(
                userId,
                dto.VehicleId,
                dto.ServiceId,
                dto.AppointmentDate,
                dto.StartTime);

            if (validation.Error is not null)
            {
                return validation.Error;
            }

            var appointment =
                AppointmentMapper.CreateDtoToEntity(
                    dto,
                    userId,
                    validation.EndTime);

            try
            {
                await _context.Appointments.AddAsync(
                    appointment);

                await _context.SaveChangesAsync();

                return SuccessAction(
                    HttpStatusCode.CREATED,
                    "Cita creada correctamente.",
                    appointment.Id);
            }
            catch
            {
                return InternalServerError();
            }
        }

        public async Task<ResponseDto<AppointmentActionResponseDto>> DeleteAsync(string id)
        {
            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(appointment =>
                    appointment.Id == id);

            if (appointment is null)
            {
                return Error<AppointmentActionResponseDto>(
                    HttpStatusCode.NOT_FOUND,
                    "No se encontró la cita.");
            }

            try
            {
                _context.Appointments.Remove(appointment);
                await _context.SaveChangesAsync();

                return SuccessAction(
                    HttpStatusCode.OK,
                    "Cita eliminada correctamente.",
                    appointment.Id);
            }
            catch (DbUpdateException)
            {
                return Error<AppointmentActionResponseDto>(
                    HttpStatusCode.CONFLICT,
                    "La cita está relacionada con otro registro y no puede eliminarse.");
            }
            catch
            {
                return InternalServerError();
            }
        }

        public async Task<ResponseDto<AppointmentActionResponseDto>> EditAsync(string id, AppointmentEditDto dto)
        {
            var appointment = await _context.Appointments
               .FirstOrDefaultAsync(appointment =>
                   appointment.Id == id);

            if (appointment is null)
            {
                return Error<AppointmentActionResponseDto>(
                    HttpStatusCode.NOT_FOUND,
                    "No se encontró la cita.");
            }

            if (!IsValidStatusTransition(
                appointment.Status,
                dto.Status))
            {
                return Error<AppointmentActionResponseDto>(
                    HttpStatusCode.BAD_REQUEST,
                    "El cambio de estado no es válido.");
            }

            if (!string.IsNullOrWhiteSpace(
                dto.TechnicianId))
            {
                var technician = await _userManager
                    .FindByIdAsync(dto.TechnicianId);

                if (technician is null ||
                    !technician.IsActive ||
                    !await _userManager.IsInRoleAsync(
                        technician,
                        RolesConstant.Technician))
                {
                    return Error<
                        AppointmentActionResponseDto>(
                        HttpStatusCode.BAD_REQUEST,
                        "El técnico seleccionado no es válido.");
                }
            }

            var validation = await ValidateSlotAsync(
                appointment.UserId,
                dto.VehicleId,
                dto.ServiceId,
                dto.AppointmentDate,
                dto.StartTime,
                appointment.Id);

            if (validation.Error is not null)
            {
                return validation.Error;
            }

            AppointmentMapper.EditDtoToEntity(
                appointment,
                dto,
                validation.EndTime);

            try
            {
                await _context.SaveChangesAsync();

                return SuccessAction(
                    HttpStatusCode.OK,
                    "Cita actualizada correctamente.",
                    appointment.Id);
            }
            catch
            {
                return InternalServerError();
            }
        }

        public async Task<ResponseDto<List<AppointmentDto>>> GetAllAsync()
        {
            var appointments = await _context.Appointments
                .AsNoTracking()
                .OrderBy(appointment =>
                    appointment.AppointmentDate)
                .ThenBy(appointment =>
                    appointment.StartTime)
                .ToListAsync();

            return new ResponseDto<List<AppointmentDto>>
            {
                StatusCode = HttpStatusCode.OK,
                Status = true,
                Message = "Citas encontradas correctamente.",
                Data = AppointmentMapper.ListEntityToListDto(
                    appointments)
            };
        }

        public async Task<ResponseDto<List<AppointmentDto>>> GetMineAsync(string userId)
        {
            var appointments = await _context.Appointments
               .AsNoTracking()
               .Where(appointment =>
                   appointment.UserId == userId)
               .OrderBy(appointment =>
                   appointment.AppointmentDate)
               .ThenBy(appointment =>
                   appointment.StartTime)
               .ToListAsync();

            return new ResponseDto<List<AppointmentDto>>
            {
                StatusCode = HttpStatusCode.OK,
                Status = true,
                Message = "Citas encontradas correctamente.",
                Data = AppointmentMapper.ListEntityToListDto(
                    appointments)
            };
        }

        public async Task<ResponseDto<AppointmentDto>> GetOneAsync(string id, string userId, bool canManage)
        {
            var appointment = await _context.Appointments
                .AsNoTracking()
                .FirstOrDefaultAsync(appointment =>
                    appointment.Id == id);

            if (appointment is null ||
                (!canManage &&
                 appointment.UserId != userId))
            {
                return Error<AppointmentDto>(
                    HttpStatusCode.NOT_FOUND,
                    "No se encontró la cita.");
            }

            return new ResponseDto<AppointmentDto>
            {
                StatusCode = HttpStatusCode.OK,
                Status = true,
                Message = "Cita encontrada correctamente.",
                Data = AppointmentMapper.EntityToDto(
                    appointment)
            };
        }

        private async Task<SlotValidationResult> ValidateSlotAsync(
                string userId,
                string vehicleId,
                string serviceId,
                DateOnly appointmentDate,
                TimeOnly startTime,
                string? excludedAppointmentId = null)
        {
            var startDateTime =
                appointmentDate.ToDateTime(startTime);

            if (startDateTime <= DateTime.Now)
            {
                return SlotValidationResult.Failed(
                    Error<AppointmentActionResponseDto>(
                        HttpStatusCode.BAD_REQUEST,
                        "No se pueden reservar horarios pasados."));
            }

            var vehicleExists = await _context.Vehicles
                .AsNoTracking()
                .AnyAsync(vehicle =>
                    vehicle.Id == vehicleId &&
                    vehicle.UserId == userId &&
                    vehicle.IsActive);

            if (!vehicleExists)
            {
                return SlotValidationResult.Failed(
                    Error<AppointmentActionResponseDto>(
                        HttpStatusCode.NOT_FOUND,
                        "No se encontró un vehículo válido del cliente."));
            }

            var service = await _context.Services
                .AsNoTracking()
                .FirstOrDefaultAsync(service =>
                    service.Id == serviceId &&
                    service.IsActive);

            if (service is null)
            {
                return SlotValidationResult.Failed(
                    Error<AppointmentActionResponseDto>(
                        HttpStatusCode.NOT_FOUND,
                        "No se encontró el servicio."));
            }

            var endDateTime = startDateTime.AddMinutes(
                service.DurationMinutes);

            if (DateOnly.FromDateTime(endDateTime) !=
                appointmentDate)
            {
                return SlotValidationResult.Failed(
                    Error<AppointmentActionResponseDto>(
                        HttpStatusCode.BAD_REQUEST,
                        "La cita no puede terminar en otro día."));
            }

            var endTime =
                TimeOnly.FromDateTime(endDateTime);

            var isInsideSchedule =
                await _context.Schedules
                    .AsNoTracking()
                    .AnyAsync(schedule =>
                        schedule.IsAvailable &&
                        schedule.DayOfWeek ==
                            appointmentDate.DayOfWeek &&
                        schedule.StartTime <= startTime &&
                        schedule.EndTime >= endTime);

            if (!isInsideSchedule)
            {
                return SlotValidationResult.Failed(
                    Error<AppointmentActionResponseDto>(
                        HttpStatusCode.BAD_REQUEST,
                        "El horario seleccionado no está disponible."));
            }

            var isReserved =
                await _context.Appointments
                    .AsNoTracking()
                    .AnyAsync(existing =>
                        existing.Id !=
                            excludedAppointmentId &&
                        existing.AppointmentDate ==
                            appointmentDate &&
                        existing.Status !=
                            AppointmentStatus.Cancelled &&
                        existing.StartTime < endTime &&
                        existing.EndTime > startTime);

            if (isReserved)
            {
                return SlotValidationResult.Failed(
                    Error<AppointmentActionResponseDto>(
                        HttpStatusCode.CONFLICT,
                        "El horario seleccionado ya está reservado."));
            }

            return SlotValidationResult.Succeeded(
                endTime);
        }

        private static bool IsValidStatusTransition(AppointmentStatus current, AppointmentStatus next)
        {
            if (current == next)
            {
                return true;
            }

            return current switch
            {
                AppointmentStatus.Pending =>
                    next == AppointmentStatus.Confirmed ||
                    next == AppointmentStatus.Cancelled,

                AppointmentStatus.Confirmed =>
                    next == AppointmentStatus.InProgress ||
                    next == AppointmentStatus.Cancelled,

                AppointmentStatus.InProgress =>
                    next == AppointmentStatus.Completed ||
                    next == AppointmentStatus.Cancelled,

                AppointmentStatus.Completed => false,
                AppointmentStatus.Cancelled => false,
                _ => false
            };
        }

        private static ResponseDto<T> Error<T>(
            int statusCode,
            string message)
        {
            return new ResponseDto<T>
            {
                StatusCode = statusCode,
                Status = false,
                Message = message
            };
        }

        private static ResponseDto<AppointmentActionResponseDto> SuccessAction(
                int statusCode,
                string message,
                string id)
        {
            return new ResponseDto<
                AppointmentActionResponseDto>
            {
                StatusCode = statusCode,
                Status = true,
                Message = message,
                Data = new AppointmentActionResponseDto
                {
                    Id = id
                }
            };
        }

        private static ResponseDto<AppointmentActionResponseDto> InternalServerError()
        {
            return Error<AppointmentActionResponseDto>(
                HttpStatusCode.INTERNAL_SERVER_ERROR,
                "Ocurrió un error interno en el servidor.");
        }

        private class SlotValidationResult
        {
            public TimeOnly EndTime { get; private set; }

            public ResponseDto< AppointmentActionResponseDto>? Error
            { get; private set; }

            public static SlotValidationResult Succeeded(
                TimeOnly endTime)
            {
                return new SlotValidationResult
                {
                    EndTime = endTime
                };
            }

            public static SlotValidationResult Failed(ResponseDto<AppointmentActionResponseDto> error)
            {
                return new SlotValidationResult
                {
                    Error = error
                };
            }
        }
    }
}