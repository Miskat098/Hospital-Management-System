using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Hospital_Management_System.DTOs;
using Hospital_Management_System.Models;
using Hospital_Management_System.Services.Interfaces;

namespace Hospital_Management_System.Controllers
{
    /// <summary>
    /// Patient Feedback Controller
    /// POST /api/feedback, GET /api/feedback?patientId=...
    /// </summary>
    [ApiController]
    [Route("api/feedback")]
    [Authorize]
    public class FeedbackController : ControllerBase
    {
        private readonly IFeedbackService _feedbackService;

        public FeedbackController(IFeedbackService feedbackService)
        {
            _feedbackService = feedbackService;
        }

        [HttpGet]
        [Authorize(Roles = UserRoles.Patient)]
        public async Task<IActionResult> Get([FromQuery] string patientId)
        {
            var entries = await _feedbackService.GetForPatientAsync(patientId);
            return Ok(entries);
        }

        [HttpPost]
        [Authorize(Roles = UserRoles.Patient)]
        public async Task<IActionResult> Submit([FromBody] SubmitFeedbackDto dto)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            var patientId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                            ?? User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;

            if (string.IsNullOrEmpty(patientId))
            {
                return Unauthorized(new { message = "Patient identification failed." });
            }

            try
            {
                var created = await _feedbackService.SubmitAsync(patientId, dto);
                return Ok(created);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
            }
        }
    }
}