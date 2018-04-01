using System.Windows;

namespace ContosoCheckIn
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            DispatcherUnhandledException += new System.Windows.Threading.DispatcherUnhandledExceptionEventHandler((s, e) =>
            {
                string message = e.Exception.ToString();

                MessageBox.Show(message);

                e.Handled = true;
            });
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {

        }
    }
}
