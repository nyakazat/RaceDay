
using System;
using System.ComponentModel.DataAnnotations;

namespace RaceDayMVC.ViewModels
{
    public class EventViewModel
    {
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

        [Required]
        public string EventType { get; set; } = string.Empty;

        [Range(0.01, 999999.99)]
        public decimal Distance { get; set; }

        public DateTime DateCreated { get; set; }

        public int OrganiserID { get; set; }
    }
}
