using Hospital_Management_System.DTOs;

namespace Hospital_Management_System.Services.Interfaces
{
    public interface IAuthService
    {
        /// <summary>
        /// Validates credentials AND role against ASP.NET Identity, matching
        /// the three-way check (email + password + role) performed by the
        /// mock MOCK_USERS.find() in api.js.
        /// </summary>
        Task<LoginResponseDto> LoginAsync(LoginRequestDto request);

        /// <summary>Creates a new Patient account (registration is patient-only).</summary>
        Task<UserDto> RegisterAsync(RegisterRequestDto request);

        /// <summary>
        /// No server-side session to invalidate with stateless JWTs; this
        /// exists so the endpoint shape matches MediCoreAPI.Auth.logout()
        /// and leaves room for a future refresh-token blocklist.
        /// </summary>
        Task LogoutAsync(string userId);
    }
}