using AutoCare_Club_Api.Dtos.Common;
using AutoCare_Club_Api.Dtos.Technicians;

namespace AutoCare_Club_Api.Services.Technicians
{
    public interface ITechnicianService
    {
        Task<ResponseDto<PageDto<List<TechnicianDto>>>>
            GetPageAsync(
                string searchTerm = "",
                int page = 1,
                int pageSize = 10,
                bool includeInactive = false);

        Task<ResponseDto<TechnicianDto>> GetOneAsync(
            string userId);

        Task<ResponseDto<TechnicianActionResponseDto>>
            CreateAsync(TechnicianCreateDto dto);

        Task<ResponseDto<TechnicianActionResponseDto>>
            EditAsync(
                string userId,
                TechnicianEditDto dto);

        Task<ResponseDto<TechnicianActionResponseDto>>
            DeleteAsync(string userId);
    }
}
