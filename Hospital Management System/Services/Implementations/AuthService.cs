using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Hospital_Management_System.Data;
using Hospital_Management_System.DTOs;
using Hospital_Management_System.Helpers;
using Hospital_Management_System.Models;
using Hospital_Management_System.Services.Interfaces;

namespace Hospital_Management_System.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly JwtTokenGenerator _tokenGenerator;
        private readonly ApplicationDbContext _context;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            JwtTokenGenerator tokenGenerator,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenGenerator = tokenGenerator;
            _context = context;
        }

        public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                throw new UnauthorizedAccessException("Invalid email or password.");
            }

            var passwordValid = await _userManager.CheckPasswordAsync(user, request.Password);
            if (!passwordValid)
            {
                throw new UnauthorizedAccessException("Invalid email or password.");
            }

            var roles = await _userManager.GetRolesAsync(user);
            if (!roles.Contains(request.Role))
            {
                throw new UnauthorizedAccessException("Invalid email or password.");
            }

            if (user.Status == "Suspended")
            {
                throw new UnauthorizedAccessException("This account has been suspended. Contact an administrator.");
            }

            var token = _tokenGenerator.GenerateToken(user, roles);

            string? department = null;
            if (request.Role == Models.UserRoles.Doctor)
            {
                var doctor = await _context.Doctors
                    .Include(d => d.Department)
                    .FirstOrDefaultAsync(d => d.Id == user.Id);
                department = doctor?.Department.Name;
            }

            return new LoginResponseDto
            {
                Token = token,
                User = new UserDto
                {
                    Id = user.Id,
                    Name = user.FullName,
                    Email = user.Email ?? string.Empty,
                    Role = request.Role,
                    Department = department,
                    Status = user.Status
                }
            };
        }

        public async Task<UserDto> RegisterAsync(RegisterRequestDto request)
        {
            var existing = await _userManager.FindByEmailAsync(request.Email);
            if (existing != null)
            {
                throw new InvalidOperationException("An account with this email already exists.");
            }

            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                PhoneNumber = request.Phone,
                DateOfBirth = request.Dob,
                Gender = request.Gender,
                PrimaryRole = Models.UserRoles.Patient,
                Status = "Active",
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(" ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException(errors);
            }

            await _userManager.AddToRoleAsync(user, Models.UserRoles.Patient);

            _context.Patients.Add(new Patient
            {
                Id = user.Id,
                RegisteredAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            return new UserDto
            {
                Id = user.Id,
                Name = user.FullName,
                Email = user.Email ?? string.Empty,
                Role = Models.UserRoles.Patient,
                Status = user.Status
            };
        }

        public Task LogoutAsync(string userId)
        {
            // Stateless JWTs — nothing to invalidate server-side today.
            // Reserved for a future refresh-token/blocklist implementation.
            return Task.CompletedTask;
        }
    }
}