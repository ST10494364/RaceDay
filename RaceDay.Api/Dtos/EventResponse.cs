using System.Text.Json.Serialization;

namespace RaceDay.Api.Dtos
{//start of namespace

    //what the API returns about an event
    public class EventResponse
    {//start of class

        [JsonPropertyName("event_id")]
        public int EventId { get; set; }

        [JsonPropertyName("organiser_id")]
        public int OrganiserId { get; set; }

        [JsonPropertyName("event_type_id")]
        public int EventTypeId { get; set; }

        [JsonPropertyName("event_type_name")]
        public string EventTypeName { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("event_date")]
        public DateTime EventDate { get; set; }

        [JsonPropertyName("location")]
        public string Location { get; set; } = string.Empty;

        [JsonPropertyName("distance_km")]
        public decimal DistanceKm { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

    }//end of class
}//end of namespace