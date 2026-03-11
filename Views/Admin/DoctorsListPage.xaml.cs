using RegentHealth.Models;
using RegentHealth.Services;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace RegentHealth.Views.Admin
{
    public partial class DoctorsListPage : Page
    {
        public DoctorsListPage()
        {
            InitializeComponent();
            LoadDoctors();
        }

        private void LoadDoctors()
        {
            DoctorsList.ItemsSource =
                DataService.Instance.Doctors;
        }

        private void DeleteDoctor_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;

            var doctor = button?.DataContext as Doctor;

            if (doctor == null)
                return;

            var confirm = MessageBox.Show(
                $"Delete {doctor.FullName} ?",
                "Confirm",
                MessageBoxButton.YesNo);

            if (confirm != MessageBoxResult.Yes)
                return;

            DataService.Instance.Doctors.Remove(doctor);

            LoadDoctors();
        }



        private void Doctor_Open(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var doctor = DoctorsList.SelectedItem as Doctor;

            if (doctor == null)
                return;

            NavigationService.Navigate(
                new DoctorDetailsPage(doctor));
        }




        private void CreateDoctor_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(
                new CreateDoctorPage());
        }




        private void Back_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(
                new RegentHealth.Views.AdminPage());
        }
    }
}