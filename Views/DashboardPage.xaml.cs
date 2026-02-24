using System.Windows;
using System.Windows.Controls;
using RegentHealth.Services;

namespace RegentHealth.Views
{
    public partial class DashboardPage : Page
    {
        private readonly AuthService _authService;
        private readonly AppointmentService _appointmentService;

        public DashboardPage(AuthService authService)
        {
            InitializeComponent();

            _authService = authService;

            _appointmentService = new AppointmentService(
                DataService.Instance,
                _authService);

            LoadWelcomeText();
        }

        private void LoadWelcomeText()
        {
            var user = _authService.CurrentUser;

            if (user != null)
            {
                WelcomeTextBlock.Text =
                    $"Welcome, {user.Name} {user.Surname}";
            }
        }

        private void AppointmentsButton_Click(object sender, RoutedEventArgs e)
        {

            if (Application.Current.MainWindow is MainWindow main)
            {
                main.MainFrame.Navigate(new PatientPage(_appointmentService, _authService));
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
    }
}