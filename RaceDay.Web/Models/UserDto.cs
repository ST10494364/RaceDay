using System.Text.Json.Serialization;

namespace RaceDay.Web.Models
{//start of namespace

    //a user as the API describes them
    public class UserDto
    {//start of class

        [JsonPropertyName("user_id")]
        public int UserId { get; set; }

        [JsonPropertyName("full_name")]
        public string FullName { get; set; } = string.Empty;

        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("role")]
        public string Role { get; set; } = string.Empty;

        [JsonPropertyName("profile_picture_url")]
        public string? ProfilePictureUrl { get; set; }

    }//end of class
}//end of namespace