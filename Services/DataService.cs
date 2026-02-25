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

    // HELPERS

    public static void CancelAppointment(Appointment appointment)
    {
        Instance.Appointments.Remove(appointment);
    }
}





