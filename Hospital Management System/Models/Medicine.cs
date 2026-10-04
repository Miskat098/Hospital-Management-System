using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hospital_Management_System.Models
{
    /// <summary>MEDICINE — referenced by PRESCRIPTION_ITEMS as a junction target.</summary>
    public class Medicine
    {
        [Key]
        [MaxLength(20)]
        public string Id { get; set; } = string.Empty; // e.g. "MED-0001"

        [Required, MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Category { get; set; } = string.Empty; // Analgesic, Antibiotic, Cardiac, etc.

        [MaxLength(30)]
        public string Form { get; set; } = "Tablet"; // Tablet, Capsule, Syrup, Injection, Ointment

        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }

        public int StockQuantity { get; set; }

        public DateTime? ExpiryDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public ICollection<PrescriptionItem> PrescriptionItems { get; set; } = new List<PrescriptionItem>();

        /// <summary>"In Stock" | "Low Stock" | "Critical" — computed, not stored.</summary>
        [NotMapped]
        public string StockStatus =>
            StockQuantity <= 20 ? "Critical" :
            StockQuantity <= 150 ? "Low Stock" : "In Stock";
    }
}