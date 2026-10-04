using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Hospital_Management_System.Data;
using Hospital_Management_System.DTOs;
using Hospital_Management_System.Models;
using Hospital_Management_System.Services.Interfaces;

namespace Hospital_Management_System.Services.Implementations
{
    public class StaffService : IStaffService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public StaffService(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<List<StaffDto>> GetAllAsync(string? role = null)
        {
            var query = _context.Users
                .Include(u => u.DoctorProfile).ThenInclude(d => d!.Department)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(role) && role != "all")
            {
                var capitalizedRole = char.ToUpper(role[0]) + role[1..].ToLower();
                query = query.Where(u => u.PrimaryRole == capitalizedRole);
            }

            var users = await query.OrderByDescending(u => u.CreatedAt).ToListAsync();

            return users.Select(u => new StaffDto
            {
                Id = u.Id,
                Name = u.FullName,
                Role = u.PrimaryRole,
                Email = u.Email ?? string.Empty,
                Phone = u.PhoneNumber,
                Department = u.DoctorProfile?.Department.Name,
                Status = u.Status,
                JoinedAt = u.CreatedAt
            }).ToList();
        }

        public async Task<StaffDto?> GetByIdAsync(string id)
        {
            var user = await _context.Users
                .Include(u => u.DoctorProfile).ThenInclude(d => d!.Department)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null) return null;

            return new StaffDto
            {
                Id = user.Id,
                Name = user.FullName,
                Role = user.PrimaryRole,
                Email = user.Email ?? string.Empty,
                Phone = user.PhoneNumber,
                Department = user.DoctorProfile?.Department.Name,
                Status = user.Status,
                JoinedAt = user.CreatedAt
            };
        }

        public async Task<StaffDto> CreateAsync(StaffCreateUpdateDto dto)
        {
            var user = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                PhoneNumber = dto.Phone,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                PrimaryRole = dto.Role,
                Status = dto.Status,
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };

            // Temporary password — in production this should trigger an
            // invite email with a secure password-set link instead.
            var tempPassword = $"Temp@{Guid.NewGuid().ToString()[..8]}";
            var result = await _userManager.CreateAsync(user, tempPassword);
            if (!result.Succeeded)
            {
                var errors = string.Join(" ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException(errors);
            }

            await _userManager.AddToRoleAsync(user, dto.Role);

            if (dto.Role == UserRoles.Doctor)
            {
                // Accept either a department Id (string of an int) or a department name.
                Department? department = null;
                if (!string.IsNullOrWhiteSpace(dto.Department))
                {
                    if (int.TryParse(dto.Department, out var parsedId))
                    {
                        department = await _context.Departments.FirstOrDefaultAsync(d => d.Id == parsedId);
                    }
                    else
                    {
                        department = await _context.Departments.FirstOrDefaultAsync(d => d.Name.Equals(dto.Department, StringComparison.OrdinalIgnoreCase));
                    }
                }

                if (department == null)
                {
                    // Fail fast: creating a doctor without a valid department is invalid.
                    throw new InvalidOperationException("Department not found.");
                }

                _context.Doctors.Add(new Doctor
                {
                    Id = user.Id,
                    DepartmentId = department.Id,
                    Specialization = department.Name ?? string.Empty
                });
            }
            else if (dto.Role == UserRoles.Patient)
            {
                _context.Patients.Add(new Patient { Id = user.Id });
            }

            await _context.SaveChangesAsync();

            return (await GetByIdAsync(user.Id))!;
        }

        public async Task<StaffDto?> UpdateAsync(string id, StaffCreateUpdateDto dto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return null;

            user.FirstName = dto.FirstName;
            user.LastName = dto.LastName;
            user.Email = dto.Email;
            user.UserName = dto.Email;
            user.PhoneNumber = dto.Phone;
            user.Status = dto.Status;

            if (dto.Role == UserRoles.Doctor)
            {
                var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.Id == id);
                if (doctor != null && !string.IsNullOrWhiteSpace(dto.Department))
                {
                    Department? department = null;
                    if (int.TryParse(dto.Department, out var parsedId))
                    {
                        department = await _context.Departments.FirstOrDefaultAsync(d => d.Id == parsedId);
                    }
                    else
                    {
                        department = await _context.Departments.FirstOrDefaultAsync(d => d.Name.Equals(dto.Department, StringComparison.OrdinalIgnoreCase));
                    }

                    if (department != null)
                    {
                        doctor.DepartmentId = department.Id;
                    }
                    else
                    {
                        // Leave unchanged if the supplied department is invalid; caller may expect validation earlier.
                    }
                }
            }

            await _context.SaveChangesAsync();
            return await GetByIdAsync(id);
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return false;

            var hasAppointments = await _context.Appointments
                .AnyAsync(a => a.PatientId == id || a.DoctorId == id);
            if (hasAppointments)
            {
                // Soft-delete instead of a hard delete to preserve referential
                // integrity with historical appointments/prescriptions/invoices.
                user.Status = "Suspended";
                await _context.SaveChangesAsync();
                return true;
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}