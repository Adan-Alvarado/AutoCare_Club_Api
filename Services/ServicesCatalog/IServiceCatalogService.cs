using AutoCare_Club.Api.Dtos.Services;

namespace AutoCare_Club.Api.Services.ServicesCatalog
{
    public interface IServiceCatalogService
    {
         Task<List<ServiceDto>> GetAllAsync(
            bool includeInactive = false);

        Task<ServiceDto?> GetByIdAsync(
            int id,
            bool includeInactive = false
        );

        Task<ServiceDto> CreateAsync(ServiceCreateDto dto);
        Task<ServiceDto?> EditAsync(
            int id,
            ServiceEditDto dto
        );

        Task<bool> DeleteAsync(int id);
    }
}