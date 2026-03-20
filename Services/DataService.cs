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

    // In-memory data (will be replaced by DbContext calls later)
    public List<User> Users { get; set; }
    public List<Doctor> Doctors { get; set; }
    public ObservableCollection<Appointment> Appointments { get; set; }
    public Queue<Appointment> EmergencyQueue { get; set; }
    public List<DoctorRotation> WeeklyRotations { get; set; }

    private DataService()
    {
        Users = new List<User>();
        Doctors = new List<Doctor>();
        Appointments = new ObservableCollection<Appointment>();
        EmergencyQueue = new Queue<Appointment>();
        WeeklyRotations = new List<DoctorRotation>();

        AuthService = new AuthService(this);
        AdminService = new AdminService(this);

        SeedAdmin();
        SeedDoctor();
    }

    // ── Seed ─────────────────────────────────────────────────────────
    private void SeedAdmin()
    {
        Users.Add(new User
        {
            Id = 1,
            Name = "System",
            Surname = "Admin",
            Email = "admin",
            PasswordHash = PasswordHelper.HashPassword("admin"),
            Role = UserRole.Admin
        });
    }

    private void SeedDoctor()
    {
        var doctorUser = new User
        {
            Id = 2,
            Name = "John",
            Surname = "Smith",
            Email = "doctor@test.com",
            PasswordHash = PasswordHelper.HashPassword("123"),
            Role = UserRole.Doctor
        };

        Users.Add(doctorUser);

        Doctors.Add(new Doctor
        {
            Id = 1,
            UserId = doctorUser.Id,
            User = doctorUser,
            WorkStart = new TimeSpan(9, 0, 0),
            WorkEnd = new TimeSpan(17, 0, 0),
            IsActive = true,
            IsOnShift = false,
            WorkingDays = new List<DayOfWeek>
            {
                DayOfWeek.Monday,
                DayOfWeek.Tuesday,
                DayOfWeek.Wednesday,
                DayOfWeek.Thursday,
                DayOfWeek.Friday
            }
        });
    }

    public static void CancelAppointment(Appointment appointment)
    {
        Instance.Appointments.Remove(appointment);
    }
}

