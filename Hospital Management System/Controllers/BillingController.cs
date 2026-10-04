using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Hospital_Management_System.DTOs;
using Hospital_Management_System.Models;
using Hospital_Management_System.Services.Interfaces;

namespace Hospital_Management_System.Controllers
{
    /// <summary>
    /// Billing Management Controller
    /// GET /api/billing/invoices?patientId=..., POST /api/billing/invoices/{id}/pay
    /// GET /api/billing/summary (Admin Revenue Overview)
    /// </summary>
    [ApiController]
    [Route("api/billing")]
    [Authorize]
    public class BillingController : ControllerBase
    {
        private readonly IBillingService _billingService;

        public BillingController(IBillingService billingService)
        {
            _billingService = billingService;
        }

        [HttpGet("invoices")]
        [Authorize(Roles = UserRoles.Admin + "," + UserRoles.Patient)]
        public async Task<IActionResult> GetInvoices([FromQuery] string? patientId, [FromQuery] string? status)
        {
            if (!string.IsNullOrWhiteSpace(patientId))
            {
                return Ok(await _billingService.GetInvoicesForPatientAsync(patientId));
            }

            return Ok(await _billingService.GetAllInvoicesAsync(status));
        }

        [HttpGet("invoices/{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var invoice = await _billingService.GetByIdAsync(id);
            return invoice == null
                ? NotFound(new { message = "Invoice not found." })
                : Ok(invoice);
        }

        [HttpPost("invoices/{id}/pay")]
        [Authorize(Roles = UserRoles.Patient)]
        public async Task<IActionResult> Pay(string id, [FromBody] PayInvoiceDto dto)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            try
            {
                var paid = await _billingService.PayAsync(id, dto);
                return paid == null
                    ? NotFound(new { message = "Invoice not found." })
                    : Ok(new { success = true, invoiceId = id, invoice = paid });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("summary")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> GetSummary([FromQuery] string period = "30d")
        {
            var summary = await _billingService.GetRevenueSummaryAsync(period);
            return Ok(summary);
        }
    }
}