using System.Windows;

namespace ArtemisWest.Mayfair.Shell
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            // The boostrapper will create the Shell instance, so the App.xaml does not have a StartupUri.
            var bootstrapper = new Bootstrapper();
            bootstrapper.Run();
        }
    }
}
