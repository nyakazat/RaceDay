
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
    public class CategoriesController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public CategoriesController(RaceDayDbContext context)
        {
            _context = context;
        }

        // =====================================
        // VALIDATE AUTHENTICATED SESSION
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

        // =====================================
        // GET ALL CATEGORIES
        // Organiser and Participant
        // =====================================
        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            if (!HasValidSession())
                return Unauthorized("Invalid session.");

            var categories = await _context.Categories
                .AsNoTracking()
                .Select(c => new
                {
                    c.CategoryID,
                    c.CategoryName,
                    c.CategoryDescription
                })
                .ToListAsync();

            return Ok(categories);
        }

        // =====================================
        // GET CATEGORY BY ID
        // Organiser and Participant
        // =====================================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCategory(int id)
        {
            if (!HasValidSession())
                return Unauthorized("Invalid session.");

            var category = await _context.Categories
                .AsNoTracking()
                .Where(c => c.CategoryID == id)
                .Select(c => new
                {
                    c.CategoryID,
                    c.CategoryName,
                    c.CategoryDescription
                })
                .FirstOrDefaultAsync();

            if (category == null)
                return NotFound("Category not found.");

            return Ok(category);
        }

        // =====================================
        // CREATE CATEGORY
        // Organiser only
        // =====================================
        [HttpPost]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> CreateCategory(
            CategoryRequest request)
        {
            if (!HasValidSession())
                return Unauthorized("Invalid session.");

            var category = new Category
            {
                CategoryName = request.CategoryName.Trim(),
                CategoryDescription =
                    request.CategoryDescription
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetCategory),
                new { id = category.CategoryID },
                new
                {
                    category.CategoryID,
                    category.CategoryName,
                    category.CategoryDescription
                });
        }

        // =====================================
        // UPDATE CATEGORY
        // Organiser only
        // =====================================
        [HttpPut("{id}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> UpdateCategory(
            int id,
            CategoryRequest request)
        {
            if (!HasValidSession())
                return Unauthorized("Invalid session.");

            var category = await _context.Categories
                .FindAsync(id);

            if (category == null)
                return NotFound("Category not found.");

            category.CategoryName =
                request.CategoryName.Trim();

            category.CategoryDescription =
                request.CategoryDescription;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // =====================================
        // DELETE CATEGORY
        // Organiser only
        // =====================================
        [HttpDelete("{id}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            if (!HasValidSession())
                return Unauthorized("Invalid session.");

            var category = await _context.Categories
                .FindAsync(id);

            if (category == null)
                return NotFound("Category not found.");

            // Prevent deletion if this category
            // is already assigned to events
            bool isUsed = await _context.EventCategories
                .AnyAsync(ec => ec.CategoryID == id);

            if (isUsed)
            {
                return Conflict(
                    "This category is linked to events " +
                    "and cannot be deleted.");
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }

    // =====================================
    // CATEGORY REQUEST DTO
    // =====================================
    public class CategoryRequest
    {
        [Required]
        [StringLength(100)]
        public string CategoryName { get; set; }
            = string.Empty;

        [StringLength(300)]
        public string? CategoryDescription { get; set; }
    }
}
