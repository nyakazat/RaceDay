
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RaceDayAPI.Models
{
    public class EventCategory
    {
        [Key]
        public int EventCategoryID { get; set; }

        [Required]
        public int EventID { get; set; }

        [Required]
        public int CategoryID { get; set; }

        // Relationship with Event
        public Event Event { get; set; } = null!;

        // Relationship with Category
        public Category Category { get; set; } = null!;

        // Participants enrolled in this event category
        public ICollection<Enrolment> Enrolments { get; set; }
            = new List<Enrolment>();
    }
}
