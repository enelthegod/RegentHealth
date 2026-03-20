using Microsoft.EntityFrameworkCore;
using RegentHealth.Models;

namespace RegentHealth.Data
{
    public class AppDbContext : DbContext
    {
        // ── Tables ───────────────────────────────────────────────────
        public DbSet<User> Users { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<DoctorRotation> DoctorRotations { get; set; }

        // Constructor for DesignTimeDbContextFactory (EF Tools)
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // Constructor for runtime (App.xaml.cs uses this)
        public AppDbContext() { }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            // SQLite file will be created next to the .exe
            options.UseSqlite("Data Source=regenthealth.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // ── User ─────────────────────────────────────────────────
            modelBuilder.Entity<User>(e =>
            {
                e.HasKey(u => u.Id);
                e.Property(u => u.Name).IsRequired();
                e.Property(u => u.Surname).IsRequired();
                e.Property(u => u.Email).IsRequired();
                e.HasIndex(u => u.Email).IsUnique(); // no duplicate emails
                e.Property(u => u.PasswordHash).IsRequired();
                e.Property(u => u.Role).HasConversion<string>(); // store as "Admin"/"Doctor"/"Patient"
            });

            // ── Doctor ───────────────────────────────────────────────
            modelBuilder.Entity<Doctor>(e =>
            {
                e.HasKey(d => d.Id);

                // FK → User (one User has one Doctor profile)
                e.HasOne(d => d.User)
                 .WithOne()
                 .HasForeignKey<Doctor>(d => d.UserId)
                 .OnDelete(DeleteBehavior.Cascade);

                // TimeSpan stored as ticks (long) — SQLite has no native TimeSpan
                e.Property(d => d.WorkStart)
                 .HasConversion(
                     ts => ts.Ticks,
                     ticks => TimeSpan.FromTicks(ticks));

                e.Property(d => d.WorkEnd)
                 .HasConversion(
                     ts => ts.Ticks,
                     ticks => TimeSpan.FromTicks(ticks));

                // WorkingDays stored as "Monday,Tuesday,..." string
                e.Property(d => d.WorkingDaysRaw).HasColumnName("WorkingDays");
            });

            // ── Appointment ──────────────────────────────────────────
            modelBuilder.Entity<Appointment>(e =>
            {
                e.HasKey(a => a.Id);

                // Patient FK
                e.HasOne(a => a.Patient)
                 .WithMany()
                 .HasForeignKey(a => a.PatientId)
                 .OnDelete(DeleteBehavior.Restrict);

                // Doctor FK (links to User, not Doctor table)
                e.HasOne(a => a.Doctor)
                 .WithMany()
                 .HasForeignKey(a => a.DoctorId)
                 .OnDelete(DeleteBehavior.Restrict);

                e.Property(a => a.Type).HasConversion<string>();
                e.Property(a => a.Status).HasConversion<string>();
            });

            // ── DoctorRotation ───────────────────────────────────────
            modelBuilder.Entity<DoctorRotation>(e =>
            {
                e.HasKey(r => r.Id);

                e.HasOne(r => r.Doctor)
                 .WithMany()
                 .HasForeignKey(r => r.DoctorId)
                 .OnDelete(DeleteBehavior.Cascade);

                e.Property(r => r.Day).HasConversion<string>(); // store as "Monday" etc
            });

            // ── Seed data — admin always exists ─────────────────────
            modelBuilder.Entity<User>().HasData(new User
            {
                Id = 1,
                Name = "System",
                Surname = "Admin",
                Email = "admin",
                PasswordHash = RegentHealth.Helpers.PasswordHelper.HashPassword("admin"),
                Role = UserRole.Admin
            });
        }
    }
}