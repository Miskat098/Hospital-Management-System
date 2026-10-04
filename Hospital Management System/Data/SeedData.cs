using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Hospital_Management_System.Models;

namespace Hospital_Management_System.Data
{
    /// <summary>
    /// Seeds the four roles and a demo Admin/Doctor/Patient account for
    /// each, mirroring the MOCK_USERS fixture in wwwroot/js/api.js so the
    /// same demo credentials from the frontend preview keep working once
    /// MOCK_MODE is flipped to false.
    /// </summary>
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider services)
        {
            var context = services.GetRequiredService<ApplicationDbContext>();
            await context.Database.EnsureCreatedAsync();

            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

            // ----------------------------------------------------------
            // Roles
            // ----------------------------------------------------------
            foreach (var role in UserRoles.All)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // ----------------------------------------------------------
            // Demo Department (needed before seeding the demo Doctor)
            // ----------------------------------------------------------
            var cardiology = context.Departments.FirstOrDefault(d => d.Name == "Cardiology");
            if (cardiology == null)
            {
                cardiology = new Department
                {
                    Name = "Cardiology",
                    Description = "Heart & vascular care, diagnostics, and rehabilitation.",
                    Status = "Active"
                };
                context.Departments.Add(cardiology);
                await context.SaveChangesAsync();
            }

            // ----------------------------------------------------------
            // Demo Admin — admin@medicore.com / Demo@123
            // ----------------------------------------------------------
            await CreateUserIfNotExists(
                userManager,
                email: "admin@medicore.com",
                password: "Demo@123",
                firstName: "Ayesha",
                lastName: "Khatun",
                role: UserRoles.Admin,
                configureExtra: null);

            // ----------------------------------------------------------
            // Demo Doctor — demo@medicore.com / Demo@123
            // ----------------------------------------------------------
            var doctorUser = await CreateUserIfNotExists(
                userManager,
                email: "demo@medicore.com",
                password: "Demo@123",
                firstName: "Nasrin",
                lastName: "Karim",
                role: UserRoles.Doctor,
                configureExtra: null);

            if (doctorUser != null && !context.Doctors.Any(d => d.Id == doctorUser.Id))
            {
                context.Doctors.Add(new Doctor
                {
                    Id = doctorUser.Id,
                    DepartmentId = cardiology.Id,
                    Specialization = "Cardiology",
                    ExperienceYears = 14,
                    RegistrationNumber = "BMDC-44210",
                    ConsultationFee = 1500,
                    RoomNumber = "204",
                    Rating = 4.9m,
                    ReviewCount = 312
                });
                await context.SaveChangesAsync();
            }

            // ----------------------------------------------------------
            // Demo Patient — patient@medicore.com / Demo@123
            // ----------------------------------------------------------
            var patientUser = await CreateUserIfNotExists(
                userManager,
                email: "patient@medicore.com",
                password: "Demo@123",
                firstName: "Rafiq",
                lastName: "Ahmed",
                role: UserRoles.Patient,
                configureExtra: null);

            if (patientUser != null && !context.Patients.Any(p => p.Id == patientUser.Id))
            {
                context.Patients.Add(new Patient
                {
                    Id = patientUser.Id,
                    BloodGroup = "B+",
                    HeightCm = 172,
                    Allergies = "Penicillin",
                    ChronicConditions = "Hypertension",
                    CurrentMedications = "Amlodipine 5mg (once daily), Atorvastatin 20mg (once daily)"
                });
                await context.SaveChangesAsync();
            }

            // ----------------------------------------------------------
            // Demo Medicines (matches wwwroot/pages/admin/medicine-inventory.html)
            // ----------------------------------------------------------
            if (!context.Medicines.Any())
            {
                context.Medicines.AddRange(
                    new Medicine { Id = "MED-0001", Name = "Paracetamol 500mg", Category = "Analgesic", Form = "Tablet", Price = 2.50m, StockQuantity = 42, ExpiryDate = new DateTime(2027, 3, 1) },
                    new Medicine { Id = "MED-0002", Name = "Amoxicillin 250mg", Category = "Antibiotic", Form = "Capsule", Price = 6.00m, StockQuantity = 312, ExpiryDate = new DateTime(2027, 11, 1) },
                    new Medicine { Id = "MED-0003", Name = "Cetirizine 10mg", Category = "Antihistamine", Form = "Tablet", Price = 3.20m, StockQuantity = 118, ExpiryDate = new DateTime(2027, 1, 1) },
                    new Medicine { Id = "MED-0004", Name = "Atorvastatin 20mg", Category = "Cardiac", Form = "Tablet", Price = 9.75m, StockQuantity = 256, ExpiryDate = new DateTime(2027, 8, 1) },
                    new Medicine { Id = "MED-0005", Name = "Insulin Glargine", Category = "Endocrine", Form = "Injection", Price = 420.00m, StockQuantity = 9, ExpiryDate = new DateTime(2027, 6, 1) }
                );
                await context.SaveChangesAsync();
            }
        }

        private static async Task<ApplicationUser?> CreateUserIfNotExists(
            UserManager<ApplicationUser> userManager,
            string email,
            string password,
            string firstName,
            string lastName,
            string role,
            Action<ApplicationUser>? configureExtra)
        {
            var existing = await userManager.FindByEmailAsync(email);
            if (existing != null) return existing;

            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                FirstName = firstName,
                LastName = lastName,
                PrimaryRole = role,
                Status = "Active"
            };

            configureExtra?.Invoke(user);

            var result = await userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to seed user {email}: {errors}");
            }

            await userManager.AddToRoleAsync(user, role);
            return user;
        }
    }
}