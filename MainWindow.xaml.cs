using System.Windows;
using RegentHealth.Views;

namespace RegentHealth
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            MainFrame.Navigate(new LoginPage());
        }
    }
}
