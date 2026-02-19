using RegentHealth.Helpers;
using RegentHealth.Models;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Collections.ObjectModel;
public class DataService
{

    //one for everything auto
    public static DataService Instance { get; } = new DataService();

    //data list 
    public List<User> Users { get; set; }

    public ObservableCollection<Appointment> Appointments { get; set; }
    = new ObservableCollection<Appointment>();



    //private constructor

    private DataService()
    {
        Users = new List<User>();     
        SeedAdmin();
    }

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

    public static void CancelAppointment(Appointment appointment)
    {
        Instance.Appointments.Remove(appointment);
    }

}





