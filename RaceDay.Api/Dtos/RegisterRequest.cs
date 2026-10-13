using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace RaceDay.Api.Dtos
{//start of namespace

    //request body for POST /api/auth/register
    public class RegisterRequest
    {//start of class

        [Required, MaxLength(100)]
        [JsonPropertyName("full_name")]
        public string FullName { get; set; } = string.Empty;

        [Required, EmailAddress, MaxLength(100)]
        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [Required, MinLength(8), MaxLength(100)]
        [JsonPropertyName("password")]
        public string Password { get; set; } = string.Empty;

        //only Organiser or Participant is accepted
        [Required]
        [RegularExpression("^(Organiser|Participant)$", ErrorMessage = "Role must be Organiser or Participant.")]
        [JsonPropertyName("role")]
        public string Role { get; set; } = string.Empty;

    }//end of class
}//end of namespace