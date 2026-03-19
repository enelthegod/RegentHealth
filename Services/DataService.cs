using RegentHealth.Helpers;
using RegentHealth.Models;
using RegentHealth.Services;
using System.Collections.ObjectModel;

public class DataService
{
    // Singleton
    public static DataService Instance { get; } = new DataService();

    // SERVICES
    public AuthService AuthService { get; }
    public AdminService AdminService { get; }

    // DATA
    public List<Doctor> Doctors { get; set; }
    public List<User> Users { get; set; }

    public ObservableCollection<Appointment> Appointments { get; set; }
        = new ObservableCollection<Appointment>();

    public Queue<Appointment> EmergencyQueue { get; set; }
        = new Queue<Appointment>();

    public List<DoctorRotation> WeeklyRotations { get; set; }
        = new List<DoctorRotation>();

    // CONSTRUCTOR
    private DataService()
    {
        Users = new List<User>();
        Doctors = new List<Doctor>();

        AuthService = new AuthService(this);
        AdminService = new AdminService(this);

        SeedAdmin();
        SeedDoctor();
    }

    // SEED DATA
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
            UserId = doctorUser.Id,
            WorkStart = new TimeSpan(9, 0, 0),
            WorkEnd = new TimeSpan(17, 0, 0),
            IsActive = true,
            IsOnShift = false,
            LastLogin = null,

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

    // HELPERS
    public static void CancelAppointment(Appointment appointment)
    {
        Instance.Appointments.Remove(appointment);
    }
}



