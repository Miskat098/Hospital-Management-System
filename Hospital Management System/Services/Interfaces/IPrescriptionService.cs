using Hospital_Management_System.DTOs;

namespace Hospital_Management_System.Services.Interfaces
{
    public interface IPrescriptionService
    {
        /// <summary>Returns prescriptions for a patient (patient/prescriptions.html)
        /// or, when patientId is null, all prescriptions written by a doctor.</summary>
        Task<List<PrescriptionDto>> GetForPatientAsync(string patientId);
        Task<List<PrescriptionDto>> GetForDoctorAsync(string doctorId);
        Task<PrescriptionDto?> GetByIdAsync(string id);
        Task<PrescriptionDto> CreateAsync(string doctorId, CreatePrescriptionDto dto);
    }
}