using AutoCare_Club_Api.Dtos.Common;
using AutoCare_Club_Api.Dtos.Schedules;

namespace AutoCare_Club_Api.Services.Schedules
{
    public interface IScheduleService
    {
        Task<ResponseDto<List<ScheduleDto>>> GetAllAsync();

        Task<ResponseDto<ScheduleDto>> GetOneAsync(string id);

        Task<ResponseDto<ScheduleActionResponseDto>> CreateAsync(ScheduleCreateDto dto);

        Task<ResponseDto<ScheduleActionResponseDto>> EditAsync(string id, ScheduleEditDto dto);

        Task<ResponseDto<ScheduleActionResponseDto>> DeleteAsync(string id);

        Task<ResponseDto<List<ScheduleAvailabilityDto>>>
            GetAvailableAsync(
                string serviceId,
                DateOnly date,
                string? userId);
    }
}
