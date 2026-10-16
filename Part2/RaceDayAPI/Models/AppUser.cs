
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RaceDayAPI.Models
{
    public class AppUser
    {
        [Key]
        public int UserID { get; set; }

        [Required]
        public int RoleID { get; set; }

        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string PasswordHash { get; set; } = string.Empty;

        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        public DateTime DateRegistered { get; set; }
            = DateTime.Now;

        // Relationship with Role
        public Role Role { get; set; } = null!;

        // Events organised by this user
        public ICollection<Event> OrganisedEvents { get; set; }
            = new List<Event>();

        // Event enrolments
        public ICollection<Enrolment> Enrolments { get; set; }
            = new List<Enrolment>();
    }
}
