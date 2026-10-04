using System.ComponentModel.DataAnnotations;

namespace Hospital_Management_System.DTOs
{
    /// <summary>
    /// Matches the invoice-card fields rendered on
    /// wwwroot/pages/patient/billing.html and
    /// wwwroot/pages/admin/billing-overview.html.
    /// </summary>
    public class InvoiceDto
    {
        public string Id { get; set; } = string.Empty;
        public string AppointmentId { get; set; } = string.Empty;
        public string PatientId { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string DoctorName { get; set; } = string.Empty;
        public decimal ConsultationFee { get; set; }
        public decimal TestFees { get; set; }
        public decimal ServiceCharge { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = "Pending"; // Pending | Paid | Overdue
        public string? PaymentMethod { get; set; }
        public DateTime IssuedAt { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? PaidAt { get; set; }
    }

    /// <summary>
    /// Matches the payload built by patient/billing.js
    /// confirmPaymentBtn handler: { method }
    /// </summary>
    public class PayInvoiceDto
    {
        [Required]
        public string Method { get; set; } = string.Empty; // card | bkash | nagad | bank

        public string? CardNumberLast4 { get; set; }
    }

    /// <summary>
    /// Matches the KPI cards on admin/billing-overview.html.
    /// </summary>
    public class RevenueSummaryDto
    {
        public decimal TotalRevenue { get; set; }
        public int InvoicesIssued { get; set; }
        public int PendingPayments { get; set; }
        public int OverdueInvoices { get; set; }
        public List<RevenueTrendPointDto> Trend { get; set; } = new();
    }

    public class RevenueTrendPointDto
    {
        public string Label { get; set; } = string.Empty; // week/day label
        public decimal Amount { get; set; }
    }
}