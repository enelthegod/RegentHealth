using RegentHealth.Models;
using RegentHealth.Services;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace RegentHealth.Views.Admin
{
    public partial class TodayAppointmentsPage : Page
    {
        public TodayAppointmentsPage()
        {
            InitializeComponent();

            LoadAppointments();
        }

        private void LoadAppointments()
        {
            var today = DateTime.Today;

            var appointments =
                DataService.Instance.Appointments
                .Where(a => a.AppointmentDate.Date == today)
                .OrderBy(a => a.AppointmentDate)
                .ToList();

            AppointmentsList.ItemsSource = appointments;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var appointment = button?.DataContext as Appointment;

            if (appointment == null)
                return;

            appointment.Status = AppointmentStatus.Cancelled;

            LoadAppointments();
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new AdminPage());
        }
    }
}
