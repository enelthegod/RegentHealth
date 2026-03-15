using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using RegentHealth.Services;
using RegentHealth.Models;
using System.Linq;

namespace RegentHealth.Views
{
    public partial class DoctorPage : Page
    {
        private readonly AppointmentService _appointmentService;
        private readonly AuthService _authService;

        // Timer refreshes shift status every minute automatically
        private readonly DispatcherTimer _shiftTimer;

        public DoctorPage(
            AppointmentService appointmentService,
            AuthService authService)
        {
            InitializeComponent();

            _appointmentService = appointmentService;
            _authService = authService;

            DataContext = new ViewModels.DoctorViewModel(_appointmentService);

            // Update shift status now
            UpdateShiftUI();

            // Then refresh every 60 seconds so it switches automatically
            _shiftTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(60)
            };
            _shiftTimer.Tick += (s, e) => UpdateShiftUI();
            _shiftTimer.Start();

            // Stop timer when page is unloaded (doctor navigated away)
            this.Unloaded += (s, e) => _shiftTimer.Stop();
        }

        private void CompleteAppointment_Click(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as ViewModels.DoctorViewModel;

            if (vm?.SelectedAppointment == null)
            {
                MessageBox.Show("Select appointment first.");
                return;
            }

            vm.CompleteAppointment();
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            _shiftTimer.Stop();
            if (Application.Current.MainWindow is MainWindow main)
                main.MainFrame.Navigate(new DashboardPage());
        }

        // ── Auto shift based on WorkStart / WorkEnd ──────────────────
        private void UpdateShiftUI()
        {
            var doctor = DataService.Instance.Doctors
                .FirstOrDefault(d => d.UserId == _authService.CurrentUser.Id);

            if (doctor == null) return;

            TimeSpan now = DateTime.Now.TimeOfDay;

            bool isWorkingHours =
                now >= doctor.WorkStart &&
                now < doctor.WorkEnd;

            // Keep the flag in sync so scheduler can find the doctor
            doctor.IsOnShift = isWorkingHours;

            if (isWorkingHours)
            {
                ShiftStatusText.Text = "ON SHIFT";
                ShiftStatusText.Foreground = Brushes.Green;
                ShiftButton.Content = $"Working until {doctor.WorkEnd:hh\\:mm}";
                ShiftButton.IsEnabled = false;
            }
            else
            {
                ShiftStatusText.Text = "OFF SHIFT";
                ShiftStatusText.Foreground = Brushes.Red;
                ShiftButton.Content = $"Shift starts {doctor.WorkStart:hh\\:mm}";
                ShiftButton.IsEnabled = false;
            }
        }
    }
}