using RegentHealth.Helpers;
using RegentHealth.Models;
using RegentHealth.Services;
using System.Collections.ObjectModel;
using YourProject.Models;

public class DataService
{
    //  Singleton 
    public static DataService Instance { get; } = new DataService();

    // SERVICES
    public AuthService AuthService { get; }

    public List<Doctor> Doctors { get; set; }
    public List<DoctorSchedule> DoctorSchedules { get; set; } = new();


    // DATA
    public List<User> Users { get; set; }

    public ObservableCollection<Appointment> Appointments { get; set; }
        = new ObservableCollection<Appointment>();


    // CONSTRUCTOR
    private DataService()
    {
        Users = new List<User>();
        Doctors = new List<Doctor>();

        // create service once
        AuthService = new AuthService(this);

        SeedAdmin();
        SeedDoctor();

        // TEMP for test
        DoctorSchedules.Add(new DoctorSchedule
        {
            Id = 1,
            DoctorId = 1,
            DayOfWeek = DayOfWeek.Monday,
            WorkStart = new TimeSpan(9, 0, 0),
            WorkEnd = new TimeSpan(17, 0, 0),
            BreakStart = new TimeSpan(13, 0, 0),
            BreakEnd = new TimeSpan(14, 0, 0)
        });
    }

    // SEED DATA
    private void SeedAdmin()
    {
        Users.Add(new User
        {
            Id = 1,
            Name = "System",
            Surname = "Admin",
            Email = "admin@admin.com",
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
            WorkStart = new TimeSpan(9, 0, 0),
            WorkEnd = new TimeSpan(17, 0, 0),
            LunchStart = new TimeSpan(13, 0, 0),
            LunchEnd = new TimeSpan(14, 0, 0),
            AppointmentDurationMinutes = 20,
            IsActive = true
        });
    }

    // HELPERS

    public static void CancelAppointment(Appointment appointment)
    {
        Instance.Appointments.Remove(appointment);
    }
}





