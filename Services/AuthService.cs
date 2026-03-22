using RegentHealth.Data;
using RegentHealth.Helpers;
using RegentHealth.Models;
using System;
using System.Linq;

public class AuthService
{
    private readonly DataService _dataService;

    public AuthService(DataService dataService)
    {
        _dataService = dataService;
    }

    public User CurrentUser { get; private set; }

    public User Login(string email, string password)
    {
        string hashedPassword = PasswordHelper.HashPassword(password);

        var user = _dataService.Users
            .FirstOrDefault(u => u.Email == email &&
                                 u.PasswordHash == hashedPassword);

        if (user != null)
        {
            CurrentUser = user;

            if (user.Role == UserRole.Doctor)
            {
                var doctor = _dataService.Doctors
                    .FirstOrDefault(d => d.UserId == user.Id);

                if (doctor != null)
                {
                    doctor.LastLogin = DateTime.Now;
                    // Save updated LastLogin to DB
                    _dataService.SaveDoctors();
                }
            }
        }

        return user;
    }

    public void Logout()
    {
        CurrentUser = null;
    }

    // Patient registration
    public User Register(string name, string surname, string email, string password)
    {
        if (!IsValidEmail(email))
            return null;

        if (_dataService.Users.Any(u => u.Email == email))
            return null;

        var user = new User
        {
            Name = name,
            Surname = surname,
            Email = email,
            PasswordHash = PasswordHelper.HashPassword(password),
            Role = UserRole.Patient
        };

        // 1. Save to DB first so SQLite assigns the Id
        using (var db = new AppDbContext())
        {
            db.Users.Add(user);
            db.SaveChanges(); // after this line user.Id is set by SQLite
        }

        // 2. Add to in-memory list
        _dataService.Users.Add(user);
        CurrentUser = user;

        return user;
    }

    // Admin creates a doctor
    public bool RegisterDoctor(string name, string surname,
                               string email, string password)
    {
        if (!IsAdmin())
            throw new Exception("Only admin can create doctors.");

        if (_dataService.Users.Any(u => u.Email == email))
            return false;

        var newUser = new User
        {
            Name = name,
            Surname = surname,
            Email = email,
            PasswordHash = PasswordHelper.HashPassword(password),
            Role = UserRole.Doctor
        };

        var newDoctor = new Doctor
        {
            IsActive = true,
            IsOnShift = false,
            IsEmergencyDoctor = false,
            WorkStart = new TimeSpan(9, 0, 0),
            WorkEnd = new TimeSpan(17, 0, 0),
            WorkingDays = new List<DayOfWeek>
            {
                DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday,
                DayOfWeek.Thursday, DayOfWeek.Friday
            }
        };

        // Save both User and Doctor in one transaction
        // Transaction = "либо всё сохранилось, либо ничего"
        using (var db = new AppDbContext())
        {
            db.Users.Add(newUser);
            db.SaveChanges(); // SQLite assigns newUser.Id

            newDoctor.UserId = newUser.Id; // link Doctor to User
            newDoctor.User = newUser;    // set nav property for FullName
            db.Doctors.Add(newDoctor);
            db.SaveChanges();
        }

        // Add to in-memory lists so UI updates immediately
        _dataService.Users.Add(newUser);
        _dataService.Doctors.Add(newDoctor);

        return true;
    }

    public void RemoveDoctor(int doctorId)
    {
        if (!IsAdmin())
            throw new Exception("Access denied");

        var user = _dataService.Users
            .FirstOrDefault(u => u.Id == doctorId && u.Role == UserRole.Doctor);

        if (user == null) return;

        // Remove from DB
        using (var db = new AppDbContext())
        {
            var dbUser = db.Users.Find(doctorId);
            if (dbUser != null)
            {
                db.Users.Remove(dbUser);
                db.SaveChanges();
            }
        }

        // Remove from in-memory list
        _dataService.Users.Remove(user);
    }

    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch { return false; }
    }

    public bool IsAdmin() => CurrentUser?.Role == UserRole.Admin;
    public bool IsDoctor() => CurrentUser?.Role == UserRole.Doctor;
    public bool IsPatient() => CurrentUser?.Role == UserRole.Patient;
}
