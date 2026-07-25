using AutoCare_Club.Api.Dtos.Vehicle;
using AutoCare_Club.Api.Database;
using AutoCare_Club.Api.Entities;
using AutoCare_Club.Api.Mappers;
using Microsoft.EntityFrameworkCore;

namespace AutoCare_Club.Api.Services.Vehicle
{
    public class VehicleService : IVehicleService
    {
        private readonly AutoCareDbContext _context;
        public VehicleService(AutoCareDbContext context)
        {
            _context = context;
        }

        public async Task<List<VehicleDto>> GetAllAsync(
             bool includeInactive = false)
        {
            IQueryable<VehicleEntity> query =
                _context.Vehicles.AsNoTracking();

            if (!includeInactive)
            {
                query = query.Where(vehicle => vehicle.IsActive);
            }

            List<VehicleEntity> vehicles =
                await query.ToListAsync();

            return VehicleMapper.ListEntityToListDto(vehicles);
        }

        public async Task<VehicleDto?> GetByIdAsync(
            string id,
            bool includeInactive = false)
        {
            IQueryable<VehicleEntity> query =
                 _context.Vehicles.AsNoTracking();

            if (!includeInactive)
            {
                query = query.Where(vehicle => vehicle.IsActive);
            }
            
            VehicleEntity? vehicle = await query
            .FirstOrDefaultAsync(vehicle => vehicle.Id == id); 

            if(vehicle is null)
            {
            return null;
            }

            return VehicleMapper.EntityToDto(vehicle);
        }

        public async Task<VehicleDto> CreateAsync(VehicleCreateDto dto)
        {
            VehicleEntity vehicle =
                VehicleMapper.CreateDtoToEntity(dto);

            await _context.Vehicles.AddAsync(vehicle);
            await _context.SaveChangesAsync();
            return VehicleMapper.EntityToDto(vehicle);
        }

        public async Task<VehicleDto?> EditAsync(
            string id,
            VehicleEditDto dto)
        {
            VehicleEntity? vehicle = await _context.Vehicles
            .FirstOrDefaultAsync(vehicle => vehicle.Id == id);
            if (vehicle is null)
            {
                return null;
            }

            VehicleMapper.EditDtoToEntity(vehicle, dto);
            await _context.SaveChangesAsync();
            return VehicleMapper.EntityToDto(vehicle);
        }

        public async Task<bool> DeleteAsync(string id)
        {
            VehicleEntity? vehicle= await _context.Vehicles
                .FirstOrDefaultAsync(vehicle => vehicle.Id == id);

            if (vehicle is null)
            {
                return false;
            }

            vehicle.IsActive = false;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
