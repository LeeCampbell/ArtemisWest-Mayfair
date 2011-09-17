using System.Windows;
using ArtemisWest.Mayfair.Infrastructure;
using ArtemisWest.PropertyInvestment.Calculator;
using Microsoft.Practices.Prism.UnityExtensions;
using Microsoft.Practices.Unity;

namespace ArtemisWest.Mayfair.Shell
{
    class Bootstrapper : UnityBootstrapper
    {
        #region Overrides of Bootstrapper

        protected override DependencyObject CreateShell()
        {
            return new Shell();
        }

        protected override void InitializeShell()
        {
            base.InitializeShell();
            Application.Current.MainWindow = (Window)Shell;
            Application.Current.MainWindow.Show();
        }

        protected override void ConfigureModuleCatalog()
        {
            base.ConfigureModuleCatalog();
            ModuleCatalog.AddModule<PropertyInvestmentModule>();
        }

        protected override void ConfigureContainer()
        {
            base.ConfigureContainer();
            Container.RegisterInstance<ISchedulerProvider>(new SchedulerProvider());
        }
        #endregion
    }
}
