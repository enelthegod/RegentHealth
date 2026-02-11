using System;
using System.Security.Cryptography;
using System.Text;

public class AuthService
{
    private string HashPassword(string password)
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
    // naming convention for private case _xY
    private readonly DataService _dataService;

    public AuthService()
    {
        _dataService = DataService.Instance;
    }

    public User Login(string email, string password) // changed with hash password
    {
        string hashedPassword = HashPassword(password);

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
            PasswordHash = HashPassword(password),
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


}

