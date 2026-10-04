using Microsoft.EntityFrameworkCore;
using Hospital_Management_System.Data;
using Hospital_Management_System.DTOs;
using Hospital_Management_System.Models;
using Hospital_Management_System.Services.Interfaces;

namespace Hospital_Management_System.Services.Implementations
{
    public class MedicineService : IMedicineService
    {
        private readonly ApplicationDbContext _context;

        public MedicineService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<MedicineDto>> GetAllAsync()
        {
            return await _context.Medicines
                .OrderBy(m => m.Name)
                .Select(m => ToDto(m))
                .ToListAsync();
        }

        public async Task<MedicineDto?> GetByIdAsync(string id)
        {
            var medicine = await _context.Medicines.FindAsync(id);
            return medicine == null ? null : ToDto(medicine);
        }

        public async Task<MedicineDto> CreateAsync(MedicineCreateUpdateDto dto)
        {
            var nextNumber = await _context.Medicines.CountAsync() + 1;
            var id = $"MED-{nextNumber:D4}";

            var medicine = new Medicine
            {
                Id = id,
                Name = dto.Name,
                Category = dto.Category,
                Form = dto.Form,
                Price = dto.Price,
                StockQuantity = dto.Stock,
                ExpiryDate = dto.ExpiryDate,
                CreatedAt = DateTime.UtcNow
            };

            _context.Medicines.Add(medicine);
            await _context.SaveChangesAsync();

            return ToDto(medicine);
        }

        public async Task<MedicineDto?> UpdateAsync(string id, MedicineCreateUpdateDto dto)
        {
            var medicine = await _context.Medicines.FindAsync(id);
            if (medicine == null) return null;

            medicine.Name = dto.Name;
            medicine.Category = dto.Category;
            medicine.Form = dto.Form;
            medicine.Price = dto.Price;
            medicine.StockQuantity = dto.Stock;
            medicine.ExpiryDate = dto.ExpiryDate;

            await _context.SaveChangesAsync();
            return ToDto(medicine);
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var medicine = await _context.Medicines.FindAsync(id);
            if (medicine == null) return false;

            var isPrescribed = await _context.PrescriptionItems.AnyAsync(pi => pi.MedicineId == id);
            if (isPrescribed)
            {
                throw new InvalidOperationException("This medicine appears on existing prescriptions and cannot be deleted.");
            }

            _context.Medicines.Remove(medicine);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<MedicineOptionDto>> GetOptionsAsync()
        {
            return await _context.Medicines
                .OrderBy(m => m.Name)
                .Select(m => new MedicineOptionDto { Id = m.Id, Name = m.Name })
                .ToListAsync();
        }

        private static MedicineDto ToDto(Medicine m) => new()
        {
            Id = m.Id,
            Name = m.Name,
            Category = m.Category,
            Form = m.Form,
            Price = m.Price,
            StockQuantity = m.StockQuantity,
            StockStatus = m.StockStatus,
            ExpiryDate = m.ExpiryDate
        };
    }
}