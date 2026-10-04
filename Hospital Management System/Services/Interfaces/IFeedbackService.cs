using Hospital_Management_System.DTOs;

namespace Hospital_Management_System.Services.Interfaces
{
    public interface IFeedbackService
    {
        Task<List<FeedbackDto>> GetForPatientAsync(string patientId);
        Task<FeedbackDto> SubmitAsync(string patientId, SubmitFeedbackDto dto);
    }
}