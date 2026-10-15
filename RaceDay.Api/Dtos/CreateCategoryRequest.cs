using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace RaceDay.Api.Dtos
{//start of namespace

    //request body for POST /api/events/{id}/categories
    public class CreateCategoryRequest
    {//start of class

        [Required, MaxLength(50)]
        [JsonPropertyName("category_name")]
        public string CategoryName { get; set; } = string.Empty;

    }//end of class
}//end of namespace