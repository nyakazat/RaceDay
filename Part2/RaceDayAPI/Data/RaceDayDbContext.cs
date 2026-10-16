
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Models;

namespace RaceDayAPI.Data
{
    public class RaceDayDbContext : DbContext
    {
        public RaceDayDbContext(
            DbContextOptions<RaceDayDbContext> options)
            : base(options)
        {
        }

        // Database Tables
        public DbSet<Role> Roles { get; set; }
        public DbSet<AppUser> AppUsers { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<EventCategory> EventCategories { get; set; }
        public DbSet<Enrolment> Enrolments { get; set; }
        public DbSet<Result> Results { get; set; }

        protected override void OnModelCreating(
            ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // =====================================
            // ROLE AND USER RELATIONSHIP
            // =====================================
            modelBuilder.Entity<AppUser>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleID)
                .OnDelete(DeleteBehavior.Restrict);

            // =====================================
            // ORGANISER AND EVENT RELATIONSHIP
            // =====================================
            modelBuilder.Entity<Event>()
                .HasOne(e => e.Organiser)
                .WithMany(u => u.OrganisedEvents)
                .HasForeignKey(e => e.OrganiserID)
                .OnDelete(DeleteBehavior.Restrict);

            // =====================================
            // EVENT AND EVENT CATEGORY
            // =====================================
            modelBuilder.Entity<EventCategory>()
                .HasOne(ec => ec.Event)
                .WithMany(e => e.EventCategories)
                .HasForeignKey(ec => ec.EventID)
                .OnDelete(DeleteBehavior.Cascade);

            // =====================================
            // CATEGORY AND EVENT CATEGORY
            // =====================================
            modelBuilder.Entity<EventCategory>()
                .HasOne(ec => ec.Category)
                .WithMany(c => c.EventCategories)
                .HasForeignKey(ec => ec.CategoryID)
                .OnDelete(DeleteBehavior.Restrict);

            // =====================================
            // PARTICIPANT AND ENROLMENT
            // =====================================
            modelBuilder.Entity<Enrolment>()
                .HasOne(en => en.User)
                .WithMany(u => u.Enrolments)
                .HasForeignKey(en => en.UserID)
                .OnDelete(DeleteBehavior.Restrict);

            // =====================================
            // EVENT CATEGORY AND ENROLMENT
            // =====================================
            modelBuilder.Entity<Enrolment>()
                .HasOne(en => en.EventCategory)
                .WithMany(ec => ec.Enrolments)
                .HasForeignKey(en => en.EventCategoryID)
                .OnDelete(DeleteBehavior.Restrict);

            // =====================================
            // ENROLMENT AND RESULT
            // =====================================
            modelBuilder.Entity<Result>()
                .HasOne(r => r.Enrolment)
                .WithOne(en => en.Result)
                .HasForeignKey<Result>(r => r.EnrolmentID)
                .OnDelete(DeleteBehavior.Cascade);

            // =====================================
            // PREVENT DUPLICATE REGISTRATIONS
            // =====================================
            modelBuilder.Entity<Enrolment>()
                .HasIndex(en => new
                {
                    en.UserID,
                    en.EventCategoryID
                })
                .IsUnique();

            // =====================================
            // ONE RESULT PER ENROLMENT
            // =====================================
            modelBuilder.Entity<Result>()
                .HasIndex(r => r.EnrolmentID)
                .IsUnique();

            // =====================================
            // DEFAULT USER ROLES
            // =====================================
            modelBuilder.Entity<Role>().HasData(
                new Role
                {
                    RoleID = 1,
                    RoleName = "Organiser"
                },
                new Role
                {
                    RoleID = 2,
                    RoleName = "Participant"
                }
            );
        }
    }
}
