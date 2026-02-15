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

            // already exists AuthService
            _authService = authService;

            // create AppointmentService with same AuthService
            _appointmentService = new AppointmentService(
                DataService.Instance,
                _authService);
        }

        private void AppointmentsButton_Click(object sender, RoutedEventArgs e)        // open the new window with appointments PatientWindow
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
