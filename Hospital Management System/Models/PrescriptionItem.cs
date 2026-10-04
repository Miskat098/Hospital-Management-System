using Hospital_Management_System.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hospital_Management_System.Models
{
    /// <summary>
    /// PRESCRIPTION_ITEMS — the junction table between PRESCRIPTION and
    /// MEDICINE, carrying Dosage, Frequency, and Duration per the ER
    /// diagram fixes.
    /// </summary>
    public class PrescriptionItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string PrescriptionId { get; set; } = string.Empty;
        public Prescription Prescription { get; set; } = null!;

        [Required]
        public string MedicineId { get; set; } = string.Empty;
        public Medicine Medicine { get; set; } = null!;

        [Required, MaxLength(50)]
        public string Dosage { get; set; } = string.Empty; // e.g. "500mg", "1 tab"

        [Required, MaxLength(50)]
        public string Frequency { get; set; } = string.Empty; // e.g. "Once daily"

        [Required, MaxLength(50)]
        public string Duration { get; set; } = string.Empty; // e.g. "7 days"

        public int Quantity { get; set; } = 1;
    }
}