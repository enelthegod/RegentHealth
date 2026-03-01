using System.Windows;
using System.Windows.Controls;
using RegentHealth.Services;

namespace RegentHealth.Views
{
    public partial class DashboardPage : Page
    {
        private readonly AuthService _authService;
        private readonly AppointmentService _appointmentService;

        // add constructor with authservice
        public DashboardPage(AuthService authService = null)
        {
            InitializeComponent();



            // if authservice done - take it , if no - take from dataservice 
            _authService = authService ?? DataService.Instance.AuthService;

            _appointmentService = new AppointmentService(
                DataService.Instance,
                _authService);

            LoadWelcomeText();
            ApplyRoleUI();
        }

        private void LoadWelcomeText()
        {
            var user = _authService.CurrentUser;

            if (user != null)
            {
                WelcomeTextBlock.Text =
                    $"Welcome, {user.FullName}";
            }
        }

        private void AppointmentsButton_Click(object sender, RoutedEventArgs e)
        {
            if (Application.Current.MainWindow is MainWindow main)
            {
                if (_authService.IsDoctor())
                {
                    main.MainFrame.Navigate(
                        new DoctorPage(_appointmentService, _authService));
                }
                else if (_authService.IsAdmin())
                {
                    main.MainFrame.Navigate(
                        new AdminPage(_appointmentService, _authService));
                }
                else
                {
                    main.MainFrame.Navigate(
                        new PatientPage(_appointmentService, _authService));
                }
            }
        }



        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            _authService.Logout();

            if (Application.Current.MainWindow is MainWindow main)
            {
                main.MainFrame.Navigate(new LoginPage());
            }
        }

        /// 
        /// ROLES CASE
        /// /////////////////////////////////////////////////////
        private void ApplyRoleUI()
        {
            var user = _authService.CurrentUser;

            // defualt - hide 
            AppointmentsButton.Visibility = Visibility.Collapsed;


            if (user.Role == UserRole.Patient)
            {
                AppointmentsButton.Visibility = Visibility.Visible;
                AppointmentsButton.Content = "My Appointments";
            }

            if (user.Role == UserRole.Doctor)
            {
                AppointmentsButton.Visibility = Visibility.Visible;
                AppointmentsButton.Content = "Appointments List";
            }

            if (user.Role == UserRole.Admin)
            {
                AppointmentsButton.Visibility = Visibility.Visible;
                AppointmentsButton.Content = "Admin Dashboard";
            }


        }


    }

}