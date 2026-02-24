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
            _authService = new AuthService();
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            User user = _authService.Register(
                NameTextBox.Text,
                SurnameTextBox.Text,
                EmailTextBox.Text,
                PasswordBox.Password
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