using AutoCare_Club_Api.Dtos.Appointments;
using AutoCare_Club_Api.Dtos.Common;

namespace AutoCare_Club_Api.Services.Appointments
{
    public interface IAppointmentService
    {
        Task<ResponseDto<List<AppointmentDto>>> GetAllAsync();

        Task<ResponseDto<List<AppointmentDto>>> GetMineAsync(string userId);

        Task<ResponseDto<List<AppointmentDto>>>
            GetTechnicianAppointmentsAsync(string technicianId);

        Task<ResponseDto<AppointmentDto>> GetOneAsync(string id, string userId, bool canManage);

        Task<ResponseDto<AppointmentActionResponseDto>> CreateAsync(string userId, AppointmentCreateDto dto);

        Task<ResponseDto<AppointmentActionResponseDto>> EditAsync(string id, AppointmentEditDto dto);

        Task<ResponseDto<AppointmentActionResponseDto>>
            UpdateStatusByTechnicianAsync(
                string appointmentId,
                string technicianId,
                AppointmentStatusEditDto dto);

        Task<ResponseDto<AppointmentActionResponseDto>> CancelAsync(string id,string userId, bool canManage);

        Task<ResponseDto<AppointmentActionResponseDto>> DeleteAsync(string id);
    }
}
