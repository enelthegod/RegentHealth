using System;

public class AuthService
{
    // naming convention for private case
    private readonly DataService _dataService;

    public AuthService()
    {
        _dataService = DataService.Instance;
    }

    public User Login(string email, string password)
    {
        // if no found go null
        var user = _dataService.Users.FirstOrDefault(u => u.Email == email && u.Password == password);
        return user;
    }
    public User CurrentUser { get; private set; }
    public User Register(string name, string surname, string email, string password)
    {
        if (_dataService.Users.Any(u => u.Email == email))
            return null;

        User user = new User
        {
            Name = name,
            Surname = surname,
            Email = email,
            Password = password,
            Role = UserRole.Patient
        };

        _dataService.Users.Add(user);
        CurrentUser = user;

        return user;
    }



}

