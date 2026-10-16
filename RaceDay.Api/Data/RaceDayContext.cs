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

            //event date is a date only (no time), matching the Part 1 SQL script
            modelBuilder.Entity<Event>()
                .Property(e => e.EventDate)
                .HasColumnType("date");

            //seed the three event types, matching the Part 1 SQL script
            modelBuilder.Entity<EventType>().HasData(
                new EventType { EventTypeId = 1, TypeName = "Run" },
                new EventType { EventTypeId = 2, TypeName = "Walk" },
                new EventType { EventTypeId = 3, TypeName = "Cycle" }
            );

            //database-level defaults, matching the DEFAULT constraints in the Part 1 SQL script
            modelBuilder.Entity<User>()
                .Property(u => u.CreatedAt)
                .HasDefaultValueSql("GETDATE()");

            modelBuilder.Entity<Enrolment>()
                .Property(en => en.EnrolmentDate)
                .HasDefaultValueSql("GETDATE()");

            modelBuilder.Entity<Event>()
                .Property(e => e.Status)
                .HasDefaultValue("Open");

            //no cascade deletes, matching the SQL script and avoiding SQL Server multiple cascade path errors
            foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {//start of foreach
                relationship.DeleteBehavior = DeleteBehavior.Restrict;
            }//end of foreach

            base.OnModelCreating(modelBuilder);

        }//end of OnModelCreating
        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {//start of ConfigureConventions

            //use varchar instead of nvarchar, matching the Part 1 SQL script
            configurationBuilder.Properties<string>().AreUnicode(false);

            //use datetime instead of datetime2, matching the Part 1 SQL script
            configurationBuilder.Properties<DateTime>().HaveColumnType("datetime");

        }//end of ConfigureConventions

    }//end of class
}//end of namespace