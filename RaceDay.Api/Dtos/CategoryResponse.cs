using System.Text.Json.Serialization;

namespace RaceDay.Api.Dtos
{//start of namespace

    //what the API returns about a category
    public class CategoryResponse
    {//start of class

        [JsonPropertyName("category_id")]
        public int CategoryId { get; set; }

        [JsonPropertyName("event_id")]
        public int EventId { get; set; }

        [JsonPropertyName("category_name")]
        public string CategoryName { get; set; } = string.Empty;

    }//end of class
}//end of namespace