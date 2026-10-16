
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
    public class EventsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public EventsController(RaceDayDbContext context)
        {
            _context = context;
        }

        // =====================================
        // CHECK SESSION AND COOKIE IDENTITY
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
        // GET ALL EVENTS
        // Both roles
        // =====================================
        [HttpGet]
        public async Task<IActionResult> GetEvents()
        {
            if (!HasValidSession())
                return Unauthorized("Invalid session.");

            var events = await _context.Events
                .AsNoTracking()
                .ToListAsync();

            return Ok(events);
        }

        // =====================================
        // GET EVENT BY ID
        // Both roles
        // =====================================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetEvent(int id)
        {
            if (!HasValidSession())
                return Unauthorized("Invalid session.");

            var raceEvent = await _context.Events
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    e => e.EventID == id);

            if (raceEvent == null)
                return NotFound("Event not found.");

            return Ok(raceEvent);
        }

        // =====================================
        // CREATE EVENT
        // Organiser only
        // =====================================
        [HttpPost]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> CreateEvent(
            EventRequest request)
        {
            if (!HasValidSession())
                return Unauthorized("Invalid session.");

            int organiserID = GetLoggedInUserID();

            var raceEvent = new Event
            {
                EventName = request.EventName,
                EventDate = request.EventDate,
                Location = request.Location,
                Description = request.Description,
                EventType = request.EventType,
                Distance = request.Distance,
                DateCreated = DateTime.UtcNow,
                OrganiserID = organiserID
            };

            _context.Events.Add(raceEvent);

            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetEvent),
                new { id = raceEvent.EventID },
                raceEvent);
        }

        // =====================================
        // UPDATE EVENT
        // Owning Organiser only
        // =====================================
        [HttpPut("{id}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> UpdateEvent(
            int id,
            EventRequest request)
        {
            if (!HasValidSession())
                return Unauthorized("Invalid session.");

            int organiserID = GetLoggedInUserID();

            var raceEvent = await _context.Events
                .FindAsync(id);

            if (raceEvent == null)
                return NotFound("Event not found.");

            if (raceEvent.OrganiserID != organiserID)
                return Forbid();

            raceEvent.EventName = request.EventName;
            raceEvent.EventDate = request.EventDate;
            raceEvent.Location = request.Location;
            raceEvent.Description = request.Description;
            raceEvent.EventType = request.EventType;
            raceEvent.Distance = request.Distance;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // =====================================
        // DELETE EVENT
        // Owning Organiser only
        // =====================================
        [HttpDelete("{id}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            if (!HasValidSession())
                return Unauthorized("Invalid session.");

            int organiserID = GetLoggedInUserID();

            var raceEvent = await _context.Events
                .FindAsync(id);

            if (raceEvent == null)
                return NotFound("Event not found.");

            if (raceEvent.OrganiserID != organiserID)
                return Forbid();

            _context.Events.Remove(raceEvent);

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }

    // =====================================
    // EVENT REQUEST MODEL
    // Only fields users are allowed to edit
    // =====================================
    public class EventRequest
    {
        [Required]
        [StringLength(100)]
        public string EventName { get; set; }
            = string.Empty;

        [Required]
        public DateTime EventDate { get; set; }

        [Required]
        [StringLength(200)]
        public string Location { get; set; }
            = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        [RegularExpression(
            "^(Run|Walk|Cycle)$",
            ErrorMessage =
                "Event type must be Run, Walk or Cycle.")]
        public string EventType { get; set; }
            = string.Empty;

        [Range(
            typeof(decimal),
            "0.01",
            "999999.99")]
        public decimal Distance { get; set; }
    }
}
