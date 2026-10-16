
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Data;
using RaceDayAPI.Models;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace RaceDayAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        private readonly PasswordHasher<AppUser> _passwordHasher
            = new PasswordHasher<AppUser>();

        public AuthController(RaceDayDbContext context)
        {
            _context = context;
        }

        // =====================================
        // REGISTER USER
        // POST: api/Auth/register
        // =====================================
        [HttpPost("register")]
        public async Task<IActionResult> Register(
            RegisterRequest request)
        {
            if (request.RoleID != 1 && request.RoleID != 2)
            {
                return BadRequest(
                    "RoleID must be 1 (Organiser) or 2 (Participant).");
            }

            string email = request.Email.Trim().ToLowerInvariant();

            bool emailExists = await _context.AppUsers
                .AnyAsync(u => u.Email == email);

            if (emailExists)
            {
                return Conflict("Email is already registered.");
            }

            bool roleExists = await _context.Roles
                .AnyAsync(r => r.RoleID == request.RoleID);

            if (!roleExists)
            {
                return BadRequest("Selected role does not exist.");
            }

            var user = new AppUser
            {
                FirstName = request.FirstName.Trim(),
                LastName = request.LastName.Trim(),
                Email = email,
                RoleID = request.RoleID,
                PhoneNumber = request.PhoneNumber,
                DateRegistered = DateTime.UtcNow
            };

            user.PasswordHash =
                _passwordHasher.HashPassword(
                    user, request.Password);

            _context.AppUsers.Add(user);
            await _context.SaveChangesAsync();

            return StatusCode(201, new
            {
                Message = "Registration successful.",
                user.UserID,
                user.FirstName,
                user.LastName,
                user.Email,
                Role = request.RoleID == 1
                    ? "Organiser" : "Participant"
            });
        }

        // =====================================
        // LOGIN USER
        // POST: api/Auth/login
        // =====================================
        [HttpPost("login")]
        public async Task<IActionResult> Login(
            LoginRequest request)
        {
            string email = request.Email.Trim().ToLowerInvariant();

            var user = await _context.AppUsers
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                return Unauthorized(
                    "Invalid email or password.");
            }

            var passwordResult =
                _passwordHasher.VerifyHashedPassword(
                    user,
                    user.PasswordHash,
                    request.Password);

            if (passwordResult ==
                PasswordVerificationResult.Failed)
            {
                return Unauthorized(
                    "Invalid email or password.");
            }

            var claims = new List<Claim>
            {
                new Claim(
                    ClaimTypes.NameIdentifier,
                    user.UserID.ToString()),

                new Claim(
                    ClaimTypes.Name,
                    user.FirstName),

                new Claim(
                    ClaimTypes.Role,
                    user.Role.RoleName)
            };

            var identity = new ClaimsIdentity(
                claims,
                "RaceDayCookie");

            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                "RaceDayCookie",
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = false,
                    ExpiresUtc = DateTimeOffset.UtcNow
                        .AddMinutes(30)
                });

            HttpContext.Session.SetInt32(
                "UserID", user.UserID);

            HttpContext.Session.SetString(
                "Role", user.Role.RoleName);

            return Ok(new
            {
                Message = "Login successful.",
                user.UserID,
                user.FirstName,
                user.Email,
                Role = user.Role.RoleName
            });
        }

        // =====================================
        // GET CURRENT LOGGED-IN USER
        // GET: api/Auth/me
        // =====================================
        [Authorize]
        [HttpGet("me")]
        public IActionResult GetCurrentUser()
        {
            int? userID =
                HttpContext.Session.GetInt32("UserID");

            string? role =
                HttpContext.Session.GetString("Role");

            string? claimUserID =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            string? claimRole =
                User.FindFirstValue(ClaimTypes.Role);

            if (userID == null ||
                userID.ToString() != claimUserID ||
                role == null ||
                role != claimRole)
            {
                return Unauthorized(
                    "Session expired or invalid.");
            }

            return Ok(new
            {
                Message = "User is authenticated.",
                UserID = userID,
                Role = role
            });
        }

        // =====================================
        // LOGOUT USER
        // POST: api/Auth/logout
        // =====================================
        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Clear();

            await HttpContext.SignOutAsync(
                "RaceDayCookie");

            return Ok(new
            {
                Message = "Logout successful."
            });
        }
    }

    // =====================================
    // REGISTRATION REQUEST MODEL
    // =====================================
    public class RegisterRequest
    {
        [Required]
        [StringLength(100)]
        public string FirstName { get; set; }
            = string.Empty;

        [Required]
        [StringLength(100)]
        public string LastName { get; set; }
            = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; }
            = string.Empty;

        [Required]
        [MinLength(8)]
        public string Password { get; set; }
            = string.Empty;

        [Required]
        [Range(1, 2)]
        public int RoleID { get; set; }

        public string? PhoneNumber { get; set; }
    }

    // =====================================
    // LOGIN REQUEST MODEL
    // =====================================
    public class LoginRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
            = string.Empty;

        [Required]
        public string Password { get; set; }
            = string.Empty;
    }
}
