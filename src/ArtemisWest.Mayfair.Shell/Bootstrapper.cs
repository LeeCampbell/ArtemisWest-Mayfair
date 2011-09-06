using System;
using System.Windows;
using System.Windows.Controls.DataVisualization.Charting;
//using ArtemisWest.Mayfair.Shell.Controls;
using ArtemisWest.PropertyInvestment.Calculator;
using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Prism.UnityExtensions;
using Microsoft.Practices.ServiceLocation;

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
        //protected override Microsoft.Practices.Prism.Regions.IRegionBehaviorFactory ConfigureDefaultRegionBehaviors()
        //{
        //    var defaultRegionBehaviorFactory =  base.ConfigureDefaultRegionBehaviors();
        //    defaultRegionBehaviorFactory.AddIfMissing(ChartSeriesSyncBehavior.BehaviorKey, typeof(ChartSeriesSyncBehavior));
        //    return defaultRegionBehaviorFactory;
        //}
        //protected override Microsoft.Practices.Prism.Regions.RegionAdapterMappings ConfigureRegionAdapterMappings()
        //{
        //    var instance =  base.ConfigureRegionAdapterMappings();
        //    instance.RegisterMapping(typeof(Chart), ServiceLocator.Current.GetInstance<ChartRegionAdapter>());
        //    return instance;
        //}
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
}
