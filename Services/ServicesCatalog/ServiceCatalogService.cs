using AutoCare_Club.Api.Database;
using AutoCare_Club.Api.Dtos.Services;
using AutoCare_Club.Api.Entities;
using AutoCare_Club.Api.Mappers;
using Microsoft.EntityFrameworkCore;

namespace AutoCare_Club.Api.Services.ServicesCatalog
{
    public class ServiceCatalogService : IServiceCatalogService
    {
        private readonly AutoCareDbContext _context;

        public ServiceCatalogService(AutoCareDbContext context)
        {
            _context = context;
        }

        public async Task<List<ServiceDto>> GetAllAsync(
            bool includeInactive = false)
        {
            IQueryable<ServiceEntity> query =
                _context.Services.AsNoTracking();

            if (!includeInactive)
            {
                query = query.Where(service => service.IsActive);
            }

            List<ServiceEntity> services = await query
                .OrderBy(service => service.Name)
                .ToListAsync();

            return ServiceMapper.ListEntityToListDto(services);
        }

        public async Task<ServiceDto?> GetByIdAsync(
            int id,
            bool includeInactive = false)
        {
            IQueryable<ServiceEntity> query =
                _context.Services.AsNoTracking();

            if (!includeInactive)
            {
                query = query.Where(service => service.IsActive);
            }

            ServiceEntity? service = await query
                .FirstOrDefaultAsync(service => service.Id == id);

            if (service is null)
            {
                return null;
            }

            return ServiceMapper.EntityToDto(service);
        }

        public async Task<ServiceDto> CreateAsync(
            ServiceCreateDto dto)
        {
            ServiceEntity service =
                ServiceMapper.CreateDtoToEntity(dto);

            await _context.Services.AddAsync(service);
            await _context.SaveChangesAsync();
            return ServiceMapper.EntityToDto(service);
        }

        public async Task<ServiceDto?> EditAsync(
            int id,
            ServiceEditDto dto)
        {
            ServiceEntity? service = await _context.Services
                .FirstOrDefaultAsync(service => service.Id == id);

            if (service is null)
            {
                return null;
            }

            ServiceMapper.EditDtoToEntity(service, dto);
            await _context.SaveChangesAsync();
            return ServiceMapper.EntityToDto(service);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            ServiceEntity? service = await _context.Services
                .FirstOrDefaultAsync(service => service.Id == id);

            if (service is null)
            {
                return false;
            }

            service.IsActive = false;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}