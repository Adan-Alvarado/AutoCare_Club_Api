using AutoCare_Club.Api.Dtos.Vehicle;
using AutoCare_Club.Api.Database;
using AutoCare_Club.Api.Entities;
using AutoCare_Club.Api.Mappers;

namespace AutoCare_Club.Api.Services.Vehicle
{
    public class VehicleService : IVehicleService
    {
        private readonly AutoCareDbContext _context;
        public VehicleService(AutoCareDbContext context)
        {
            _context = context;
        }

        public async Task<VehicleDto> CreateAsync(VehicleCreateDto dto)
        {
            VehicleEntity vehicle =
                VehicleMapper.CreateDtoToEntity(dto);

            await _context.Vehicles.AddAsync(vehicle);
            await _context.SaveChangesAsync();
            return VehicleMapper.EntityToDto(vehicle);
        }

        public Task<bool> DeleteAsync(string id)
        {
            throw new NotImplementedException();
        }

        public Task<VehicleDto?> EditAsync(string id, VehicleEditDto dto)
        {
            throw new NotImplementedException();
        }

        public Task<List<VehicleDto>> GetAllAsync(bool includeInactive = false)
        {
            throw new NotImplementedException();
        }

        public Task<VehicleDto?> GetByIdAsync(string id, bool includeInactive = false)
        {
            throw new NotImplementedException();
        }
    }
}
