using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.Api.Models
{//start of namespace

    //maps to the Enrolment table
    [Table("Enrolment")]
    public class Enrolment
    {//start of class

        [Key]
        [Column("enrolment_id")]
        public int EnrolmentId { get; set; }

        //foreign key to the Participant who enrolled
        [Column("participant_id")]
        public int ParticipantId { get; set; }

        [ForeignKey("ParticipantId")]
        public User? Participant { get; set; }

        //foreign key to the event
        [Column("event_id")]
        public int EventId { get; set; }

        [ForeignKey("EventId")]
        public Event? Event { get; set; }

        //foreign key to the chosen category
        [Column("category_id")]
        public int CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public Category? Category { get; set; }

        [Column("enrolment_date")]
        public DateTime EnrolmentDate { get; set; } = DateTime.Now;

    }//end of class
}//end of namespace