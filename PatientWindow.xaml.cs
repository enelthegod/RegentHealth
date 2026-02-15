using System.Windows;
using RegentHealth.Services;

namespace RegentHealth
{
    public partial class PatientWindow : Window
    {
        private readonly AppointmentService _appointmentService;
        private readonly AuthService _authService;

        public PatientWindow(
            AppointmentService appointmentService,
            AuthService authService)
        {
            InitializeComponent();

            _appointmentService = appointmentService;
            _authService = authService;

            LoadAppointments();
        }

        private void LoadAppointments()
        {
            var appointments =
                _appointmentService.GetAppointmentsForCurrentUser();

            AppointmentsListBox.ItemsSource = appointments;
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            LoadAppointments();
        }
    }
}
