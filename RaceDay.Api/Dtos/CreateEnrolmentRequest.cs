using System.Text.Json.Serialization;

namespace RaceDay.Api.Dtos
{//start of namespace

    //request body for POST /api/events/{id}/enrolments
    public class CreateEnrolmentRequest
    {//start of class

        [JsonPropertyName("category_id")]
        public int CategoryId { get; set; }

    }//end of class
}//end of namespace