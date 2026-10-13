using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace RaceDay.Api.Dtos
{//start of namespace

    //request body for PUT /api/users/me
    public class UpdateProfileRequest
    {//start of class

        [Required, MaxLength(100)]
        [JsonPropertyName("full_name")]
        public string FullName { get; set; } = string.Empty;

        [Required, EmailAddress, MaxLength(100)]
        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

    }//end of class
}//end of namespace