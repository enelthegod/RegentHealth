using System.Windows;
using RegentHealth.Services;
using RegentHealth.Enums;

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

        private void CreateAppointment_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!int.TryParse(DoctorIdTextBox.Text, out int doctorId))
                {
                    MessageBox.Show("Invalid Doctor Id");
                    return;
                }

                if (AppointmentDatePicker.SelectedDate == null)
                {
                    MessageBox.Show("Please select a date");
                    return;
                }

                DateTime date = AppointmentDatePicker.SelectedDate.Value;

                                                                         // TEMP values (until UI added)
                TimeSpan time = new TimeSpan(10, 0, 0);                  // 10:00 AM
                AppointmentType type = AppointmentType.Consultation;

                _appointmentService.CreateAppointment(
                    doctorId,
                    date,
                    time,
                    type);

                MessageBox.Show("Appointment created!");

                LoadAppointments();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
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
