using Microsoft.AspNetCore.Mvc;
using Hospital_Management_System.Data;
using Hospital_Management_System.Helpers;
using Microsoft.Extensions.Configuration;

namespace Hospital_Management_System.Controllers
{
    [ApiController]
    [Route("api/debug")]
    public class DebugController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly IConfiguration _config;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<DebugController> _logger;

        public DebugController(ApplicationDbContext db, IConfiguration config, IWebHostEnvironment env, ILogger<DebugController> logger)
        {
            _db = db;
            _config = config;
            _env = env;
            _logger = logger;
        }

        [HttpGet("check")]
        public IActionResult Check()
        {
            if (!_env.IsDevelopment())
                return NotFound();

            var cs = _config.GetConnectionString("DefaultConnection");
            string dbName = "";
            try
            {
                // try to parse Database= value from connection string
                var parts = cs.Split(';');
                foreach (var p in parts)
                {
                    var kv = p.Split('=', 2);
                    if (kv.Length == 2 && kv[0].Trim().Equals("Database", StringComparison.OrdinalIgnoreCase))
                    {
                        dbName = kv[1].Trim();
                        break;
                    }
                }
            }
            catch { }

            // Basic DB info
            var usersCount = 0;
            try { usersCount = _db.Users.Count(); } catch (Exception ex) { _logger.LogError(ex, "Error counting users"); }

            var email = "sino12@gmail.com";
            var normalized = email.Trim().ToLowerInvariant();
            var user = _db.Users.FirstOrDefault(u => u.Email.ToLower() == normalized);

            var userInfo = user == null ? null : new {
                user.UserID,
                user.Name,
                EmailStored = user.Email,
                RoleStored = user.Role,
                PasswordHash = user.PasswordHash,
                PasswordMatches = PasswordHelper.VerifyPassword("12345@#a", user.PasswordHash)
            };

            var result = new {
                Environment = _env.EnvironmentName,
                ConnectionString = cs,
                Database = dbName,
                UsersCount = usersCount,
                QueriedEmail = email,
                Found = user != null,
                User = userInfo
            };

            _logger.LogInformation("Debug check run: {@result}", result);

            return Ok(result);
        }
    }
}
