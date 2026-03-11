using RegentHealth.Models;
using RegentHealth.Services;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace RegentHealth.Views.Admin
{
    public partial class DoctorDetailsPage : Page
    {
        private Doctor _doctor;

        public DoctorDetailsPage(Doctor doctor)
        {
            InitializeComponent();

            _doctor = doctor;

            LoadDoctor();
        }

        private void LoadDoctor()
        {
            DoctorNameText.Text = _doctor.FullName;

            ShiftStatusText.Text =
                _doctor.IsOnShift ? "ON SHIFT" : "OFFLINE";

            EmergencyStatusText.Text =
                _doctor.IsEmergencyDoctor ? "YES" : "NO";

            var appointments =
                DataService.Instance.Appointments
                .Where(a => a.DoctorId == _doctor.UserId)
                .OrderByDescending(a => a.AppointmentDate);

            AppointmentsList.ItemsSource = appointments;
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new DoctorsListPage());
        }
    }
}