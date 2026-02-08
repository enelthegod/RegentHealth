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

        private void LoginButton_Click(object sender, RoutedEventArgs e) //way / e=event
        {
            string email = LoginTextBox.Text;
            string password = PasswordBox.Password;

            var user = _authService.Login(email, password);

            if (user == null)
            {
                MessageBox.Show("Invalid email or password");
                return;
            }

            switch (user.Role)
            {
                case UserRole.Admin:
                    MessageBox.Show("Logged in as Admin");
                    break;

                case UserRole.Doctor:
                    MessageBox.Show("Logged in as Doctor");
                    break;

                case UserRole.Patient:
                    MessageBox.Show("Logged in as Patient");
                    break;
            }
        }
    }
}