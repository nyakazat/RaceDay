
using System;
using System.ComponentModel.DataAnnotations;

namespace RaceDayMVC.ViewModels
{
    public class ResultViewModel
    {
        public int ResultID { get; set; }

        public int EnrolmentID { get; set; }

        public int UserID { get; set; }

        public int EventID { get; set; }

        public TimeSpan FinishTime { get; set; }

        public int FinishPosition { get; set; }

        public int Points { get; set; }

        public DateTime DateRecorded { get; set; }
    }
}
