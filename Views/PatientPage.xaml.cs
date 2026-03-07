using RegentHealth.Enums;
using RegentHealth.Helpers;
using RegentHealth.Models;
using RegentHealth.Services;
using RegentHealth.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;
using static System.Runtime.InteropServices.JavaScript.JSType;

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

                AppointmentType type =
                    (AppointmentType)AppointmentTypeComboBox.SelectedItem;

                DateTime selectedDate = AppointmentDatePicker.SelectedDate.Value;

                DateTime selectedDateTime;

                // EMERGENCY → no timeslot needed
                if (type == AppointmentType.Emergency)
                {
                    selectedDateTime = selectedDate.Date.Add(DateTime.Now.TimeOfDay);
                }
                else
                {
                    if (TimeSlotComboBox.SelectedItem == null)
                    {
                        MessageBox.Show("Select time slot");
                        return;
                    }

                    selectedDateTime =
                        (DateTime)TimeSlotComboBox.SelectedItem;
                }

                // weekend check
                DayOfWeek day = selectedDateTime.DayOfWeek;

                bool isWeekend =
                    day == DayOfWeek.Saturday ||
                    day == DayOfWeek.Sunday;

                if (isWeekend && type != AppointmentType.Emergency)
                {
                    MessageBox.Show("Only emergency appointments are available on weekends.");
                    return;
                }

                // lunch break rule
                TimeSpan time = selectedDateTime.TimeOfDay;

                TimeSpan lunchStart = new TimeSpan(12, 0, 0);
                TimeSpan lunchEnd = new TimeSpan(13, 0, 0);

                if (time >= lunchStart && time < lunchEnd)
                {
                    MessageBox.Show("Doctor is on lunch break from 12:00 to 13:00.");
                    return;
                }

                _viewModel.CreateAppointment(selectedDateTime, type);

                MessageBox.Show("Appointment created!");

                // refresh slots
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

        // DYNAMIC CHANGING
        private void AppointmentDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AppointmentDatePicker.SelectedDate == null)
                return;

            if (AppointmentTypeComboBox.SelectedItem == null)
                return;

            AppointmentType type =
                (AppointmentType)AppointmentTypeComboBox.SelectedItem;

            if (type == AppointmentType.Emergency)
            {
                TimeSlotComboBox.IsEnabled = false;
                TimeSlotComboBox.ItemsSource = null;
                return;
            }

            TimeSlotComboBox.IsEnabled = true;

            int interval = AppointmentRules.GetSlotInterval(type);

            DateTime selectedDate = AppointmentDatePicker.SelectedDate.Value.Date;

            List<DateTime> allSlots = new List<DateTime>();

            DateTime start = selectedDate.AddHours(9);
            DateTime end = selectedDate.AddHours(17);

            while (start < end)
            {
                if (!(start.Hour >= 12 && start.Hour < 13))
                {
                    allSlots.Add(start);
                }

                start = start.AddMinutes(interval);
            }

            var doctors = DataService.Instance.Doctors
                .Where(d => d.IsActive && !d.IsEmergencyDoctor)
                .ToList();

            int totalDoctors = doctors.Count;

            var freeSlots = allSlots.Where(slot =>
            {
                int busyDoctors = DataService.Instance.Appointments
                    .Where(a =>
                        a.Status == AppointmentStatus.Scheduled &&
                        a.AppointmentDate == slot)
                    .Select(a => a.DoctorId)
                    .Distinct()
                    .Count();

                return busyDoctors < totalDoctors;
            })
            .ToList();

            TimeSlotComboBox.ItemsSource = freeSlots;
        }

        private void AppointmentTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AppointmentDatePicker_SelectedDateChanged(null, null);
        }


    }
}