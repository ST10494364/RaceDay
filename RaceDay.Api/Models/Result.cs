using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.Api.Models
{//start of namespace

    //maps to the Result table
    [Table("Result")]
    public class Result
    {//start of class

        [Key]
        [Column("result_id")]
        public int ResultId { get; set; }

        //foreign key to the Participant who finished
        [Column("participant_id")]
        public int ParticipantId { get; set; }

        [ForeignKey("ParticipantId")]
        public User? Participant { get; set; }

        //foreign key to the event
        [Column("event_id")]
        public int EventId { get; set; }

        [ForeignKey("EventId")]
        public Event? Event { get; set; }

        [Column("finish_time")]
        public TimeSpan FinishTime { get; set; }

        [Column("finishing_position")]
        public int FinishingPosition { get; set; }

    }//end of class
}//end of namespace