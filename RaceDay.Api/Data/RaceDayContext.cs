using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Models;

namespace RaceDay.Api.Data
{//start of namespace

    public class RaceDayContext : DbContext
    {//start of class

        public RaceDayContext(DbContextOptions<RaceDayContext> options) : base(options)
        {//start of constructor
        }//end of constructor

        //one DbSet per table
        public DbSet<User> Users { get; set; }
        public DbSet<EventType> EventTypes { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Enrolment> Enrolments { get; set; }
        public DbSet<Result> Results { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {//start of OnModelCreating

            //email must be unique (matches the UNIQUE constraint in the SQL script)
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            //no cascade deletes, matching the SQL script and avoiding SQL Server multiple cascade path errors
            foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {//start of foreach
                relationship.DeleteBehavior = DeleteBehavior.Restrict;
            }//end of foreach

            base.OnModelCreating(modelBuilder);

        }//end of OnModelCreating

    }//end of class
}//end of namespace