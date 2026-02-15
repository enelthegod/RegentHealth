using System;
using System.Linq;
using RegentHealth.Helpers;

public class AuthService
{
    // naming convention for private case _xY
    private readonly DataService _dataService;

    public AuthService()
    {
        _dataService = DataService.Instance;
    }

    public User CurrentUser { get; private set; }

    public User Login(string email, string password)
    {
        string hashedPassword = PasswordHelper.HashPassword(password);

        var user = _dataService.Users
            .FirstOrDefault(u => u.Email == email &&
                                 u.PasswordHash == hashedPassword);

        if (user != null)
            CurrentUser = user;

        return user;
    }

    public void Logout()
    {
        CurrentUser = null;
    }

    public User Register(string name, string surname, string email, string password)
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

    private bool IsValidEmail(string email)
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

    public bool IsAdmin() =>
        CurrentUser != null && CurrentUser.Role == UserRole.Admin;

    public bool IsDoctor() =>
        CurrentUser != null && CurrentUser.Role == UserRole.Doctor;

    public bool IsPatient() =>
        CurrentUser != null && CurrentUser.Role == UserRole.Patient;

    public void RemoveDoctor(int doctorId)
    {
        if (!IsAdmin())
            throw new Exception("Access denied");

        var doctor = _dataService.Users
            .FirstOrDefault(u => u.Id == doctorId &&
                                 u.Role == UserRole.Doctor);

        if (doctor != null)
            _dataService.Users.Remove(doctor);
    }

    public bool RegisterDoctor(string name, string surname,
                               string email, string password)
    {
        if (!IsAdmin())
            throw new Exception("Only admin can create doctors.");

        bool exists = _dataService.Users.Any(u => u.Email == email);

        if (exists)
            return false;

        var doctor = new User
        {
            Id = _dataService.Users.Count + 1,
            Name = name,
            Surname = surname,
            Email = email,
            PasswordHash = PasswordHelper.HashPassword(password),
            Role = UserRole.Doctor
        };

        _dataService.Users.Add(doctor);

        return true;
    }
}
