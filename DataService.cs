using System;
using System.Security.Cryptography;
using System.Text;
using RegentHealth.Helpers;

public class DataService
{

    //one for everything auto
    public static DataService Instance { get; } = new DataService();

    //data list 
    public List<User> Users { get; set; }
    public List<Appointment>  Appointments { get; set; }

    //private constructor

    private DataService()
    {
        Users = new List<User>();
        Appointments = new List<Appointment>();

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
}

    
