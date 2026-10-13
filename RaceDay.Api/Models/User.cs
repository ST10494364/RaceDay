using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.Api.Models
{//start of namespace

    //maps to the User table in the Part 1 SQL script
    [Table("User")]
    public class User
    {//start of class

        //primary key
        [Key]
        [Column("user_id")]
        public int UserId { get; set; }

        //user's full name
        [Required, MaxLength(100)]
        [Column("full_name")]
        public string FullName { get; set; } = string.Empty;

        //login email
        [Required, MaxLength(100)]
        [Column("email")]
        public string Email { get; set; } = string.Empty;

        //hashed password (the original password is never stored)
        [Required, MaxLength(255)]
        [Column("password_hash")]
        public string PasswordHash { get; set; } = string.Empty;

        //Organiser or Participant
        [Required, MaxLength(20)]
        [Column("role")]
        public string Role { get; set; } = string.Empty;

        //date and time the account was created
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

    }//end of class
}//end of namespace