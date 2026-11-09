using System.Text.Json.Serialization;

namespace RaceDay.Web.Models
{//start of namespace

    //an event as the API describes it
    public class EventDto
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

        [JsonPropertyName("banner_image_url")]
        public string? BannerImageUrl { get; set; }

    }//end of class

    //a category (age or distance group) of an event
    public class CategoryDto
    {//start of class

        [JsonPropertyName("category_id")]
        public int CategoryId { get; set; }

        [JsonPropertyName("event_id")]
        public int EventId { get; set; }

        [JsonPropertyName("category_name")]
        public string CategoryName { get; set; } = string.Empty;

    }//end of class

    //an enrolment of a Participant in an event
    public class EnrolmentDto
    {//start of class

        [JsonPropertyName("enrolment_id")]
        public int EnrolmentId { get; set; }

        [JsonPropertyName("participant_id")]
        public int ParticipantId { get; set; }

        [JsonPropertyName("participant_name")]
        public string ParticipantName { get; set; } = string.Empty;

        [JsonPropertyName("event_id")]
        public int EventId { get; set; }

        [JsonPropertyName("category_id")]
        public int CategoryId { get; set; }

        [JsonPropertyName("category_name")]
        public string CategoryName { get; set; } = string.Empty;

        [JsonPropertyName("enrolment_date")]
        public DateTime EnrolmentDate { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

    }//end of class

    //a finishing result of a Participant in an event
    public class ResultDto
    {//start of class

        [JsonPropertyName("result_id")]
        public int ResultId { get; set; }

        [JsonPropertyName("participant_id")]
        public int ParticipantId { get; set; }

        [JsonPropertyName("event_id")]
        public int EventId { get; set; }

        [JsonPropertyName("event_name")]
        public string EventName { get; set; } = string.Empty;

        [JsonPropertyName("event_date")]
        public DateTime EventDate { get; set; }

        [JsonPropertyName("category_name")]
        public string CategoryName { get; set; } = string.Empty;

        [JsonPropertyName("finish_time")]
        public TimeSpan FinishTime { get; set; }

        [JsonPropertyName("finishing_position")]
        public int FinishingPosition { get; set; }

        [JsonPropertyName("total_finishers")]
        public int TotalFinishers { get; set; }

    }//end of class
}//end of namespace