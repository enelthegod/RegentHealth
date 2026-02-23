using RegentHealth.Enums;
using RegentHealth.Helpers;
using RegentHealth.Models;
using RegentHealth.Services;
using RegentHealth.ViewModels;
using System;
using System.Windows;

namespace RegentHealth
{
    public partial class PatientWindow : Window
    {
        private readonly AppointmentService _appointmentService;
        private readonly AuthService _authService;
        private readonly AppointmentsViewModel _viewModel;

        public PatientWindow(
            AppointmentService appointmentService,
            AuthService authService)
        {
            InitializeComponent();

            _appointmentService = appointmentService;
            _authService = authService;

            _viewModel = new AppointmentsViewModel(_appointmentService);

            DataContext = _viewModel;

            AppointmentTypeComboBox.ItemsSource =
                Enum.GetValues(typeof(AppointmentType));

            TimeSlotComboBox.ItemsSource =
                EnumDisplayHelper.GetTimeSlots();
        }

        // CREATE APPOINTMENT
        private void CreateAppointment_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (AppointmentDatePicker.SelectedDate == null)
                {
                    MessageBox.Show("Please select a date");
                    return;
                }

                if (AppointmentTypeComboBox.SelectedItem == null)
                {
                    MessageBox.Show("Select appointment type");
                    return;
                }

                if (TimeSlotComboBox.SelectedItem == null)
                {
                    MessageBox.Show("Select time slot");
                    return;
                }

                DateTime date = AppointmentDatePicker.SelectedDate.Value;

                AppointmentType type =
                    (AppointmentType)AppointmentTypeComboBox.SelectedItem;

                // convert "10:00" -> TimeSlot enum
                string selectedTime =
                    TimeSlotComboBox.SelectedItem.ToString();

                string enumName =
                    "Slot" + selectedTime.Replace(":", "_");

                TimeSlot slot =
                    (TimeSlot)Enum.Parse(typeof(TimeSlot), enumName);

                //  create via ViewModel
                _viewModel.CreateAppointment(date, slot, type);

                MessageBox.Show("Appointment created!");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        // CANCEL APPOINTMENT
        private void CancelAppointment_Click(object sender, RoutedEventArgs e)
        {
            var selectedAppointment =
                AppointmentsListBox.SelectedItem as Appointment;

            if (selectedAppointment == null)
            {
                MessageBox.Show("Please select an appointment to cancel.");
                return;
            }

            var result = MessageBox.Show(
                $"Cancel appointment on {selectedAppointment.AppointmentDate:dd MMM yyyy} at {selectedAppointment.DisplayTime}?",
                "Confirm cancellation",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                _viewModel.CancelAppointment();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        // BACK TO DASHBOARD
        private void Back_Click(object sender, RoutedEventArgs e)
        {
            DashboardWindow dashboard =
                new DashboardWindow(_authService);

            dashboard.Show();
            Close();
        }

    }
}