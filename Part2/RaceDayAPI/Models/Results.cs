
using System;
using System.ComponentModel.DataAnnotations;

namespace RaceDayAPI.Models
{
    public class Result
    {
        [Key]
        public int ResultID { get; set; }

        [Required]
        public int EnrolmentID { get; set; }

        [Required]
        public TimeSpan FinishTime { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int FinishPosition { get; set; }

        public int Points { get; set; } = 0;

        public DateTime DateRecorded { get; set; }
            = DateTime.Now;

        // Relationship with Enrolment
        public Enrolment Enrolment { get; set; }
            = null!;
    }
}
