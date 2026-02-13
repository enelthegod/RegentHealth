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
using System.Windows.Shapes;

namespace RegentHealth
{
    public partial class DashboardWindow : Window
    {
        private readonly User _currentUser;
        

        public DashboardWindow(User user)
        {
            InitializeComponent();       
            _currentUser = user;

            WelcomeText.Text = $"Welcome, {_currentUser.FullName}";
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();
            Close();
        }
    }
}