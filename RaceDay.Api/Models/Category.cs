using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.Api.Models
{//start of namespace

    //maps to the Category table
    [Table("Category")]
    public class Category
    {//start of class

        [Key]
        [Column("category_id")]
        public int CategoryId { get; set; }

        //foreign key to the event this category belongs to
        [Column("event_id")]
        public int EventId { get; set; }

        [ForeignKey("EventId")]
        public Event? Event { get; set; }

        [Required, MaxLength(50)]
        [Column("category_name")]
        public string CategoryName { get; set; } = string.Empty;

    }//end of class
}//end of namespace