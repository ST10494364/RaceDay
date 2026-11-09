using System.ComponentModel.DataAnnotations;

namespace RaceDay.Web.Models
{//start of namespace

    //what the register form collects
    public class RegisterViewModel
    {//start of class

        [Required(ErrorMessage = "Please enter your full name.")]
        [MaxLength(100)]
        [Display(Name = "Full name")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter your email address.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please choose a password.")]
        [MinLength(8, ErrorMessage = "Your password must be at least 8 characters long.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please confirm your password.")]
        [Compare("Password", ErrorMessage = "The two passwords do not match.")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please choose how you will use RaceDay.")]
        [RegularExpression("^(Organiser|Participant)$", ErrorMessage = "Please choose Organiser or Participant.")]
        [Display(Name = "I am a")]
        public string Role { get; set; } = string.Empty;

    }//end of class
}//end of namespace