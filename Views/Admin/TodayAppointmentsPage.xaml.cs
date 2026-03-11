using RegentHealth.Models;
using RegentHealth.Services;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

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

            // Get all appointments for today
            var appointments =
                DataService.Instance.Appointments
                .Where(a => a.AppointmentDate.Date == today)
                .OrderBy(a => a.AppointmentDate)
                .ToList();

            // Create a collection view with grouping by DoctorFullName
            var view = new ListCollectionView(appointments);
            view.GroupDescriptions.Add(new PropertyGroupDescription("DoctorFullName"));

            AppointmentsList.ItemsSource = view;
        }

        private void Appointment_DoubleClick(object sender, MouseButtonEventArgs e)
        {
 
            var listViewItem = sender as ListViewItem;
            if (listViewItem == null) return;

            // Get the Appointment from the DataContext
            var appointment = listViewItem.DataContext as Appointment;
            if (appointment == null) return;

            // Show appointment details 
            MessageBox.Show(
                $"Appointment ID: {appointment.Id}\n" +
                $"Type: {appointment.Type}\n" +
                $"Patient: {appointment.PatientFullName}\n" +
                $"Status: {appointment.Status}\n" +
                $"Time: {appointment.AppointmentDate:HH:mm}");
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new AdminPage());
        }
    }
}