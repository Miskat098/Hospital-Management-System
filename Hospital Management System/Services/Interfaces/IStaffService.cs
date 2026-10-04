using Hospital_Management_System.DTOs;

namespace Hospital_Management_System.Services.Interfaces
{
    public interface IStaffService
    {
        /// <summary>Returns all Doctor/Patient/Staff accounts for the
        /// unified table on admin/staff-management.html. Pass a role to
        /// filter server-side, or null for "All".</summary>
        Task<List<StaffDto>> GetAllAsync(string? role = null);

        Task<StaffDto?> GetByIdAsync(string id);
        Task<StaffDto> CreateAsync(StaffCreateUpdateDto dto);
        Task<StaffDto?> UpdateAsync(string id, StaffCreateUpdateDto dto);
        Task<bool> DeleteAsync(string id);
    }
}