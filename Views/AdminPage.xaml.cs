using RegentHealth.Services;
using System.Windows;
using System.Windows.Controls;

namespace RegentHealth.Views
{
    public partial class AdminPage : Page
    {
        private readonly AuthService _authService;

        public AdminPage(AuthService authService = null)
        {
            InitializeComponent();

            _authService = authService ?? DataService.Instance.AuthService;
        }

        private void CreateDoctor_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(
                new RegentHealth.Views.Admin.CreateDoctorPage());
        }

        private void Rotation_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(
                new RegentHealth.Views.Admin.RotationPage());
        }

        private void Doctors_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(
                new RegentHealth.Views.Admin.DoctorsListPage());
        }

        private void Appointments_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(
                new RegentHealth.Views.Admin.TodayAppointmentsPage());
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (Application.Current.MainWindow is MainWindow main)
            {
                main.MainFrame.Navigate(new DashboardPage());
            }
        }
    }
}