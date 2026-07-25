using AutoCare_Club.Api.Dtos.Services;

namespace AutoCare_Club.Api.Services.ServicesCatalog
{
    public interface IServiceCatalogService
    {
        Task<List<ServiceDto>> GetAllAsync(
           bool includeInactive = false);

        Task<ServiceDto?> GetByIdAsync(
            string id,
            bool includeInactive = false
        );

        Task<ServiceDto> CreateAsync(ServiceCreateDto dto);
        Task<ServiceDto?> EditAsync(
            string id,
            ServiceEditDto dto
        );

        Task<bool> DeleteAsync(string id);
    }
}
