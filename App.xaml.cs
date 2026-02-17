using System.Configuration;
using System.Data;
using System.Windows;
using System.Globalization;
using System.Threading;

namespace RegentHealth
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            var culture = new CultureInfo("en-GB");

            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
        }
    }


}
