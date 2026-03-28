using Microsoft.EntityFrameworkCore;
using RegentHealth.Data;
using RegentHealth.Helpers;
using RegentHealth.Models;
using RegentHealth.Services;
using System.Collections.ObjectModel;

public class DataService
{
    // Singleton
    public static DataService Instance { get; } = new DataService();

    // Services
    public AuthService AuthService { get; }
    public AdminService AdminService { get; }

    // In-memory data — loaded from DB on startup
    public List<User> Users { get; set; }
    public List<Doctor> Doctors { get; set; }
    public ObservableCollection<Appointment> Appointments { get; set; }
    public List<DoctorRotation> WeeklyRotations { get; set; }

    // Queue stays in memory only — no need to persist
    public Queue<Appointment> EmergencyQueue { get; set; } = new();

    private DataService()
    {
        // Load everything from SQLite on startup
        using var db = new AppDbContext();

        Users = db.Users.ToList();

        // Include loads the related User object into each Doctor
        // so Doctor.FullName (which uses Doctor.User) works correctly
        Doctors = db.Doctors
            .Include(d => d.User)
            .ToList();

        // Include loads Patient and Doctor User objects
        // so Appointment.DoctorFullName and PatientFullName work
        Appointments = new ObservableCollection<Appointment>(
            db.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .ToList());

        WeeklyRotations = db.DoctorRotations
            .Include(r => r.Doctor)
            .ToList();

        AuthService = new AuthService(this);
        AdminService = new AdminService(this);

        // Seed doctor only if no doctors exist yet
        // (admin is seeded separately via AppDbContext.HasData)
        if (!Doctors.Any())
            SeedInitialData(db);
    }

    // ── First-launch seed ─────────────────────────────────────────────
    // Runs only once when the DB is brand new and empty
    private void SeedInitialData(AppDbContext db)
    {
        var admin = new User
        {
            Name = "System",
            Surname = "Admin",
            Email = "admin",
            PasswordHash = PasswordHelper.HashPassword("admin"),
            Role = UserRole.Admin
        };

        var doctorUser = new User
        {
            Name = "John",
            Surname = "Smith",
            Email = "doctor@test.com",
            PasswordHash = PasswordHelper.HashPassword("123"),
            Role = UserRole.Doctor
        };

        db.Users.AddRange(admin, doctorUser);
        db.SaveChanges(); // IDs are now assigned by SQLite

        var doctor = new Doctor
        {
            UserId = doctorUser.Id,
            User = doctorUser,
            WorkStart = new TimeSpan(9, 0, 0),
            WorkEnd = new TimeSpan(17, 0, 0),
            IsActive = true,
            IsOnShift = false,
            WorkingDays = new List<DayOfWeek>
            {
                DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday,
                DayOfWeek.Thursday, DayOfWeek.Friday
            }
        };

        db.Doctors.Add(doctor);
        db.SaveChanges();

        // Reload into memory so the rest of the app sees the data
        Users = db.Users.ToList();
        Doctors = db.Doctors.Include(d => d.User).ToList();
    }

    // ── Save helpers — call these after changing data ─────────────────
    public void SaveDoctors()
    {
        using var db = new AppDbContext();

        foreach (var doctor in Doctors)
        {
            // Check if this doctor already exists in DB
            var exists = db.Doctors.Any(d => d.Id == doctor.Id);

            if (exists)
            {
                // Update only the Doctor row, ignore navigation property User
                // Entry() gets the tracking info for this object
                // State = Modified tells EF "update this row"
                db.Entry(doctor).State = EntityState.Modified;

                // Do NOT update UserId and User navigation — those never change
                db.Entry(doctor).Property(d => d.UserId).IsModified = false;
            }
            else
            {
                // New doctor — add it
                db.Doctors.Add(doctor);
            }
        }

        db.SaveChanges();
    }

    public void SaveAppointments()
    {
        using var db = new AppDbContext();

        foreach (var appt in Appointments)
        {
            var exists = db.Appointments.Any(a => a.Id == appt.Id);

            if (exists)
            {
                // Update only Status — Patient and Doctor nav properties stay untouched
                db.Entry(appt).State = EntityState.Modified;
                db.Entry(appt).Property(a => a.PatientId).IsModified = false;
                db.Entry(appt).Property(a => a.DoctorId).IsModified = false;
            }
            else
            {
                db.Appointments.Add(appt);
            }
        }

        db.SaveChanges();
    }

    public void SaveRotations()
    {
        using var db = new AppDbContext();

        // Remove all existing rotations for this week
        db.DoctorRotations.RemoveRange(db.DoctorRotations);

        // Create clean objects without navigation properties
        // because EF gets confused when Doctor nav property is already tracked
        foreach (var r in WeeklyRotations)
        {
            db.DoctorRotations.Add(new RegentHealth.Models.DoctorRotation
            {
                DoctorId = r.DoctorId,
                Day = r.Day,
                IsEmergency = r.IsEmergency
                // Id = 0 means SQLite will assign a new Id automatically
                // Doctor navigation property intentionally left null here
            });
        }

        db.SaveChanges();
    }

}