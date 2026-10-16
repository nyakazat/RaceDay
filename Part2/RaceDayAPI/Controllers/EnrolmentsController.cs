
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
    /// <summary>
    /// Manages participant enrolments for sporting events.
    /// </summary>
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

        private int GetLoggedInUserID()
        {
            return int.Parse(
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier)!);
        }

        // =====================================
        // GET ENROLMENTS FOR OWN EVENTS
        // =====================================

        /// <summary>
        /// Gets enrolments for events owned by the Organiser.
        /// </summary>
        /// <response code="200">Enrolments retrieved.</response>
        /// <response code="401">Not authenticated.</response>
        /// <response code="403">Organiser role required.</response>
        [HttpGet]
        [Authorize(Roles = "Organiser")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
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
                    EventID = e.EventCategory.EventID,
                    e.EnrolmentDate,
                    e.Status
                })
                .ToListAsync();

            return Ok(enrolments);
        }

        // =====================================
        // GET PARTICIPANT'S OWN ENROLMENTS
        // =====================================

        /// <summary>
        /// Gets the logged-in Participant's enrolments.
        /// </summary>
        [HttpGet("my")]
        [Authorize(Roles = "Participant")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
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
                    EventID = e.EventCategory.EventID,
                    e.EnrolmentDate,
                    e.Status
                })
                .ToListAsync();

            return Ok(enrolments);
        }

        // =====================================
        // GET ENROLMENT BY ID
        // =====================================

        /// <summary>
        /// Gets an enrolment by ID.
        /// </summary>
        /// <remarks>
        /// Only the enrolled Participant or the
        /// Organiser who owns the event can view it.
        /// </remarks>
        /// <param name="id">Enrolment ID.</param>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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
                    EventID = e.EventCategory.EventID,
                    OrganiserID =
                        e.EventCategory.Event.OrganiserID,
                    e.EnrolmentDate,
                    e.Status
                })
                .FirstOrDefaultAsync();

            if (enrolment == null)
                return NotFound("Enrolment not found.");

            bool participantOwner =
                User.IsInRole("Participant")
                && enrolment.UserID == userID;

            bool eventOrganiser =
                User.IsInRole("Organiser")
                && enrolment.OrganiserID == userID;

            if (!participantOwner && !eventOrganiser)
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
        // =====================================

        /// <summary>
        /// Enrols the logged-in Participant in an event category.
        /// </summary>
        /// <remarks>
        /// Participant access only.
        /// The UserID is obtained from the login session.
        /// Duplicate enrolments are rejected.
        /// </remarks>
        /// <param name="request">
        /// The EventCategoryID to enrol in.
        /// </param>
        /// <response code="201">Enrolment created.</response>
        /// <response code="400">Invalid category.</response>
        /// <response code="401">Not authenticated.</response>
        /// <response code="403">Participant role required.</response>
        /// <response code="409">Already enrolled.</response>
        [HttpPost]
        [Authorize(Roles = "Participant")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
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
                EventCategoryID = request.EventCategoryID,
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
        // UPDATE OWN ENROLMENT
        // =====================================

        /// <summary>
        /// Updates the logged-in Participant's enrolment status.
        /// </summary>
        /// <param name="id">Enrolment ID.</param>
        /// <param name="request">
        /// Status must be Active or Cancelled.
        /// </param>
        [HttpPut("{id}")]
        [Authorize(Roles = "Participant")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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
        // =====================================

        /// <summary>
        /// Deletes a Participant's own enrolment.
        /// </summary>
        /// <remarks>
        /// Enrolments containing race results
        /// cannot be deleted.
        /// </remarks>
        /// <param name="id">Enrolment ID.</param>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Participant")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
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
    // CREATE ENROLMENT REQUEST DTO
    // =====================================
    public class CreateEnrolmentRequest
    {
        [Range(1, int.MaxValue)]
        public int EventCategoryID { get; set; }
    }

    // =====================================
    // UPDATE ENROLMENT REQUEST DTO
    // =====================================
    public class UpdateEnrolmentRequest
    {
        [Required]
        public string Status { get; set; } = "Active";
    }
}
