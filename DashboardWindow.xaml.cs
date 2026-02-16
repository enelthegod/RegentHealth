using System.Windows;
using RegentHealth.Services;

namespace RegentHealth
{
    public partial class DashboardWindow : Window
    {
        private readonly AuthService _authService;
        private readonly AppointmentService _appointmentService;

        public DashboardWindow(AuthService authService)
        {
            InitializeComponent();

            // use already exists AuthService
            _authService = authService;

            // Make AppointmentService with same AuthService
            _appointmentService = new AppointmentService(
                DataService.Instance,
                _authService);

            LoadWelcomeText();
        }

        private void LoadWelcomeText()
        {
            var user = _authService.CurrentUser;

            if (user != null)
            {
                WelcomeTextBlock.Text =
                    $"Welcome, {user.Name} {user.Surname}";
            }
        }

        private void AppointmentsButton_Click(object sender, RoutedEventArgs e)
        {
            PatientWindow window =
                new PatientWindow(_appointmentService, _authService);

            window.Show();
            Close();
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            _authService.Logout();

            LoginWindow login = new LoginWindow();
            login.Show();

            Close();
        }
    }
}