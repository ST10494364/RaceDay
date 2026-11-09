using System.ComponentModel.DataAnnotations;

namespace RaceDay.Web.Models
{//start of namespace

    //one of the Participant's enrolments together with the event it belongs to
    public class MyEventRow
    {//start of class

        public EnrolmentDto Enrolment { get; set; } = new EnrolmentDto();
        public EventDto? Event { get; set; }

    }//end of class

    //the Profile page form
    public class ProfileViewModel
    {//start of class

        public int UserId { get; set; }
        public string Role { get; set; } = string.Empty;
        public bool HasPicture { get; set; }

        [Required(ErrorMessage = "Please enter your full name.")]
        [MaxLength(100)]
        [Display(Name = "Full name")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter your email address.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;

    }//end of class
}//end of namespace