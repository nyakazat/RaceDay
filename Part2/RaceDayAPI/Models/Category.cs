
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RaceDayAPI.Models
{
    public class Category
    {
        [Key]
        public int CategoryID { get; set; }

        [Required]
        [StringLength(100)]
        public string CategoryName { get; set; } = string.Empty;

        [StringLength(300)]
        public string? CategoryDescription { get; set; }

        // Relationship with EventCategory
        public ICollection<EventCategory> EventCategories
        { get; set; } = new List<EventCategory>();
    }
}
