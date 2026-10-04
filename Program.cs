using Hospital_Management_System.Data;
using Hospital_Management_System.Models;
using Hospital_Management_System.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// ---- Database (Entity Framework Core) ----
builder.Services.AddDbContext<ApplicationDbContext>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ---- Session (used for login state) ----
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
// Ensure session is available before authorization checks
app.UseSession();

// Middleware: protect static pages and API endpoints based on session role
app.Use(async (context, next) =>
{
    var path = context.Request.Path.Value ?? string.Empty;

    // Allow public MVC account pages and API login/register endpoints
    var publicPaths = new[] {
        "/pages/login.html",
        "/pages/register.html",
        "/Account/Login",
        "/Account/Register",
        "/api/account/login",
        "/api/debug/check",
        "/Account/Logout"
    };

    // If request is for API endpoints, require authentication (session) for all except api/account/login
    if (path.StartsWith("/api/", StringComparison.OrdinalIgnoreCase))
    {
        // Allow the login endpoint (and debug check in development) to be called without a session
        if (string.Equals(path, "/api/account/login", StringComparison.OrdinalIgnoreCase) || string.Equals(path, "/api/debug/check", StringComparison.OrdinalIgnoreCase))
        {
            await next();
            return;
        }

        var uid = context.Session.GetInt32("UserID");
        if (!uid.HasValue)
        {
            context.Response.StatusCode = 401; // Unauthorized
            await context.Response.WriteAsJsonAsync(new { success = false, message = "Not authenticated" });
            return;
        }

        await next();
        return;
    }

    // Protect static pages under /pages
    if (path.StartsWith("/pages/", StringComparison.OrdinalIgnoreCase))
    {
        // Allow public static pages
        if (publicPaths.Any(p => path.Equals(p, StringComparison.OrdinalIgnoreCase)))
        {
            await next();
            return;
        }

        var uid = context.Session.GetInt32("UserID");
        var role = context.Session.GetString("UserRole") ?? string.Empty;
        if (!uid.HasValue)
        {
            // Redirect unauthenticated users to static login
            context.Response.Redirect("/pages/login.html");
            return;
        }

        // More explicit page-level role mapping including role-specific dashboards.
        string page = Path.GetFileName(path).ToLowerInvariant();
        bool allowed = page switch
        {
            // public / unprotected pages handled earlier
            // role-specific dashboards
            "admin-dashboard.html" => role.Equals("admin", StringComparison.OrdinalIgnoreCase),
            "doctor-dashboard.html" => role.Equals("doctor", StringComparison.OrdinalIgnoreCase),
            "patient-dashboard.html" => role.Equals("patient", StringComparison.OrdinalIgnoreCase),

            // pages accessible to Admin, Doctor and Patient (list view)
            "doctors.html" => role.Equals("admin", StringComparison.OrdinalIgnoreCase) || role.Equals("doctor", StringComparison.OrdinalIgnoreCase) || role.Equals("patient", StringComparison.OrdinalIgnoreCase),
            "departments.html" => role.Equals("admin", StringComparison.OrdinalIgnoreCase),
            "medicines.html" => role.Equals("admin", StringComparison.OrdinalIgnoreCase),

            // billing accessible to Admin and Patient
            "billing.html" => role.Equals("admin", StringComparison.OrdinalIgnoreCase) || role.Equals("patient", StringComparison.OrdinalIgnoreCase),

            // pages accessible to Admin and Doctor (and Patient where noted)
            "patients.html" => role.Equals("admin", StringComparison.OrdinalIgnoreCase) || role.Equals("doctor", StringComparison.OrdinalIgnoreCase) || role.Equals("patient", StringComparison.OrdinalIgnoreCase),
            "feedback.html" => role.Equals("admin", StringComparison.OrdinalIgnoreCase) || role.Equals("doctor", StringComparison.OrdinalIgnoreCase) || role.Equals("patient", StringComparison.OrdinalIgnoreCase),
            "appointments.html" => role.Equals("admin", StringComparison.OrdinalIgnoreCase) || role.Equals("doctor", StringComparison.OrdinalIgnoreCase) || role.Equals("patient", StringComparison.OrdinalIgnoreCase),
            "prescriptions.html" => role.Equals("admin", StringComparison.OrdinalIgnoreCase) || role.Equals("doctor", StringComparison.OrdinalIgnoreCase) || role.Equals("patient", StringComparison.OrdinalIgnoreCase),

            // Allow login/register and other non-protected pages by default
            _ => true
        };

        if (!allowed)
        {
            // If user is authenticated but not allowed, send them to access-denied or to their own dashboard
            if (role.Equals("admin", StringComparison.OrdinalIgnoreCase))
            {
                context.Response.Redirect("/pages/admin-dashboard.html");
                return;
            }
            else if (role.Equals("doctor", StringComparison.OrdinalIgnoreCase))
            {
                context.Response.Redirect("/pages/doctor-dashboard.html");
                return;
            }
            else if (role.Equals("patient", StringComparison.OrdinalIgnoreCase))
            {
                context.Response.Redirect("/pages/patient-dashboard.html");
                return;
            }

            // Fallback to access denied
            context.Response.Redirect("/pages/access-denied.html");
            return;
        }

        await next();
        return;
    }

    await next();
});

