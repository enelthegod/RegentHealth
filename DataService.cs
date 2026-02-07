using System;

public class DataService
{
    //one for everything auto
    public static DataService Instance { get; } = new DataService();

    //data list 
    public List<User> Users { get; set; }
    public List<Appointments>  Appointment { get; set; }

    //private constructor

    private DataService()
    {
        Users = new List<User>();
        Appointment = new List<Appointments>();

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
            Password = "admin", // later to  ****
            Role = UserRole.Admin
        });
    }
}

    
