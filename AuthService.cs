using System;
using System.Security.Cryptography;
using System.Text;
using RegentHealth.Helpers;
public class AuthService
{
    // naming convention for private case _xY
    private readonly DataService _dataService;

    public AuthService()
    {
        _dataService = DataService.Instance;
    }

    public User Login(string email, string password) // changed with hash password
    {
        string hashedPassword = PasswordHelper.HashPassword(password);

        var user = _dataService.Users
            .FirstOrDefault(u => u.Email == email && u.PasswordHash == hashedPassword);

        if (user != null)
        {
            CurrentUser = user;
        }

        return user;
    }
    public User CurrentUser { get; private set; }
    public User Register(string name, string surname, string email, string password) // changed with hash password
    {
        if (!IsValidEmail(email))
            return null;

        if (_dataService.Users.Any(u => u.Email == email))
            return null;

        User user = new User
        {
            Name = name,
            Surname = surname,
            Email = email,
            PasswordHash = PasswordHelper.HashPassword(password),
            Role = UserRole.Patient
        };

        _dataService.Users.Add(user);
        CurrentUser = user;

        return user;
    }

    private bool IsValidEmail(string email) //validation for email
    {
        try
        {
            var addv = new System.Net.Mail.MailAddress(email);
            return addv.Address == email;
        }
        catch
        {
            return false;
        }
    }

    public bool IsAdmin()                                                 // checking role 
    {
        return CurrentUser != null && CurrentUser.Role == UserRole.Admin;
    }

    public bool IsDoctor()
    {
        return CurrentUser != null && CurrentUser.Role == UserRole.Doctor;
    }

    public bool IsPatient()
    {
        return CurrentUser != null && CurrentUser.Role == UserRole.Patient;
    }

    public void RemoveDoctor(int doctorId)                                         // role-based access control
    {
        if (!IsAdmin())
            throw new Exception("Access denied");

        var doctor = _dataService.Users
            .FirstOrDefault(u => u.Id == doctorId && u.Role == UserRole.Doctor);

        if (doctor != null)
            _dataService.Users.Remove(doctor);
    }


}

