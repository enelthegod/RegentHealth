using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RegentHealth.Views.Admin
{

    public partial class CreateDoctorPage : Page
    {
        public CreateDoctorPage()
        {
            InitializeComponent();

        }
        private void Create_Click(object sender, RoutedEventArgs e)
        {
            DataService.Instance.AuthService.RegisterDoctor(
                NameBox.Text,
                SurnameBox.Text,
                EmailBox.Text,
                PasswordBox.Password);

            MessageBox.Show("Doctor created");
        }
    }


}
