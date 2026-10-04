using Hospital_Management_System.DTOs;

namespace Hospital_Management_System.Services.Interfaces
{
    public interface IDepartmentService
    {
        Task<List<DepartmentDto>> GetAllAsync();
        Task<DepartmentDto?> GetByIdAsync(int id);
        Task<DepartmentDto> CreateAsync(DepartmentCreateUpdateDto dto);
        Task<DepartmentDto?> UpdateAsync(int id, DepartmentCreateUpdateDto dto);
        Task<bool> DeleteAsync(int id);
    }
}