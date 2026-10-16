
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
    public class EventCategoriesController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public EventCategoriesController(
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
        // GET ALL EVENT CATEGORIES
        // =====================================
        [HttpGet]
        public async Task<IActionResult> GetEventCategories()
        {
            if (!HasValidSession())
                return Unauthorized("Invalid session.");

            var categories = await _context.EventCategories
                .AsNoTracking()
                .Select(ec => new
                {
                    ec.EventCategoryID,
                    ec.EventID,
                    EventName = ec.Event.EventName,
                    ec.CategoryID,
                    CategoryName = ec.Category.CategoryName
                })
                .ToListAsync();

            return Ok(categories);
        }

        // =====================================
        // GET EVENT CATEGORY BY ID
        // =====================================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetEventCategory(int id)
        {
            if (!HasValidSession())
                return Unauthorized("Invalid session.");

            var category = await _context.EventCategories
                .AsNoTracking()
                .Where(ec => ec.EventCategoryID == id)
                .Select(ec => new
                {
                    ec.EventCategoryID,
                    ec.EventID,
                    EventName = ec.Event.EventName,
                    ec.CategoryID,
                    CategoryName = ec.Category.CategoryName
                })
                .FirstOrDefaultAsync();

            if (category == null)
                return NotFound("Event category not found.");

            return Ok(category);
        }

        // =====================================
        // GET CATEGORIES FOR AN EVENT
        // =====================================
        [HttpGet("event/{eventId}")]
        public async Task<IActionResult> GetByEvent(int eventId)
        {
            if (!HasValidSession())
                return Unauthorized("Invalid session.");

            bool eventExists = await _context.Events
                .AnyAsync(e => e.EventID == eventId);

            if (!eventExists)
                return NotFound("Event not found.");

            var categories = await _context.EventCategories
                .AsNoTracking()
                .Where(ec => ec.EventID == eventId)
                .Select(ec => new
                {
                    ec.EventCategoryID,
                    ec.EventID,
                    ec.CategoryID,
                    ec.Category.CategoryName
                })
                .ToListAsync();

            return Ok(categories);
        }

        // =====================================
        // ASSIGN CATEGORY TO EVENT
        // Owning Organiser only
        // =====================================
        [HttpPost]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> CreateEventCategory(
            EventCategoryRequest request)
        {
            if (!HasValidSession())
                return Unauthorized("Invalid session.");

            int organiserID = GetUserID();

            var raceEvent = await _context.Events
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    e => e.EventID == request.EventID);

            if (raceEvent == null)
                return NotFound("Event not found.");

            if (raceEvent.OrganiserID != organiserID)
                return Forbid();

            bool categoryExists = await _context.Categories
                .AnyAsync(c =>
                    c.CategoryID == request.CategoryID);

            if (!categoryExists)
                return BadRequest("Category does not exist.");

            bool alreadyAssigned =
                await _context.EventCategories
                .AnyAsync(ec =>
                    ec.EventID == request.EventID &&
                    ec.CategoryID == request.CategoryID);

            if (alreadyAssigned)
                return Conflict("Category already assigned.");

            var eventCategory = new EventCategory
            {
                EventID = request.EventID,
                CategoryID = request.CategoryID
            };

            _context.EventCategories.Add(eventCategory);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetEventCategory),
                new { id = eventCategory.EventCategoryID },
                new
                {
                    eventCategory.EventCategoryID,
                    eventCategory.EventID,
                    eventCategory.CategoryID
                });
        }

        // =====================================
        // REMOVE CATEGORY FROM EVENT
        // Owning Organiser only
        // =====================================
        [HttpDelete("{id}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> DeleteEventCategory(int id)
        {
            if (!HasValidSession())
                return Unauthorized("Invalid session.");

            int organiserID = GetUserID();

            var eventCategory = await _context.EventCategories
                .Include(ec => ec.Event)
                .FirstOrDefaultAsync(
                    ec => ec.EventCategoryID == id);

            if (eventCategory == null)
                return NotFound("Event category not found.");

            if (eventCategory.Event.OrganiserID != organiserID)
                return Forbid();

            bool hasEnrolments = await _context.Enrolments
                .AnyAsync(e => e.EventCategoryID == id);

            if (hasEnrolments)
            {
                return Conflict(
                    "Cannot remove a category with enrolments.");
            }

            _context.EventCategories.Remove(eventCategory);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }

    // =====================================
    // EVENT CATEGORY REQUEST DTO
    // =====================================
    public class EventCategoryRequest
    {
        [Range(1, int.MaxValue)]
        public int EventID { get; set; }

        [Range(1, int.MaxValue)]
        public int CategoryID { get; set; }
    }
}
