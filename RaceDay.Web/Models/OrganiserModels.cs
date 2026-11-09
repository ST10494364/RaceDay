using System.ComponentModel.DataAnnotations;

namespace RaceDay.Web.Models
{//start of namespace

    //one event on the dashboard with its enrolment count
    public class DashboardRow
    {//start of class

        public EventDto Event { get; set; } = new EventDto();
        public int TotalEnrolments { get; set; }

    }//end of class

    //what the Organiser dashboard shows
    public class DashboardViewModel
    {//start of class

        public List<DashboardRow> Rows { get; set; } = new List<DashboardRow>();
        public int TotalEvents { get; set; }
        public int UpcomingEvents { get; set; }
        public int TotalEnrolments { get; set; }

    }//end of class

    //the create and edit event form
    public class EventFormViewModel
    {//start of class

        public int EventId { get; set; }
        public bool IsEdit { get; set; }
        public bool HasBanner { get; set; }

        [Required(ErrorMessage = "Please enter the event name.")]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please describe the event.")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please choose the event date.")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Event date")]
        public DateTime EventDate { get; set; }

        [Required(ErrorMessage = "Please enter the location.")]
        [MaxLength(200)]
        public string Location { get; set; } = string.Empty;

        [Range(0.1, 1000, ErrorMessage = "The distance must be between 0.1 and 1000 km.")]
        [Display(Name = "Distance (km)")]
        public decimal DistanceKm { get; set; } = 5;

        [Range(1, 3, ErrorMessage = "Please choose an event type.")]
        [Display(Name = "Event type")]
        public int EventTypeId { get; set; } = 1;

        public string Status { get; set; } = "Open";

        [Display(Name = "Banner image")]
        public IFormFile? Banner { get; set; }

    }//end of class
}//end of namespace