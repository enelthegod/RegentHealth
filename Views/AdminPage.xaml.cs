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
        }
    }
}