
namespace RaceDayMVC.ViewModels
{
    public class EventCategoryViewModel
    {
        public int EventCategoryID { get; set; }

        public int EventID { get; set; }

        public string EventName { get; set; } = string.Empty;

        public int CategoryID { get; set; }

        public string CategoryName { get; set; } = string.Empty;
    }
}
