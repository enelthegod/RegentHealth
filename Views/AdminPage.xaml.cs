using System.Windows.Controls;
using RegentHealth.Services;

namespace RegentHealth.Views
{
    public partial class AdminPage : Page
    {
        private readonly AppointmentService _appointmentService;
        private readonly AuthService _authService;

        public AdminPage(
            AppointmentService appointmentService,
            AuthService authService)
        {
            InitializeComponent();

            _appointmentService = appointmentService;
            _authService = authService;
        }
    }
}
