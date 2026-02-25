using System;
using System.Windows;
using System.Windows.Controls;
using RegentHealth.Services;
using RegentHealth.Models;

namespace RegentHealth.Views
{
    public partial class LoginPage : Page
    {
        // Используем DataService для получения AuthService
        private readonly AuthService _authService = DataService.Instance.AuthService;

        public LoginPage()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string email = LoginTextBox.Text;
            string password = PasswordBox.Password;

            var user = _authService.Login(email, password);

            if (user == null)
            {
                MessageBox.Show("Invalid email or password");
                return;
            }

            // save the session
            SessionService.Instance.Login(user);

            // TEMP doctor for tests 
            if (_authService.IsAdmin())
            {
                _authService.RegisterDoctor(
                    "John",
                    "Smith",
                    "doctor@test.com",
                    "123"
                );
            }

            if (Application.Current.MainWindow is MainWindow main)
            {
                main.MainFrame.Navigate(new DashboardPage());
            }
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            if (Application.Current.MainWindow is MainWindow main)
            {
                main.MainFrame.Navigate(new RegisterPage());
            }
        }
    }
}