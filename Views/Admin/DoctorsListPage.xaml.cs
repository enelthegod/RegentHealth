using Microsoft.EntityFrameworkCore;
using RegentHealth.Data;
using RegentHealth.Models;
using RegentHealth.Services;
using System.Windows;
using System.Windows.Controls;

namespace RegentHealth.Views.Admin
{
    public partial class DoctorsListPage : Page
    {
        public DoctorsListPage()
        {
            InitializeComponent();
            this.Loaded += (s, e) => LoadDoctors();
        }

        private void LoadDoctors()
        {
            DoctorsList.ItemsSource = null;
            DoctorsList.ItemsSource = DataService.Instance.Doctors;
        }

        private void DeleteDoctor_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var doctor = button?.DataContext as Doctor;

            if (doctor == null) return;

            var confirm = MessageBox.Show(
                $"Delete {doctor.FullName}?",
                "Confirm",
                MessageBoxButton.YesNo);

            if (confirm != MessageBoxResult.Yes) return;

            // Delete from DB first
            // Doctor has Cascade delete so related User is deleted too
            using (var db = new AppDbContext())
            {
                // Find the Doctor in DB by its Id
                var dbDoctor = db.Doctors.Find(doctor.Id);

                if (dbDoctor != null)
                {
                    db.Doctors.Remove(dbDoctor);
                    db.SaveChanges();
                }
            }

            // Remove from in-memory list so UI updates immediately
            DataService.Instance.Doctors.Remove(doctor);

            // Also remove the User from in-memory Users list
            var user = DataService.Instance.Users
                .FirstOrDefault(u => u.Id == doctor.UserId);

            if (user != null)
                DataService.Instance.Users.Remove(user);

            LoadDoctors();
        }

        private void Doctor_Open(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var doctor = DoctorsList.SelectedItem as Doctor;
            if (doctor == null) return;
            NavigationService.Navigate(new DoctorDetailsPage(doctor));
        }

        private void CreateDoctor_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new CreateDoctorPage());
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService != null && NavigationService.CanGoBack)
                NavigationService.GoBack();
        }
    }
}