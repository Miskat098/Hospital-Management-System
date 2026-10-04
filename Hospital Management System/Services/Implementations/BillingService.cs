using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Hospital_Management_System.Data;
using Hospital_Management_System.DTOs;
using Hospital_Management_System.Models;
using Hospital_Management_System.Services.Interfaces;

namespace Hospital_Management_System.Services.Implementations
{
    public class BillingService : IBillingService
    {
        private readonly ApplicationDbContext _context;

        public BillingService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<InvoiceDto>> GetInvoicesForPatientAsync(string patientId)
        {
            var invoices = await BaseQuery()
                .Where(i => i.PatientId == patientId)
                .OrderByDescending(i => i.IssuedAt)
                .ToListAsync();

            return invoices.Select(ToDto).ToList();
        }

        public async Task<List<InvoiceDto>> GetAllInvoicesAsync(string? status = null)
        {
            var query = BaseQuery();

            if (!string.IsNullOrWhiteSpace(status) &&
                Enum.TryParse<InvoiceStatus>(status, true, out var parsedStatus))
            {
                query = query.Where(i => i.Status == parsedStatus);
            }

            var invoices = await query.OrderByDescending(i => i.IssuedAt).ToListAsync();
            return invoices.Select(ToDto).ToList();
        }

        public async Task<InvoiceDto?> GetByIdAsync(string id)
        {
            var invoice = await BaseQuery().FirstOrDefaultAsync(i => i.Id == id);
            return invoice == null ? null : ToDto(invoice);
        }

        public async Task<InvoiceDto?> PayAsync(string id, PayInvoiceDto dto)
        {
            var invoice = await _context.Invoices.FindAsync(id);
            if (invoice == null) return null;

            if (invoice.Status == InvoiceStatus.Paid)
            {
                throw new InvalidOperationException("This invoice has already been paid.");
            }

            invoice.Status = InvoiceStatus.Paid;
            invoice.PaymentMethod = dto.Method;
            invoice.TransactionReference = $"TXN-{Guid.NewGuid().ToString()[..10].ToUpper()}";
            invoice.PaidAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var saved = await BaseQuery().FirstAsync(i => i.Id == id);
            return ToDto(saved);
        }

        public async Task<RevenueSummaryDto> GetRevenueSummaryAsync(string period)
        {
            var cutoff = period switch
            {
                "7d" => DateTime.UtcNow.AddDays(-7),
                "90d" => DateTime.UtcNow.AddDays(-90),
                "12m" => DateTime.UtcNow.AddMonths(-12),
                _ => DateTime.UtcNow.AddDays(-30) // "30d" default
            };

            var invoicesQuery = _context.Invoices.Where(i => i.IssuedAt >= cutoff);

            // ডাটাবেজ লেভেলে ফিল্টার এবং অ্যাগ্রিগেশন (পারফরম্যান্স উন্নত করা হয়েছে)
            var totalRevenue = await invoicesQuery
                .Where(i => i.Status == InvoiceStatus.Paid)
                .SumAsync(i => (decimal?)i.TotalAmount) ?? 0m;

            var invoicesIssued = await invoicesQuery.CountAsync();
            var pendingCount = await invoicesQuery.CountAsync(i => i.Status == InvoiceStatus.Pending);
            var overdueCount = await invoicesQuery.CountAsync(i => i.Status == InvoiceStatus.Overdue);

            // সাপ্তাহিক ট্রেন্ড হিসাবের জন্য ডাটা আনা
            var paidInvoices = await invoicesQuery
                .Where(i => i.Status == InvoiceStatus.Paid && i.PaidAt.HasValue)
                .Select(i => new { i.PaidAt, i.TotalAmount })
                .ToListAsync();

            var trend = paidInvoices
                .GroupBy(i => new
                {
                    Year = ISOWeek.GetYear(i.PaidAt!.Value),
                    Week = ISOWeek.GetWeekOfYear(i.PaidAt!.Value)
                })
                .OrderBy(g => g.Key.Year)
                .ThenBy(g => g.Key.Week)
                .Select(g => new RevenueTrendPointDto
                {
                    Label = $"Wk {g.Key.Week}",
                    Amount = g.Sum(i => i.TotalAmount)
                })
                .TakeLast(8)
                .ToList();

            return new RevenueSummaryDto
            {
                TotalRevenue = totalRevenue,
                InvoicesIssued = invoicesIssued,
                PendingPayments = pendingCount,
                OverdueInvoices = overdueCount,
                Trend = trend
            };
        }

        // --------------------------------------------------------------
        // Helpers
        // --------------------------------------------------------------
        private IQueryable<Invoice> BaseQuery() =>
            _context.Invoices
                .AsNoTracking()
                .Include(i => i.Patient).ThenInclude(p => p.User)
                .Include(i => i.Appointment).ThenInclude(a => a.Doctor).ThenInclude(d => d.User)
                .Include(i => i.Appointment).ThenInclude(a => a.Doctor).ThenInclude(d => d.Department);

        private static InvoiceDto ToDto(Invoice i) => new()
        {
            Id = i.Id,
            AppointmentId = i.AppointmentId,
            PatientId = i.PatientId,
            PatientName = i.Patient?.User?.FullName ?? "Unknown Patient",
            Department = i.Appointment?.Doctor?.Department?.Name ?? "N/A",
            DoctorName = i.Appointment?.Doctor?.User?.FullName != null
                ? $"Dr. {i.Appointment.Doctor.User.FullName}"
                : "N/A",
            ConsultationFee = i.ConsultationFee,
            TestFees = i.TestFees,
            ServiceCharge = i.ServiceCharge,
            TotalAmount = i.TotalAmount,
            Status = i.Status.ToString(),
            PaymentMethod = i.PaymentMethod,
            IssuedAt = i.IssuedAt,
            DueDate = i.DueDate,
            PaidAt = i.PaidAt
        };
    }
}