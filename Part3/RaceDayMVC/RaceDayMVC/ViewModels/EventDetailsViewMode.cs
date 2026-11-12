
using System.Collections.Generic;

namespace RaceDayMVC.ViewModels
{
    public class EventDetailsViewModel
    {
        public EventViewModel Event { get; set; }
            = new EventViewModel();

        public List<EventCategoryViewModel> Categories { get; set; }
            = new List<EventCategoryViewModel>();
    }
}
