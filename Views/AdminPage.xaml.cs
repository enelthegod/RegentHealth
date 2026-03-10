using RegentHealth.Models;
using RegentHealth.Services;
using System;
using System.Windows;
using System.Windows.Controls;

namespace RegentHealth.Views
{
    public partial class AdminPage : Page
    {
        private readonly AppointmentService _appointmentService;
        private readonly AuthService _authService;

        public AdminPage(
            AppointmentService appointmentService = null,
            AuthService authService = null)
        {
            InitializeComponent();

            _authService = authService ?? DataService.Instance.AuthService;                 // same as on PatientPage 
            _appointmentService = appointmentService
                ?? new AppointmentService(DataService.Instance, _authService);

            LoadDoctors();
        }

        private void LoadDoctors()
        {
            DoctorsList.ItemsSource = null;
            DoctorsList.ItemsSource =
                DataService.Instance.AdminService.GetDoctors();
        }



        private void CreateDoctor_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataService.Instance.AdminService.CreateDoctor(
                    NameBox.Text,
                    SurnameBox.Text,
                    EmailBox.Text,
                    PasswordBox.Password
                );

                MessageBox.Show("Doctor created!");

                NameBox.Text = "";
                SurnameBox.Text = "";
                EmailBox.Text = "";
                PasswordBox.Password = "";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            LoadDoctors();
        }




        private void DoctorActive_Click(object sender, RoutedEventArgs e)
        {
            var check = sender as CheckBox;
            var doctor = check?.DataContext as Doctor;

            if (doctor == null)
                return;

            doctor.IsActive = check.IsChecked == true;

            LoadDoctors();
        }



        private void Emergency_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var text = sender as TextBlock;

            var doctor = text?.DataContext as Doctor;

            if (doctor == null)
                return;

            DataService.Instance.AdminService.SetEmergencyDoctor(doctor.UserId);

            LoadDoctors();
        }



        private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (Application.Current.MainWindow is MainWindow main)
            {
                main.MainFrame.Navigate(
                    new DashboardPage());
            }
        }
    }
}