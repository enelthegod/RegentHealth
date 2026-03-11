using System.Windows;
using System.Windows.Controls;
using RegentHealth.Services;
using RegentHealth.Models;
using System.Linq;
using System.Windows.Media;

namespace RegentHealth.Views
{
    public partial class DoctorPage : Page
    {
        private readonly AppointmentService _appointmentService;
        private readonly AuthService _authService;

        public DoctorPage(
            AppointmentService appointmentService,
            AuthService authService)
        {
            InitializeComponent();

            _appointmentService = appointmentService;
            _authService = authService;

            DataContext = new ViewModels.DoctorViewModel(_appointmentService);
            UpdateShiftUI();
        }

        private void CompleteAppointment_Click(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as ViewModels.DoctorViewModel;

            if (vm?.SelectedAppointment == null)
            {
                MessageBox.Show("Select appointment first");
                return;
            }

            vm.CompleteAppointment();
        }


        private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (Application.Current.MainWindow is MainWindow main)
            {
                main.MainFrame.Navigate(new DashboardPage());
            }
        }



        private void ToggleShift_Click(object sender, RoutedEventArgs e)
        {
            var doctor = DataService.Instance.Doctors
                .FirstOrDefault(d => d.UserId == _authService.CurrentUser.Id);

            if (doctor == null)
                return;

            doctor.IsOnShift = !doctor.IsOnShift;

            UpdateShiftUI();
        }


        private void UpdateShiftUI()
        {
            var doctor = DataService.Instance.Doctors
                .FirstOrDefault(d => d.UserId == _authService.CurrentUser.Id);

            if (doctor == null)
                return;

            if (doctor.IsOnShift)
            {
                ShiftStatusText.Text = "ON SHIFT";
                ShiftStatusText.Foreground = Brushes.Green;

                ShiftButton.Content = "End Shift";
            }
            else
            {
                ShiftStatusText.Text = "OFFLINE";
                ShiftStatusText.Foreground = Brushes.Red;

                ShiftButton.Content = "Start Shift";
            }
        }
    }
}
