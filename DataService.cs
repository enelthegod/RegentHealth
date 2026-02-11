using System;
using System.Security.Cryptography;
using System.Text;

public class DataService
{
    private string HashPassword(string password)                                 // same as in auth service *just for now* , then to password metod file
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            StringBuilder builder = new StringBuilder();

            foreach (byte b in bytes)
            {
                builder.Append(b.ToString("x2"));
            }

            return builder.ToString();
        }
    }


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
            PasswordHash = HashPassword("admin"),
            Role = UserRole.Admin
        });
    }
}

    
