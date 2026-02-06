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

    public bool Register(User newUser)
    {
        bool userExists = _dataService.Users.Any(u => u.Email == newUser.Email);

        if (userExists)
            return false;

        _dataService.Users.Add(newUser);
        return true;
    }
}

