using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace RaceDay.Api.Dtos
{//start of namespace

    //request body for POST /api/events/{id}/results
    public class CreateResultRequest
    {//start of class

        [JsonPropertyName("participant_id")]
        public int ParticipantId { get; set; }

        //finish time as hours:minutes:seconds, for example "00:45:30"
        [JsonPropertyName("finish_time")]
        public TimeSpan FinishTime { get; set; }

        [Range(1, 100000)]
        [JsonPropertyName("finishing_position")]
        public int FinishingPosition { get; set; }

    }//end of class
}//end of namespace