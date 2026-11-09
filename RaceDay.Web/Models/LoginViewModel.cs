using System.ComponentModel.DataAnnotations;

namespace RaceDay.Web.Models
{//start of namespace

    //what the login form collects
    public class LoginViewModel
    {//start of class

        [Required(ErrorMessage = "Please enter your email address.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter your password.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

    }//end of class
}//end of namespace