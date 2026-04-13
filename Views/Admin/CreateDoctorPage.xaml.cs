using System.Linq;
using RegentHealth.Services;
using System;
using System.Net.Mail;
using System.Windows;
using System.Windows.Controls;

namespace RegentHealth.Views.Admin
{
    public partial class CreateDoctorPage : Page
    {

        public CreateDoctorPage()
        {
            InitializeComponent();
        }

        private void Create_Click(object sender, RoutedEventArgs e)
        {
            string name = NameBox.Text.Trim();
            string surname = SurnameBox.Text.Trim();
            string email = EmailBox.Text.Trim();
            string password = PasswordBox.Password.Trim();

            // Check if any field is empty
            if (string.IsNullOrWhiteSpace(name) ||
                string.IsNullOrWhiteSpace(surname) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Please fill in all fields.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // password: min 6 chars, at least one digit
            if (password.Length < 6 || !password.Any(char.IsDigit))
            {
                MessageBox.Show("Password must be at least 6 characters and contain at least one number.",
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Validate email format
            if (!IsValidEmail(email))
            {
                MessageBox.Show("Please enter a valid email address.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {

                DataService.Instance.AuthService.RegisterDoctor(name, surname, email, password);
                MessageBox.Show("Doctor created successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                // ✅ FIX: Navigate back so the list refreshes
                NavigationService.Navigate(new DoctorsListPage());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating doctor: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Simple email validation
        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

    }
}