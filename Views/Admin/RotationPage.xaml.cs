using RegentHealth.Models;
using RegentHealth.Services;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace RegentHealth.Views.Admin
{
    public partial class RotationPage : Page
    {
        public RotationPage()
        {
            InitializeComponent();
            LoadRotation();
        }

        private void LoadRotation()
        {
            var rotations =
                DataService.Instance.AdminService.GetWeeklyRotation();

            RotationList.ItemsSource = rotations
                .Select(r =>
                {
                    var doctor = DataService.Instance.Users
                        .FirstOrDefault(u => u.Id == r.DoctorId);

                    return $"{r.Day} - {doctor?.FullName}";
                });
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}