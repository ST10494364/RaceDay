using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.Api.Models
{//start of namespace

    //maps to the EventType table
    [Table("EventType")]
    public class EventType
    {//start of class

        //primary key
        [Key]
        [Column("event_type_id")]
        public int EventTypeId { get; set; }

        //Run, Walk or Cycle
        [Required, MaxLength(20)]
        [Column("type_name")]
        public string TypeName { get; set; } = string.Empty;

    }//end of class
}//end of namespace