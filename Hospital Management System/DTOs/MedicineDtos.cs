using System.ComponentModel.DataAnnotations;

namespace Hospital_Management_System.DTOs
{
    /// <summary>
    /// Matches each row rendered in
    /// wwwroot/pages/admin/medicine-inventory.html, including the
    /// computed StockStatus badge text ("In Stock" | "Low Stock" | "Critical").
    /// </summary>
    public class MedicineDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Form { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public string StockStatus { get; set; } = "In Stock";
        public DateTime? ExpiryDate { get; set; }
    }

    /// <summary>
    /// Matches the payload built by admin/inventory.js from
    /// #medicineForm: { name, category, form, price, stock, expiryDate }
    /// </summary>
    public class MedicineCreateUpdateDto
    {
        [Required, MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string Category { get; set; } = string.Empty;

        [Required, MaxLength(30)]
        public string Form { get; set; } = string.Empty;

        [Required, Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        [Required, Range(0, int.MaxValue)]
        public int Stock { get; set; }

        public DateTime? ExpiryDate { get; set; }
    }

    /// <summary>Lightweight shape used to populate medicine <select> options
    /// on wwwroot/pages/doctor/write-prescription.html.</summary>
    public class MedicineOptionDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }
}