using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RegentHealth
{
    public partial class LoginWindow : Window
    {
        private readonly AuthService _authService = new AuthService();

        public LoginWindow()
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
            if (_authService.IsAdmin())                         // Temporary tested - then will be delete
            {
                _authService.RegisterDoctor(
                    "John",
                    "Smith",
                    "doctor@test.com",
                    "123"
                );
            }

            DashboardWindow dashboardWindow = new DashboardWindow(_authService);
            dashboardWindow.Show();
            Close();
        }
        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            RegisterWindow registerWindow = new RegisterWindow();
            registerWindow.Show();
            Close();
        }
    }
}