using System.Configuration;
using System.Data;
using System.Windows;

namespace AD00020_Control
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static Mutex _mutex;

        protected override void OnStartup(StartupEventArgs e)
        {
            _mutex = new Mutex(true, "AD00020_Control", out var createdNew);

            if (!createdNew)
            {
                MessageBox.Show(
                    "AD00020_Control is already running.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                Shutdown();
                return;
            }

            base.OnStartup(e);
        }
    }
}