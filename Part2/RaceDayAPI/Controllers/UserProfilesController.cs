
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Data;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace RaceDayAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserProfilesController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public UserProfilesController(
            RaceDayDbContext context)
        {
            _context = context;
        }

        // =====================================
        // VERIFY AUTHENTICATED SESSION
        // =====================================
        private bool HasValidSession()
        {
            int? sessionUserID =
                HttpContext.Session.GetInt32("UserID");

            string? sessionRole =
                HttpContext.Session.GetString("Role");

            string? claimUserID =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            string? claimRole =
                User.FindFirstValue(ClaimTypes.Role);

            return sessionUserID.HasValue
                && sessionUserID.Value.ToString() == claimUserID
                && !string.IsNullOrEmpty(sessionRole)
                && sessionRole == claimRole;
        }

        private int GetUserID()
        {
            return int.Parse(
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier)!);
        }

        // =====================================
        // GET MY PROFILE
        // =====================================
        [HttpGet("me")]
        public async Task<IActionResult> GetMyProfile()
        {
            if (!HasValidSession())
                return Unauthorized("Invalid session.");

            int userID = GetUserID();

            var profile = await _context.AppUsers
                .AsNoTracking()
                .Where(u => u.UserID == userID)
                .Select(u => new
                {
                    u.UserID,
                    u.FirstName,
                    u.LastName,
                    u.Email,
                    u.PhoneNumber,
                    u.DateRegistered,
                    u.RoleID,
                    Role = u.Role.RoleName
                })
                .FirstOrDefaultAsync();

            if (profile == null)
                return NotFound("User not found.");

            return Ok(profile);
        }

        // =====================================
        // UPDATE MY PROFILE
        // =====================================
        [HttpPut("me")]
        public async Task<IActionResult> UpdateMyProfile(
            UpdateProfileRequest request)
        {
            if (!HasValidSession())
                return Unauthorized("Invalid session.");

            int userID = GetUserID();

            var user = await _context.AppUsers
                .FindAsync(userID);

            if (user == null)
                return NotFound("User not found.");

            string email =
                request.Email.Trim().ToLowerInvariant();

            bool emailUsed = await _context.AppUsers
                .AnyAsync(u =>
                    u.Email == email &&
                    u.UserID != userID);

            if (emailUsed)
                return Conflict("Email already in use.");

            user.FirstName = request.FirstName.Trim();
            user.LastName = request.LastName.Trim();
            user.Email = email;
            user.PhoneNumber = request.PhoneNumber;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Profile updated successfully.",
                user.UserID,
                user.FirstName,
                user.LastName,
                user.Email,
                user.PhoneNumber
            });
        }
    }

    // =====================================
    // UPDATE PROFILE REQUEST DTO
    // =====================================
    public class UpdateProfileRequest
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
        [StringLength(150)]
        public string Email { get; set; }
            = string.Empty;

        [StringLength(20)]
        public string? PhoneNumber { get; set; }
    }
}