// Static files from wwwroot
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
name: "default",
pattern: "{controller=Account}/{action=Login}/{id?}");

// --- Ensure a development Admin exists (create or update single admin by email) ---
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<Hospital_Management_System.Data.ApplicationDbContext>();
        // Ensure database is created (helps development environment without explicit migrations)
        try
        {
            db.Database.EnsureCreated();
        }
        catch { }
        var adminEmail = "sino12@gmail.com";
        var adminEmailLower = adminEmail.Trim().ToLowerInvariant();

        // Look up user by normalized email (avoid duplicates and case issues)
        var existing = db.Users.FirstOrDefault(u => u.Email.ToLower() == adminEmailLower);
        if (existing == null)
        {
            var admin = new Hospital_Management_System.Models.User
            {
                Name = "System Administrator",
                Email = adminEmailLower,
                // Phone column in DB is non-nullable in current migration - use empty string to avoid insert errors
                Phone = string.Empty,
                Role = "Admin",
                PasswordHash = Hospital_Management_System.Helpers.PasswordHelper.HashPassword("12345@#a")
            };
            db.Users.Add(admin);
            db.SaveChanges();
        }
        else
        {
            // Ensure role and password are correct; update if necessary
            var updated = false;
            if (!string.Equals(existing.Role, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                existing.Role = "Admin";
                updated = true;
            }

            // Ensure stored email is normalized
            if (existing.Email != adminEmailLower)
            {
                existing.Email = adminEmailLower;
                updated = true;
            }

            // Ensure password matches expected (if not, update it)
            if (!Hospital_Management_System.Helpers.PasswordHelper.VerifyPassword("12345@#a", existing.PasswordHash))
            {
                existing.PasswordHash = Hospital_Management_System.Helpers.PasswordHelper.HashPassword("12345@#a");
                updated = true;
            }

            if (updated)
            {
                db.Users.Update(existing);
                db.SaveChanges();
            }
        }
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetService<Microsoft.Extensions.Logging.ILogger<Program>>();
        logger?.LogError(ex, "Error while ensuring admin user");
    }
}

// Attempt to start the web application and provide clearer logging on bind failures
try
{
    var logger = app.Services.GetService<Microsoft.Extensions.Logging.ILogger<Program>>();
    logger?.LogInformation("Starting application. Configured URLs: {Urls}", string.Join(';', app.Urls));
    app.Run();
}
catch (System.IO.IOException ex)
{
    var logger = app.Services.GetService<Microsoft.Extensions.Logging.ILogger<Program>>();
    logger?.LogError(ex, "Failed to bind to configured address. The port may already be in use. Configured URLs: {Urls}", string.Join(';', app.Urls));
    throw;
}
