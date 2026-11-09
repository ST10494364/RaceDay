namespace RaceDay.Web.Models
{//start of namespace

    //what the home page shows: the filtered events and the filter values
    public class HomeViewModel
    {//start of class

        public List<EventDto> Events { get; set; } = new List<EventDto>();
        public string? Search { get; set; }
        public string? Type { get; set; }

    }//end of class

    //what the event details page shows
    public class EventDetailsViewModel
    {//start of class

        public EventDto Event { get; set; } = new EventDto();
        public List<CategoryDto> Categories { get; set; } = new List<CategoryDto>();
        public EnrolmentDto? MyEnrolment { get; set; }

    }//end of class
}//end of namespace