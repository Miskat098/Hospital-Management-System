using Hospital_Management_System.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hospital_Management_System.Models
{
    /// <summary>
    /// BILLING — one invoice per Appointment (1-to-1), matching the ER
    /// diagram's APPOINTMENT -> BILLING link. Line items are stored as a
    /// simple owned collection since invoices are generated server-side
    /// and rarely re-itemized after creation.
    /// </summary>
    public class Invoice
    {
        [Key]
        [MaxLength(20)]
        public string Id { get; set; } = string.Empty; // e.g. "INV-20261901"

        [Required]
        public string AppointmentId { get; set; } = string.Empty;
        public Appointment Appointment { get; set; } = null!;

        [Required]
        public string PatientId { get; set; } = string.Empty;
        public Patient Patient { get; set; } = null!;

        [Column(TypeName = "decimal(10,2)")]
        public decimal ConsultationFee { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal TestFees { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal ServiceCharge { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalAmount { get; set; }

        public InvoiceStatus Status { get; set; } = InvoiceStatus.Pending;

        [MaxLength(30)]
        public string? PaymentMethod { get; set; } // card, bkash, nagad, bank

        [MaxLength(100)]
        public string? TransactionReference { get; set; }

        public DateTime IssuedAt { get; set; } = DateTime.UtcNow;
        public DateTime DueDate { get; set; }
        public DateTime? PaidAt { get; set; }
    }
}