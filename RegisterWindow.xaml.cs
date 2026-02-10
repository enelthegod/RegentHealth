using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;


namespace RegentHealth
{
    public partial class RegisterWindow : Window
    {
        private readonly AuthService _authService;

        public RegisterWindow()
        {
            InitializeComponent();
            _authService = new AuthService();
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            string name = NameTextBox.Text;
            string surname = SurnameTextBox.Text;
            string email = EmailTextBox.Text;
            string password = PasswordBox.Password;

            User newUser = new User
            {
                Name = name,
                Surname = surname,
                Email = email,
                Password = password,
                Role = UserRole.Patient
            };

            bool success = _authService.Register(newUser);

            if (!success)
            {
                MessageBox.Show("User already exists");
                return;
            }

            MessageBox.Show("Registration successful");

            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();
            Close();
        }



        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();
            Close();
        }
    }
}

