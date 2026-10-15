using System.Text.Json.Serialization;

namespace RaceDay.Api.Dtos
{//start of namespace

    //what the API returns about a result
    public class ResultResponse
    {//start of class

        [JsonPropertyName("result_id")]
        public int ResultId { get; set; }

        [JsonPropertyName("participant_id")]
        public int ParticipantId { get; set; }

        [JsonPropertyName("event_id")]
        public int EventId { get; set; }

        [JsonPropertyName("event_name")]
        public string EventName { get; set; } = string.Empty;

        [JsonPropertyName("finish_time")]
        public TimeSpan FinishTime { get; set; }

        [JsonPropertyName("finishing_position")]
        public int FinishingPosition { get; set; }

    }//end of class
}//end of namespace