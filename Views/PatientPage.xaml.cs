using RegentHealth.Enums;
using RegentHealth.Helpers;
using RegentHealth.Models;
using RegentHealth.Services;
using RegentHealth.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace RegentHealth.Views
{
    public partial class PatientPage : Page
    {
        private readonly AppointmentService _appointmentService;
        private readonly AuthService _authService;
        private readonly AppointmentsViewModel _viewModel;

        public PatientPage(
            AppointmentService appointmentService = null,
            AuthService authService = null)
        {
            InitializeComponent();

            AppointmentDatePicker.DisplayDateStart = DateTime.Today;
            AppointmentDatePicker.DisplayDateEnd = DateTime.Today.AddDays(14);

            _authService = authService ?? DataService.Instance.AuthService;
            _appointmentService = appointmentService
                ?? new AppointmentService(DataService.Instance, _authService);

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
                    MessageBox.Show("Please select a date.");
                    return;
                }

                if (AppointmentTypeComboBox.SelectedItem == null)
                {
                    MessageBox.Show("Select appointment type.");
                    return;
                }

                AppointmentType type =
                    (AppointmentType)AppointmentTypeComboBox.SelectedItem;

                DateTime selectedDate = AppointmentDatePicker.SelectedDate.Value;
                DateTime selectedDateTime;

                if (type == AppointmentType.Emergency)
                {
                    selectedDateTime = selectedDate.Date.Add(DateTime.Now.TimeOfDay);
                }
                else
                {
                    if (TimeSlotComboBox.SelectedItem == null)
                    {
                        MessageBox.Show("Select a time slot.");
                        return;
                    }
                    selectedDateTime = (DateTime)TimeSlotComboBox.SelectedItem;
                }

                DayOfWeek day = selectedDateTime.DayOfWeek;
                bool isWeekend = day == DayOfWeek.Saturday || day == DayOfWeek.Sunday;

                if (isWeekend && type != AppointmentType.Emergency)
                {
                    MessageBox.Show("Only emergency appointments on weekends.");
                    return;
                }

                TimeSpan time = selectedDateTime.TimeOfDay;
                if (time >= new TimeSpan(12, 0, 0) && time < new TimeSpan(13, 0, 0))
                {
                    MessageBox.Show("Doctor is on lunch break 12:00–13:00.");
                    return;
                }

                _viewModel.CreateAppointment(selectedDateTime, type);
                MessageBox.Show("Appointment created!");
                RefreshSlots();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        // CANCEL APPOINTMENT
        private void CancelAppointment_Click(object sender, RoutedEventArgs e)
        {
            var selected = AppointmentsListBox.SelectedItem as Appointment;

            if (selected == null)
            {
                MessageBox.Show("Please select an appointment to cancel.");
                return;
            }

            var result = MessageBox.Show(
                $"Cancel appointment on {selected.AppointmentDate:dd MMM yyyy} at {selected.AppointmentDate:HH:mm}?",
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

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (Application.Current.MainWindow is MainWindow main)
                main.MainFrame.Navigate(new DashboardPage(_authService));
        }

        private void AppointmentDatePicker_Loaded(object sender, RoutedEventArgs e)
        {
            var picker = sender as DatePicker;
            DateTime today = DateTime.Today;
            DateTime maxDate = today.AddDays(14);

            picker.DisplayDateStart = today.AddYears(-1);
            picker.DisplayDateEnd = today.AddYears(1);

            picker.BlackoutDates.Add(
                new CalendarDateRange(DateTime.MinValue, today.AddDays(-1)));
            picker.BlackoutDates.Add(
                new CalendarDateRange(maxDate.AddDays(1), DateTime.MaxValue));
        }

        // Refresh slots when date or type changes
        private void AppointmentDatePicker_SelectedDateChanged(
            object sender, SelectionChangedEventArgs e) => RefreshSlots();

        private void AppointmentTypeComboBox_SelectionChanged(
            object sender, SelectionChangedEventArgs e) => RefreshSlots();

        private void RefreshSlots()
        {
            if (AppointmentDatePicker.SelectedDate == null) return;
            if (AppointmentTypeComboBox.SelectedItem == null) return;

            AppointmentType type =
                (AppointmentType)AppointmentTypeComboBox.SelectedItem;

            // Emergency — no timeslot needed
            if (type == AppointmentType.Emergency)
            {
                TimeSlotComboBox.IsEnabled = false;
                TimeSlotComboBox.ItemsSource = null;

                // Check if any emergency doctor is on shift
                bool hasEmgDoc = DataService.Instance.Doctors
                    .Any(d => d.IsActive && d.IsOnShift && d.IsEmergencyDoctor);

                EmergencyStatusText.Visibility = Visibility.Visible;
                EmergencyStatusText.Text = hasEmgDoc
                    ? "Emergency doctor is available."
                    : "No emergency doctor on shift. You will be queued.";
                return;
            }

            EmergencyStatusText.Visibility = Visibility.Collapsed;
            TimeSlotComboBox.IsEnabled = true;

            int interval = AppointmentRules.GetSlotInterval(type);
            DateTime selectedDate = AppointmentDatePicker.SelectedDate.Value.Date;

            var slots = new List<DateTime>();
            DateTime start = selectedDate.AddHours(9);
            DateTime end = selectedDate.AddHours(17);

            while (start < end)
            {
                // Skip lunch
                if (!(start.Hour == 12))
                {
                    // ✅ FIX: FindLeastBusyDoctor checks IsOnShift — set by Save Rotation
                    var doctor = DoctorScheduler.FindLeastBusyDoctor(start, type);
                    if (doctor != null)
                        slots.Add(start);
                }
                start = start.AddMinutes(interval);
            }

            TimeSlotComboBox.ItemsSource = slots;

            if (slots.Count == 0)
                MessageBox.Show(
                    "No available slots for this date.\n\n" +
                    "Make sure the admin has saved a rotation with doctors marked as Working.",
                    "No Slots", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}