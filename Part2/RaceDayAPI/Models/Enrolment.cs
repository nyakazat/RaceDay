
using System;
using System.ComponentModel.DataAnnotations;

namespace RaceDayAPI.Models
{
    public class Enrolment
    {
        [Key]
        public int EnrolmentID { get; set; }

        [Required]
        public int UserID { get; set; }

        [Required]
        public int EventCategoryID { get; set; }

        public DateTime EnrolmentDate { get; set; }
            = DateTime.Now;

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Active";

        // Relationship with Participant
        public AppUser User { get; set; } = null!;

        // Relationship with EventCategory
        public EventCategory EventCategory { get; set; }
            = null!;

        // Relationship with Result
        public Result? Result { get; set; }
    }
}
