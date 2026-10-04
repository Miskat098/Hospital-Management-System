using Hospital_Management_System.DTOs;

namespace Hospital_Management_System.Services.Interfaces
{
    public interface IMedicineService
    {
        Task<List<MedicineDto>> GetAllAsync();
        Task<MedicineDto?> GetByIdAsync(string id);
        Task<MedicineDto> CreateAsync(MedicineCreateUpdateDto dto);
        Task<MedicineDto?> UpdateAsync(string id, MedicineCreateUpdateDto dto);
        Task<bool> DeleteAsync(string id);

        /// <summary>Lightweight list for populating the medicine <select>
        /// on the doctor's write-prescription.html page.</summary>
        Task<List<MedicineOptionDto>> GetOptionsAsync();
    }
}