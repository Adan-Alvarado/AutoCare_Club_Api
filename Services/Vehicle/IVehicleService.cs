using AutoCare_Club.Api.Dtos.Vehicle;

namespace AutoCare_Club.Api.Services.Vehicle
{
    public interface IVehicleService
    {
        Task<List<VehicleDto>> GetAllAsync(
           bool includeInactive = false);

        Task<VehicleDto?> GetByIdAsync(
            string id,
            bool includeInactive = false
        );

        Task<VehicleDto> CreateAsync(VehicleCreateDto dto);
        Task<VehicleDto?> EditAsync(
            string id,
            VehicleEditDto dto
        );

        Task<bool> DeleteAsync(string id);
    }
}
