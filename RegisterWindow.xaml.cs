using System.Windows;

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

            DashboardWindow dashboardWindow = new DashboardWindow(user);
            dashboardWindow.Show();
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
