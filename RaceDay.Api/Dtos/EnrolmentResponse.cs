using System.Text.Json.Serialization;

namespace RaceDay.Api.Dtos
{//start of namespace

    //what the API returns about an enrolment
    public class EnrolmentResponse
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

    }//end of class
}//end of namespace