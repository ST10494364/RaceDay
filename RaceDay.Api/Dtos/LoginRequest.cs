using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace RaceDay.Api.Dtos
{//start of namespace

    //request body for POST /api/auth/login
    public class LoginRequest
    {//start of class

        [Required]
        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [JsonPropertyName("password")]
        public string Password { get; set; } = string.Empty;

    }//end of class
}//end of namespace