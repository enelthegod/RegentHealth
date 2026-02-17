using System;
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

            // fill appointment types from enum
            AppointmentTypeComboBox.ItemsSource =
                Enum.GetValues(typeof(AppointmentType));

            LoadAppointments();
        }

        private void CreateAppointment_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // check appointment type
                if (AppointmentTypeComboBox.SelectedItem == null)
                {
                    MessageBox.Show("Select appointment type");
                    return;
                }

                AppointmentType type =
                    (AppointmentType)AppointmentTypeComboBox.SelectedItem;

                // validate doctor id
                if (!int.TryParse(DoctorIdTextBox.Text, out int doctorId))
                {
                    MessageBox.Show("Invalid Doctor Id");
                    return;
                }

                // validate date
                if (AppointmentDatePicker.SelectedDate == null)
                {
                    MessageBox.Show("Please select a date");
                    return;
                }

                DateTime date =
                    AppointmentDatePicker.SelectedDate.Value;

                // TEMP time (next step → real time slots)
                TimeSpan time = new TimeSpan(10, 0, 0);

                _appointmentService.CreateAppointment(
                    doctorId,
                    date,
                    time,                                 // TEMP no change to timeSlot
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

