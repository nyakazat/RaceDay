
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDayAPI.Models
{
    public class Event
    {
        [Key]
        public int EventID { get; set; }

        [Required]
        [StringLength(100)]
        public string EventName { get; set; } = string.Empty;

        [Required]
        public DateTime EventDate { get; set; }

        [Required]
        [StringLength(200)]
        public string Location { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        // Event type
        [Required]
        [StringLength(20)]
        public string EventType { get; set; } = string.Empty;

        // Distance with SQL decimal precision
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Distance { get; set; }

        // Date created
        public DateTime DateCreated { get; set; }
            = DateTime.Now;

        // Required organiser foreign key
        [Required]
        public int OrganiserID { get; set; }

        // FIX: Nullable navigation property
        [ForeignKey(nameof(OrganiserID))]
        public AppUser? Organiser { get; set; }

        // Event categories
        public ICollection<EventCategory> EventCategories
        { get; set; } = new List<EventCategory>();
    }
}
