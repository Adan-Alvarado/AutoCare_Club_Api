using AutoCare_Club.Api.Dtos.Vehicle;

namespace AutoCare_Club.Api.Services.Vehicle
{
    public interface IVehicleService
    {
        Task<List<VehicleDto>> GetAllAsync(
            string userId,
            bool includeInactive = false);

        Task<VehicleDto?> GetByIdAsync(
            string id,
            string userId,
            bool includeInactive = false
        );

        Task<VehicleDto> CreateAsync(
            VehicleCreateDto dto,
            string userId);

        Task<VehicleDto?> EditAsync(
            string id,
            string userId,
            VehicleEditDto dto
        );

        Task<bool> DeleteAsync(
            string id,
            string userId);
    }
}
