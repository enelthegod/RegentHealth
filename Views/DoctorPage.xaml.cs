using System.Windows;
using System.Windows.Controls;
using RegentHealth.Services;
using RegentHealth.Models;

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
    }
}
