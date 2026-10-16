
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
    public class ResultsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public ResultsController(RaceDayDbContext context)
        {
            _context = context;
        }

        // =====================================
        // VERIFY COOKIE AND SERVER SESSION
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
        // GET RESULTS FOR OWN EVENTS
        // Organiser only
        // =====================================
        [HttpGet]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> GetResults()
        {
            if (!HasValidSession())
                return Unauthorized("Invalid session.");

            int organiserID = GetLoggedInUserID();

            var results = await _context.Results
                .AsNoTracking()
                .Where(r =>
                    r.Enrolment.EventCategory.Event
                        .OrganiserID == organiserID)
                .Select(r => new
                {
                    r.ResultID,
                    r.EnrolmentID,
                    r.Enrolment.UserID,
                    EventID = r.Enrolment
                        .EventCategory.EventID,
                    r.FinishTime,
                    r.FinishPosition,
                    r.Points,
                    r.DateRecorded
                })
                .ToListAsync();

            return Ok(results);
        }

        // =====================================
        // GET OWN RACE RESULTS
        // Participant only
        // =====================================
        [HttpGet("my")]
        [Authorize(Roles = "Participant")]
        public async Task<IActionResult> GetMyResults()
        {
            if (!HasValidSession())
                return Unauthorized("Invalid session.");

            int userID = GetLoggedInUserID();

            var results = await _context.Results
                .AsNoTracking()
                .Where(r => r.Enrolment.UserID == userID)
                .Select(r => new
                {
                    r.ResultID,
                    r.EnrolmentID,
                    EventID = r.Enrolment
                        .EventCategory.EventID,
                    r.FinishTime,
                    r.FinishPosition,
                    r.Points,
                    r.DateRecorded
                })
                .ToListAsync();

            return Ok(results);
        }

        // =====================================
        // GET RESULT BY ID
        // Participant owner or event Organiser
        // =====================================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetResult(int id)
        {
            if (!HasValidSession())
                return Unauthorized("Invalid session.");

            int userID = GetLoggedInUserID();

            var result = await _context.Results
                .AsNoTracking()
                .Where(r => r.ResultID == id)
                .Select(r => new
                {
                    r.ResultID,
                    r.EnrolmentID,
                    ParticipantID = r.Enrolment.UserID,
                    OrganiserID = r.Enrolment
                        .EventCategory.Event.OrganiserID,
                    r.FinishTime,
                    r.FinishPosition,
                    r.Points,
                    r.DateRecorded
                })
                .FirstOrDefaultAsync();

            if (result == null)
                return NotFound("Result not found.");

            bool canView =
                (User.IsInRole("Participant")
                    && result.ParticipantID == userID)
                ||
                (User.IsInRole("Organiser")
                    && result.OrganiserID == userID);

            if (!canView)
                return Forbid();

            return Ok(new
            {
                result.ResultID,
                result.EnrolmentID,
                result.FinishTime,
                result.FinishPosition,
                result.Points,
                result.DateRecorded
            });
        }

        // =====================================
        // CREATE RACE RESULT
        // Owning Organiser only
        // =====================================
        [HttpPost]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> CreateResult(
            CreateResultRequest request)
        {
            if (!HasValidSession())
                return Unauthorized("Invalid session.");

            if (request.FinishTime <= TimeSpan.Zero)
                return BadRequest(
                    "Finish time must be positive.");

            int organiserID = GetLoggedInUserID();

            var enrolment = await _context.Enrolments
                .AsNoTracking()
                .Where(e =>
                    e.EnrolmentID == request.EnrolmentID)
                .Select(e => new
                {
                    e.EnrolmentID,
                    e.Status,
                    OrganiserID = e.EventCategory
                        .Event.OrganiserID
                })
                .FirstOrDefaultAsync();

            if (enrolment == null)
                return BadRequest(
                    "Enrolment does not exist.");

            if (enrolment.OrganiserID != organiserID)
                return Forbid();

            if (enrolment.Status != "Active")
                return BadRequest(
                    "Enrolment must be Active.");

            bool alreadyRecorded =
                await _context.Results
                .AnyAsync(r =>
                    r.EnrolmentID == request.EnrolmentID);

            if (alreadyRecorded)
                return Conflict(
                    "Result already recorded.");

            var result = new Result
            {
                EnrolmentID = request.EnrolmentID,
                FinishTime = request.FinishTime,
                FinishPosition = request.FinishPosition,
                Points = request.Points,
                DateRecorded = DateTime.UtcNow
            };

            _context.Results.Add(result);

            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetResult),
                new { id = result.ResultID },
                new
                {
                    result.ResultID,
                    result.EnrolmentID,
                    result.FinishTime,
                    result.FinishPosition,
                    result.Points,
                    result.DateRecorded
                });
        }

        // =====================================
        // UPDATE RACE RESULT
        // Owning Organiser only
        // =====================================
        [HttpPut("{id}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> UpdateResult(
            int id,
            UpdateResultRequest request)
        {
            if (!HasValidSession())
                return Unauthorized("Invalid session.");

            if (request.FinishTime <= TimeSpan.Zero)
                return BadRequest(
                    "Finish time must be positive.");

            int organiserID = GetLoggedInUserID();

            var result = await _context.Results
                .Include(r => r.Enrolment)
                    .ThenInclude(e => e.EventCategory)
                        .ThenInclude(ec => ec.Event)
                .FirstOrDefaultAsync(r => r.ResultID == id);

            if (result == null)
                return NotFound("Result not found.");

            if (result.Enrolment.EventCategory.Event
                .OrganiserID != organiserID)
                return Forbid();

            result.FinishTime = request.FinishTime;
            result.FinishPosition = request.FinishPosition;
            result.Points = request.Points;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // =====================================
        // DELETE RACE RESULT
        // Owning Organiser only
        // =====================================
        [HttpDelete("{id}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> DeleteResult(int id)
        {
            if (!HasValidSession())
                return Unauthorized("Invalid session.");

            int organiserID = GetLoggedInUserID();

            var result = await _context.Results
                .Include(r => r.Enrolment)
                    .ThenInclude(e => e.EventCategory)
                        .ThenInclude(ec => ec.Event)
                .FirstOrDefaultAsync(r => r.ResultID == id);

            if (result == null)
                return NotFound("Result not found.");

            if (result.Enrolment.EventCategory.Event
                .OrganiserID != organiserID)
                return Forbid();

            _context.Results.Remove(result);

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }

    // =====================================
    // CREATE RESULT REQUEST
    // =====================================
    public class CreateResultRequest
    {
        [Range(1, int.MaxValue)]
        public int EnrolmentID { get; set; }

        [Required]
        public TimeSpan FinishTime { get; set; }

        [Range(1, int.MaxValue)]
        public int FinishPosition { get; set; }

        [Range(0, int.MaxValue)]
        public int Points { get; set; }
    }

    // =====================================
    // UPDATE RESULT REQUEST
    // =====================================
    public class UpdateResultRequest
    {
        [Required]
        public TimeSpan FinishTime { get; set; }

        [Range(1, int.MaxValue)]
        public int FinishPosition { get; set; }

        [Range(0, int.MaxValue)]
        public int Points { get; set; }
    }
}
