using System.Linq;
using System.Windows;
using System.Windows.Controls;
using RegentHealth.Models;
using RegentHealth.Services;

namespace RegentHealth.Views
{
    public partial class RegisterPage : Page
    {
        private readonly AuthService _authService;

        public RegisterPage()
        {
            InitializeComponent();
            _authService = DataService.Instance.AuthService;
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            string password = PasswordBox.Password;

            // password: min 6 chars, at least one digit
            if (password.Length < 6 || !password.Any(char.IsDigit))
            {
                MessageBox.Show("Password must be at least 6 characters and contain at least one number.",
                    "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            User user = _authService.Register(
                NameTextBox.Text,
                SurnameTextBox.Text,
                EmailTextBox.Text,
                password
            );

            if (user == null)
            {
                MessageBox.Show("Invalid email or user already exists");
                return;
            }


            if (Application.Current.MainWindow is MainWindow main)
            {
                main.MainFrame.Navigate(new DashboardPage(_authService));
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (Application.Current.MainWindow is MainWindow main)
            {
                main.MainFrame.Navigate(new LoginPage());
            }
        }
    }
}