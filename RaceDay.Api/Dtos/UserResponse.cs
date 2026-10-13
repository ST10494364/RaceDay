using System.Text.Json.Serialization;

namespace RaceDay.Api.Dtos
{//start of namespace

    //what the API returns about a user (never includes the password hash)
    public class UserResponse
    {//start of class

        [JsonPropertyName("user_id")]
        public int UserId { get; set; }

        [JsonPropertyName("full_name")]
        public string FullName { get; set; } = string.Empty;

        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("role")]
        public string Role { get; set; } = string.Empty;

    }//end of class
}//end of namespace