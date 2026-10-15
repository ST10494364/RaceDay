using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace RaceDay.Api.Dtos
{//start of namespace

    //request body for PUT /api/events/{id}
    public class UpdateEventRequest
    {//start of class

        [Required, MaxLength(100)]
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [Required, MaxLength(500)]
        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("event_date")]
        public DateTime EventDate { get; set; }

        [Required, MaxLength(150)]
        [JsonPropertyName("location")]
        public string Location { get; set; } = string.Empty;

        [Range(0.01, 999.99)]
        [JsonPropertyName("distance_km")]
        public decimal DistanceKm { get; set; }

        [JsonPropertyName("event_type_id")]
        public int EventTypeId { get; set; }

        //Open, Closed, Completed or Cancelled
        [Required]
        [RegularExpression("^(Open|Closed|Completed|Cancelled)$", ErrorMessage = "Status must be Open, Closed, Completed or Cancelled.")]
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

    }//end of class
}//end of namespace