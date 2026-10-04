using System.ComponentModel.DataAnnotations;

namespace Hospital_Management_System.DTOs
{
    /// <summary>
    /// Mirrors the payload sent by loginForm in wwwroot/js/auth.js:
    /// { email, password, role }
    /// </summary>
    public class LoginRequestDto
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = string.Empty; // "Admin" | "Doctor" | "Patient"
    }

    /// <summary>
    /// Mirrors the payload built by registerForm in wwwroot/js/auth.js.
    /// Registration is patient-only, per staff-management.html's note
    /// that admin creates Doctor/Staff accounts directly.
    /// </summary>
    public class RegisterRequestDto
    {
        [Required, MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, RegularExpression(@"^01[0-9]{9}$", ErrorMessage = "Enter a valid 11-digit BD phone number.")]
        public string Phone { get; set; } = string.Empty;

        [Required]
        public DateTime Dob { get; set; }

        [Required]
        public string Gender { get; set; } = string.Empty;

        [Required, MinLength(8)]
        public string Password { get; set; } = string.Empty;
    }

    /// <summary>
    /// Shape returned to the frontend as `result.user` after login/register.
    /// Matches how auth.js reads result.user.name / result.user.role.
    /// </summary>
    public class UserDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string? Department { get; set; } // populated for Doctors
        public string Status { get; set; } = "Active";
    }

    /// <summary>
    /// Matches { token, user } expected by MediCoreAPI.Auth.login() in api.js.
    /// </summary>
    public class LoginResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public UserDto User { get; set; } = null!;
    }
}