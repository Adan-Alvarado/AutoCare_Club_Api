using AutoCare_Club.Api.Constants;
using AutoCare_Club.Api.Database;
using AutoCare_Club.Api.Entities;
using AutoCare_Club_Api.Dtos.Common;
using AutoCare_Club_Api.Dtos.Technicians;
using AutoCare_Club_Api.Entities;
using AutoCare_Club_Api.Mappers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AutoCare_Club_Api.Services.Technicians
{
    public class TechnicianService : ITechnicianService
    {
        private readonly AutoCareDbContext _context;
        private readonly UserManager<UserEntity> _userManager;
        private readonly int _defaultPageSize;
        private readonly int _pageSizeLimit;

        public TechnicianService(
            AutoCareDbContext context,
            UserManager<UserEntity> userManager,
            IConfiguration configuration)
        {
            _context = context;
            _userManager = userManager;

            var configuredPageSize =
                configuration.GetValue<int>("PageSize");

            var configuredPageSizeLimit =
                configuration.GetValue<int>(
                    "PageSizeLimit");

            _defaultPageSize = configuredPageSize > 0
                ? configuredPageSize
                : 10;

            _pageSizeLimit =
                configuredPageSizeLimit > 0
                    ? configuredPageSizeLimit
                    : 100;
        }

        public async Task<
            ResponseDto<PageDto<List<TechnicianDto>>>>
            GetPageAsync(
                string searchTerm = "",
                int page = 1,
                int pageSize = 10,
                bool includeInactive = false)
        {
            page = page > 0 ? page : 1;

            pageSize = pageSize > 0
                ? pageSize
                : _defaultPageSize;

            pageSize = Math.Min(
                pageSize,
                _pageSizeLimit);

            var query = _context.Technicians
                .Include(technician => technician.User)
                .AsNoTracking()
                .AsQueryable();

            if (!includeInactive)
            {
                query = query.Where(technician =>
                    technician.IsActive);
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim();

                query = query.Where(technician =>
                    technician.Specialty.Contains(term) ||
                    technician.User.FirstName.Contains(term) ||
                    technician.User.LastName.Contains(term) ||
                    (technician.User.Email != null &&
                     technician.User.Email.Contains(term)));
            }

            var totalItems = await query.CountAsync();

            var totalPages = (int)Math.Ceiling(
                (double)totalItems / pageSize);

            var technicians = await query
                .OrderBy(technician =>
                    technician.User.FirstName)
                .ThenBy(technician =>
                    technician.User.LastName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new ResponseDto<
                PageDto<List<TechnicianDto>>>
            {
                StatusCode = HttpStatusCode.OK,
                Status = true,
                Message =
                    "Técnicos encontrados correctamente.",
                Data = new PageDto<List<TechnicianDto>>
                {
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalItems = totalItems,
                    TotalPages = totalPages,
                    HasNextPage = page < totalPages,
                    HasPreviousPage = page > 1,
                    Items =
                        TechnicianMapper.ListEntityToListDto(
                            technicians)
                }
            };
        }

        public async Task<ResponseDto<TechnicianDto>>
            GetOneAsync(string userId)
        {
            if (!Guid.TryParse(userId, out _))
            {
                return Error<TechnicianDto>(
                    HttpStatusCode.BAD_REQUEST,
                    "El identificador no es válido.");
            }

            var technician = await _context.Technicians
                .Include(technician => technician.User)
                .AsNoTracking()
                .FirstOrDefaultAsync(technician =>
                    technician.UserId == userId);

            if (technician is null)
            {
                return Error<TechnicianDto>(
                    HttpStatusCode.NOT_FOUND,
                    "No se encontró el técnico.");
            }

            return new ResponseDto<TechnicianDto>
            {
                StatusCode = HttpStatusCode.OK,
                Status = true,
                Message =
                    "Técnico encontrado correctamente.",
                Data = TechnicianMapper.EntityToDto(
                    technician)
            };
        }

        public async Task<
            ResponseDto<TechnicianActionResponseDto>>
            CreateAsync(TechnicianCreateDto dto)
        {
            if (!Guid.TryParse(dto.UserId, out _))
            {
                return Error<TechnicianActionResponseDto>(
                    HttpStatusCode.BAD_REQUEST,
                    "El identificador del usuario no es válido.");
            }

            var user = await _userManager.FindByIdAsync(
                dto.UserId);

            if (user is null)
            {
                return Error<TechnicianActionResponseDto>(
                    HttpStatusCode.NOT_FOUND,
                    "No se encontró el usuario.");
            }

            if (!user.IsActive)
            {
                return Error<TechnicianActionResponseDto>(
                    HttpStatusCode.BAD_REQUEST,
                    "El usuario se encuentra inactivo.");
            }

            var hasTechnicianRole =
                await _userManager.IsInRoleAsync(
                    user,
                    RolesConstant.Technician);

            if (!hasTechnicianRole)
            {
                return Error<TechnicianActionResponseDto>(
                    HttpStatusCode.BAD_REQUEST,
                    "El usuario no tiene el rol Technician.");
            }

            var alreadyExists =
                await _context.Technicians.AnyAsync(
                    technician =>
                        technician.UserId == dto.UserId);

            if (alreadyExists)
            {
                return Error<TechnicianActionResponseDto>(
                    HttpStatusCode.CONFLICT,
                    "El usuario ya tiene un perfil de técnico.");
            }

            var technician =
                TechnicianMapper.CreateDtoToEntity(dto);

            technician.User = user;

            try
            {
                await _context.Technicians.AddAsync(
                    technician);

                await _context.SaveChangesAsync();

                return SuccessAction(
                    HttpStatusCode.CREATED,
                    "Técnico creado correctamente.",
                    technician.UserId);
            }
            catch (DbUpdateException)
            {
                return Error<TechnicianActionResponseDto>(
                    HttpStatusCode.CONFLICT,
                    "No fue posible crear el técnico.");
            }
            catch
            {
                return InternalServerError();
            }
        }

        public async Task<
            ResponseDto<TechnicianActionResponseDto>>
            EditAsync(
                string userId,
                TechnicianEditDto dto)
        {
            var technician = await _context.Technicians
                .Include(technician => technician.User)
                .FirstOrDefaultAsync(technician =>
                    technician.UserId == userId);

            if (technician is null)
            {
                return Error<TechnicianActionResponseDto>(
                    HttpStatusCode.NOT_FOUND,
                    "No se encontró el técnico.");
            }

            if (dto.IsActive)
            {
                var hasTechnicianRole =
                    await _userManager.IsInRoleAsync(
                        technician.User,
                        RolesConstant.Technician);

                if (!technician.User.IsActive ||
                    !hasTechnicianRole)
                {
                    return Error<
                        TechnicianActionResponseDto>(
                        HttpStatusCode.BAD_REQUEST,
                        "El usuario no está activo o no tiene el rol Technician.");
                }
            }

            TechnicianMapper.EditDtoToEntity(
                technician,
                dto);

            try
            {
                await _context.SaveChangesAsync();

                return SuccessAction(
                    HttpStatusCode.OK,
                    "Técnico actualizado correctamente.",
                    technician.UserId);
            }
            catch
            {
                return InternalServerError();
            }
        }

        public async Task<
            ResponseDto<TechnicianActionResponseDto>>
            DeleteAsync(string userId)
        {
            var technician = await _context.Technicians
                .FirstOrDefaultAsync(technician =>
                    technician.UserId == userId);

            if (technician is null)
            {
                return Error<TechnicianActionResponseDto>(
                    HttpStatusCode.NOT_FOUND,
                    "No se encontró el técnico.");
            }

            var hasOpenAppointments =
                await _context.Appointments
                    .AsNoTracking()
                    .AnyAsync(appointment =>
                        appointment.TechnicianId == userId &&
                        appointment.Status !=
                            AppointmentStatus.Completed &&
                        appointment.Status !=
                            AppointmentStatus.Cancelled);

            if (hasOpenAppointments)
            {
                return Error<TechnicianActionResponseDto>(
                    HttpStatusCode.CONFLICT,
                    "El técnico tiene citas pendientes que deben reasignarse.");
            }

            if (!technician.IsActive)
            {
                return SuccessAction(
                    HttpStatusCode.OK,
                    "El técnico ya se encontraba inactivo.",
                    technician.UserId);
            }

            technician.IsActive = false;

            try
            {
                await _context.SaveChangesAsync();

                return SuccessAction(
                    HttpStatusCode.OK,
                    "Técnico desactivado correctamente.",
                    technician.UserId);
            }
            catch
            {
                return InternalServerError();
            }
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

        private static ResponseDto<
            TechnicianActionResponseDto>
            SuccessAction(
                int statusCode,
                string message,
                string id)
        {
            return new ResponseDto<
                TechnicianActionResponseDto>
            {
                StatusCode = statusCode,
                Status = true,
                Message = message,
                Data = new TechnicianActionResponseDto
                {
                    Id = id
                }
            };
        }

        private static ResponseDto<
            TechnicianActionResponseDto>
            InternalServerError()
        {
            return Error<TechnicianActionResponseDto>(
                HttpStatusCode.INTERNAL_SERVER_ERROR,
                "Ocurrió un error interno en el servidor.");
        }
    }
}