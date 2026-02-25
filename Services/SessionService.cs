using System;
using System.Collections.Generic;
using System.Text;

namespace RegentHealth.Services
{
    public class SessionService
    {
        private static SessionService _instance;

        public static SessionService Instance =>
            _instance ??= new SessionService();

        private SessionService() { }

        public User? CurrentUser { get; private set; }

        public void Login(User user)
        {
            CurrentUser = user;
        }

        public void Logout()
        {
            CurrentUser = null;
        }

        public bool IsLoggedIn => CurrentUser != null;
    }
}