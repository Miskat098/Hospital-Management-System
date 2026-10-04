using Microsoft.EntityFrameworkCore;
using Hospital_Management_System.Data;
using Hospital_Management_System.DTOs;
using Hospital_Management_System.Models;
using Hospital_Management_System.Services.Interfaces;

namespace Hospital_Management_System.Services.Implementations
{
    public class DepartmentService : IDepartmentService
    {
        private readonly ApplicationDbContext _context;

        public DepartmentService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<DepartmentDto>> GetAllAsync()
        {
            return await _context.Departments
                .Include(d => d.HeadDoctor).ThenInclude(hd => hd!.User)
                .Include(d => d.Doctors)
                .Select(d => new DepartmentDto
                {
                    Id = d.Id,
                    Name = d.Name,
                    Description = d.Description,
                    HeadDoctorId = d.HeadDoctorId,
                    HeadDoctorName = d.HeadDoctor != null ? d.HeadDoctor.User.FullName : null,
                    DoctorCount = d.Doctors.Count,
                    PatientCount = _context.Appointments
                        .Where(a => a.Doctor.DepartmentId == d.Id)
                        .Select(a => a.PatientId)
                        .Distinct()
                        .Count(),
                    Status = d.Status
                })
                .ToListAsync();
        }

        public async Task<DepartmentDto?> GetByIdAsync(int id)
        {
            return await GetAllAsync() is var all
                ? all.FirstOrDefault(d => d.Id == id)
                : null;
        }

        public async Task<DepartmentDto> CreateAsync(DepartmentCreateUpdateDto dto)
        {
            var department = new Department
            {
                Name = dto.Name,
                Description = dto.Description,
                HeadDoctorId = dto.HeadDoctorId,
                Status = dto.Status,
                CreatedAt = DateTime.UtcNow
            };

            _context.Departments.Add(department);
            await _context.SaveChangesAsync();

            return new DepartmentDto
            {
                Id = department.Id,
                Name = department.Name,
                Description = department.Description,
                HeadDoctorId = department.HeadDoctorId,
                DoctorCount = 0,
                PatientCount = 0,
                Status = department.Status
            };
        }

        public async Task<DepartmentDto?> UpdateAsync(int id, DepartmentCreateUpdateDto dto)
        {
            var department = await _context.Departments.FindAsync(id);
            if (department == null) return null;

            department.Name = dto.Name;
            department.Description = dto.Description;
            department.HeadDoctorId = dto.HeadDoctorId;
            department.Status = dto.Status;

            await _context.SaveChangesAsync();
            return await GetByIdAsync(id);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var department = await _context.Departments.FindAsync(id);
            if (department == null) return false;

            var hasDoctors = await _context.Doctors.AnyAsync(d => d.DepartmentId == id);
            if (hasDoctors)
            {
                throw new InvalidOperationException("Reassign doctors before deleting this department.");
            }

            _context.Departments.Remove(department);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}