using RegentHealth.Helpers;
using RegentHealth.Models;
using RegentHealth.Services;
using System.Collections.ObjectModel;

public class DataService
{
    //  Singleton 
    public static DataService Instance { get; } = new DataService();

    // SERVICES
    public AuthService AuthService { get; }

    public List<Doctor> Doctors { get; set; } = new List<Doctor>();


    // DATA
    public List<User> Users { get; set; }

    public ObservableCollection<Appointment> Appointments { get; set; }
        = new ObservableCollection<Appointment>();


    // CONSTRUCTOR
    private DataService()
    {
        Users = new List<User>();

        // create service once
        AuthService = new AuthService(this);

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
            Specialization = "General Practitioner",
            IsActive = true
        });
    }

    // HELPERS

    public static void CancelAppointment(Appointment appointment)
    {
        Instance.Appointments.Remove(appointment);
    }
}





