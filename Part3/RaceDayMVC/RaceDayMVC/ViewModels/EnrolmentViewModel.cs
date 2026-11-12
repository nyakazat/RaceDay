
namespace RaceDayMVC.ViewModels
{
    public class EnrolmentViewModel
    {
        public int EnrolmentID { get; set; }

        public int UserID { get; set; }

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public int EventCategoryID { get; set; }

        public int EventID { get; set; }

        public DateTime EnrolmentDate { get; set; }

        public string Status { get; set; } = string.Empty;
    }
}
