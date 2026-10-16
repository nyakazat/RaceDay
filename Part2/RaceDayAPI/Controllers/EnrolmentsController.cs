
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
    [Authorize]
    public class EnrolmentsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public EnrolmentsController(RaceDayDbContext context)
        {
            _context = context;
        }

        // =====================================
        // CHECK AUTHENTICATED SESSION
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

        private int GetLoggedInUserID()
        {
            return int.Parse(
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier)!);
        }

        // =====================================
        // GET ENROLMENTS FOR OWN EVENTS
        // Organiser only
        // =====================================
        [HttpGet]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> GetEnrolments()
        {
            if (!HasValidSession())
                return Unauthorized("Invalid session.");

            int organiserID = GetLoggedInUserID();

            var enrolments = await _context.Enrolments
                .AsNoTracking()
                .Where(e =>
                    e.EventCategory.Event.OrganiserID
                    == organiserID)
                .Select(e => new
                {
                    e.EnrolmentID,
                    e.UserID,
                    e.EventCategoryID,
                    EventID =
                        e.EventCategory.EventID,
                    e.EnrolmentDate,
                    e.Status
                })
                .ToListAsync();

            return Ok(enrolments);
        }

        // =====================================
        // GET MY ENROLMENTS
        // Participant only
        // =====================================
        [HttpGet("my")]
        [Authorize(Roles = "Participant")]
        public async Task<IActionResult> GetMyEnrolments()
        {
            if (!HasValidSession())
                return Unauthorized("Invalid session.");

            int userID = GetLoggedInUserID();

            var enrolments = await _context.Enrolments
                .AsNoTracking()
                .Where(e => e.UserID == userID)
                .Select(e => new
                {
                    e.EnrolmentID,
                    e.UserID,
                    e.EventCategoryID,
                    EventID =
                        e.EventCategory.EventID,
                    e.EnrolmentDate,
                    e.Status
                })
                .ToListAsync();

            return Ok(enrolments);
        }

        // =====================================
        // GET ENROLMENT BY ID
        // Participant owner or Event Organiser
        // =====================================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetEnrolment(int id)
        {
            if (!HasValidSession())
                return Unauthorized("Invalid session.");

            int userID = GetLoggedInUserID();

            var enrolment = await _context.Enrolments
                .AsNoTracking()
                .Where(e => e.EnrolmentID == id)
                .Select(e => new
                {
                    e.EnrolmentID,
                    e.UserID,
                    e.EventCategoryID,
                    EventID =
                        e.EventCategory.EventID,
                    OrganiserID =
                        e.EventCategory.Event.OrganiserID,
                    e.EnrolmentDate,
                    e.Status
                })
                .FirstOrDefaultAsync();

            if (enrolment == null)
                return NotFound("Enrolment not found.");

            bool isParticipantOwner =
                User.IsInRole("Participant")
                && enrolment.UserID == userID;

            bool isEventOrganiser =
                User.IsInRole("Organiser")
                && enrolment.OrganiserID == userID;

            if (!isParticipantOwner && !isEventOrganiser)
                return Forbid();

            return Ok(new
            {
                enrolment.EnrolmentID,
                enrolment.UserID,
                enrolment.EventCategoryID,
                enrolment.EventID,
                enrolment.EnrolmentDate,
                enrolment.Status
            });
        }

        // =====================================
        // CREATE ENROLMENT
        // Participant only
        // =====================================
        [HttpPost]
        [Authorize(Roles = "Participant")]
        public async Task<IActionResult> CreateEnrolment(
            CreateEnrolmentRequest request)
        {
            if (!HasValidSession())
                return Unauthorized("Invalid session.");

            int userID = GetLoggedInUserID();

            bool categoryExists =
                await _context.EventCategories
                .AnyAsync(ec =>
                    ec.EventCategoryID ==
                    request.EventCategoryID);

            if (!categoryExists)
                return BadRequest(
                    "Event category does not exist.");

            bool alreadyEnrolled =
                await _context.Enrolments
                .AnyAsync(e =>
                    e.UserID == userID &&
                    e.EventCategoryID ==
                    request.EventCategoryID);

            if (alreadyEnrolled)
                return Conflict(
                    "Participant already enrolled.");

            var enrolment = new Enrolment
            {
                UserID = userID,
                EventCategoryID =
                    request.EventCategoryID,
                EnrolmentDate = DateTime.UtcNow,
                Status = "Active"
            };

            _context.Enrolments.Add(enrolment);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetEnrolment),
                new { id = enrolment.EnrolmentID },
                new
                {
                    enrolment.EnrolmentID,
                    enrolment.UserID,
                    enrolment.EventCategoryID,
                    enrolment.EnrolmentDate,
                    enrolment.Status
                });
        }

        // =====================================
        // UPDATE OWN ENROLMENT STATUS
        // Participant only
        // =====================================
        [HttpPut("{id}")]
        [Authorize(Roles = "Participant")]
        public async Task<IActionResult> UpdateEnrolment(
            int id,
            UpdateEnrolmentRequest request)
        {
            if (!HasValidSession())
                return Unauthorized("Invalid session.");

            int userID = GetLoggedInUserID();

            var enrolment = await _context.Enrolments
                .FindAsync(id);

            if (enrolment == null)
                return NotFound("Enrolment not found.");

            if (enrolment.UserID != userID)
                return Forbid();

            if (request.Status != "Active" &&
                request.Status != "Cancelled")
            {
                return BadRequest(
                    "Status must be Active or Cancelled.");
            }

            enrolment.Status = request.Status;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // =====================================
        // DELETE OWN ENROLMENT
        // Participant only
        // =====================================
        [HttpDelete("{id}")]
        [Authorize(Roles = "Participant")]
        public async Task<IActionResult> DeleteEnrolment(int id)
        {
            if (!HasValidSession())
                return Unauthorized("Invalid session.");

            int userID = GetLoggedInUserID();

            var enrolment = await _context.Enrolments
                .FindAsync(id);

            if (enrolment == null)
                return NotFound("Enrolment not found.");

            if (enrolment.UserID != userID)
                return Forbid();

            // Prevent deleting enrolments
            // that already have race results
            bool hasResult = await _context.Results
                .AnyAsync(r => r.EnrolmentID == id);

            if (hasResult)
            {
                return Conflict(
                    "Cannot delete an enrolment " +
                    "that already has a race result.");
            }

            _context.Enrolments.Remove(enrolment);

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }

    // =====================================
    // CREATE ENROLMENT REQUEST
    // =====================================
    public class CreateEnrolmentRequest
    {
        [Range(1, int.MaxValue)]
        public int EventCategoryID { get; set; }
    }

    // =====================================
    // UPDATE ENROLMENT REQUEST
    // =====================================
    public class UpdateEnrolmentRequest
    {
        [Required]
        public string Status { get; set; }
            = "Active";
    }
}
