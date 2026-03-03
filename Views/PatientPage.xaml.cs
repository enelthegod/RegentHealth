using RegentHealth.Enums;
using RegentHealth.Helpers;
using RegentHealth.Models;
using RegentHealth.Services;
using RegentHealth.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;

namespace RegentHealth.Views
{
    public partial class PatientPage : Page
    {
        private readonly AppointmentService _appointmentService;
        private readonly AuthService _authService;
        private readonly AppointmentsViewModel _viewModel;

        // constructor with 2 services
        public PatientPage(
            AppointmentService appointmentService = null,
            AuthService authService = null)
        {
            InitializeComponent();

            //appointments 14days lock
            AppointmentDatePicker.DisplayDateStart = DateTime.Today;
            AppointmentDatePicker.DisplayDateEnd = DateTime.Today.AddDays(14);

            // if services not given , take from dataservice
            _authService = authService ?? DataService.Instance.AuthService;
            _appointmentService = appointmentService ?? new AppointmentService(DataService.Instance, _authService);

            _viewModel = new AppointmentsViewModel(_appointmentService);
            DataContext = _viewModel;

            AppointmentTypeComboBox.ItemsSource =
                Enum.GetValues(typeof(AppointmentType));

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

                // straight like DateTime
                DateTime selectedDateTime = (DateTime)TimeSlotComboBox.SelectedItem;

                AppointmentType type =
                    (AppointmentType)AppointmentTypeComboBox.SelectedItem;

                // already exists DateTime
                _viewModel.CreateAppointment(selectedDateTime, type);

                MessageBox.Show("Appointment created!");

                // обновляем список слотов после создания
                AppointmentDatePicker_SelectedDateChanged(null, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        // CANCEL APPOINTMENT
        private void CancelAppointment_Click(object sender, RoutedEventArgs e)
        {
            var selectedAppointment = AppointmentsListBox.SelectedItem as Appointment;

            if (selectedAppointment == null)
            {
                MessageBox.Show("Please select an appointment to cancel.");
                return;
            }

            var result = MessageBox.Show(
                $"Cancel appointment on {selectedAppointment.AppointmentDate:dd MMM yyyy} at {selectedAppointment.AppointmentDate:HH:mm}?",
                "Confirm cancellation",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

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
            if (Application.Current.MainWindow is MainWindow main)
            {
                // give authservice back to dashboardpage can know current user
                main.MainFrame.Navigate(new DashboardPage(_authService));
            }
        }

        private void AppointmentDatePicker_Loaded(object sender, RoutedEventArgs e)
        {
            var picker = sender as DatePicker;

            DateTime today = DateTime.Today;
            DateTime maxDate = today.AddDays(14);

            // full calendar
            picker.DisplayDateStart = today.AddYears(-1);
            picker.DisplayDateEnd = today.AddYears(1);

            // block past 
            picker.BlackoutDates.Add(
                new CalendarDateRange(DateTime.MinValue, today.AddDays(-1)));

            // block after 14
            picker.BlackoutDates.Add(
                new CalendarDateRange(maxDate.AddDays(1), DateTime.MaxValue));
        }

        private void AppointmentDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AppointmentDatePicker.SelectedDate == null)
                return;

            DateTime selectedDate = AppointmentDatePicker.SelectedDate.Value.Date;

            List<DateTime> allSlots = new List<DateTime>();

            DateTime start = selectedDate.AddHours(9);
            DateTime end = selectedDate.AddHours(17);

            while (start < end)
            {
                allSlots.Add(start);
                start = start.AddMinutes(30);
            }

            var busySlots = DataService.Instance.Appointments
                .Where(a => a.AppointmentDate.Date == selectedDate &&
                            a.Status == AppointmentStatus.Scheduled)
                .Select(a => a.AppointmentDate.TimeOfDay);

            var freeSlots = allSlots
                .Where(slot => !busySlots.Contains(slot.TimeOfDay))
                .ToList();

            TimeSlotComboBox.ItemsSource = freeSlots;
        }


    }
}