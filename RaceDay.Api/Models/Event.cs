using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.Api.Models
{//start of namespace

    //maps to the Event table
    [Table("Event")]
    public class Event
    {//start of class

        [Key]
        [Column("event_id")]
        public int EventId { get; set; }

        //foreign key to the Organiser who created the event
        [Column("organiser_id")]
        public int OrganiserId { get; set; }

        [ForeignKey("OrganiserId")]
        public User? Organiser { get; set; }

        //foreign key to the event type (Run, Walk, Cycle)
        [Column("event_type_id")]
        public int EventTypeId { get; set; }

        [ForeignKey("EventTypeId")]
        public EventType? EventType { get; set; }

        [Required, MaxLength(100)]
        [Column("name")]
        public string Name { get; set; } = string.Empty;

        [Required, MaxLength(500)]
        [Column("description")]
        public string Description { get; set; } = string.Empty;

        [Column("event_date", TypeName = "date")]
        public DateTime EventDate { get; set; }

        [Required, MaxLength(150)]
        [Column("location")]
        public string Location { get; set; } = string.Empty;

        [Column("distance_km", TypeName = "decimal(5,2)")]
        public decimal DistanceKm { get; set; }

        [Required, MaxLength(20)]
        [Column("status")]
        public string Status { get; set; } = "Open";

    }//end of class
}//end of namespace