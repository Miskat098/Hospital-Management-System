using Hospital_Management_System.DTOs;

namespace Hospital_Management_System.Services.Interfaces
{
    public interface IBillingService
    {
        Task<List<InvoiceDto>> GetInvoicesForPatientAsync(string patientId);
        Task<List<InvoiceDto>> GetAllInvoicesAsync(string? status = null);
        Task<InvoiceDto?> GetByIdAsync(string id);
        Task<InvoiceDto?> PayAsync(string id, PayInvoiceDto dto);

        /// <summary>Powers the KPI cards + revenue trend chart on
        /// admin/billing-overview.html.</summary>
        Task<RevenueSummaryDto> GetRevenueSummaryAsync(string period);
    }
}