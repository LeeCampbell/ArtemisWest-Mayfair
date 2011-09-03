using System;
using System.Windows;
using ArtemisWest.PropertyInvestment.Calculator;
//using Autofac;
using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Prism.UnityExtensions;
//using Microsoft.Practices.ServiceLocation;

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
        #endregion
    }
    public static class ModuleCatalogExtensions
    {
        public static void AddModule<T>(this IModuleCatalog moduleCatalog) where T:IModule
        {
            Type moduleType = typeof(T);
            moduleCatalog.AddModule(new ModuleInfo(moduleType.Name, moduleType.AssemblyQualifiedName));
        }
    }


    //class Bootstrapper : Microsoft.Practices.Prism.Bootstrapper
    //{
    //    #region Overrides of Bootstrapper

    //    public override void Run(bool runWithDefaultConfiguration)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    protected override DependencyObject CreateShell()
    //    {
    //        return ServiceLocator.Current.GetInstance<Shell>();
    //    }

    //    protected override void ConfigureServiceLocator()
    //    {
    //        var builder = new ContainerBuilder();

    //        builder.RegisterType<RegionManager>().As<IRegionManager>();

    //        var container = builder.Build();
    //        var serviceLocator = new AutofacContrib.CommonServiceLocator.AutofacServiceLocator(container);
    //        ServiceLocator.SetLocatorProvider(()=>serviceLocator);
    //    }

        
    //    protected override void InitializeShell()
    //    {
    //        Application.Current.MainWindow = (Window)Shell;
    //        Application.Current.MainWindow.Show();

    //        base.InitializeShell();            
    //    }
    //    #endregion
    //}
}
