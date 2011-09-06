using System;
using System.Windows;
using System.Windows.Controls.DataVisualization.Charting;
using ArtemisWest.PropertyInvestment.Calculator.Controls;
using ArtemisWest.PropertyInvestment.Calculator.UI.RentalProperty;
using ArtemisWest.PropertyInvestment.Calculator.UI.RentalProperty.Input;
using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;

namespace ArtemisWest.PropertyInvestment.Calculator
{
    public class PropertyInvestmentModule : IModule
    {
        private readonly IUnityContainer _container;

        public PropertyInvestmentModule(IUnityContainer container)
        {
            _container = container.CreateChildContainer();
        }

        #region Implementation of IModule

        public void Initialize()
        {
            ConfigureRegionManager();

            _container.RegisterType<IModule, PropertyInvestmentModule>();
            _container.RegisterType<IRentalPropertyInputPresenter, RentalPropertyInputPresenter>();

            LoadViews();

            var inputControler = _container.Resolve<RentalPropertyController>();
            inputControler.Show();

            var chartsPresenter = _container.Resolve<UI.Charts.ChartsPresenter>();
            chartsPresenter.Show();

            var inputControler2 = _container.Resolve<RentalPropertyController>();
            inputControler2.Show();
        }

        #endregion

        private void ConfigureRegionManager()
        {
            RegionAdapterMappings instance = ServiceLocator.Current.GetInstance<RegionAdapterMappings>();
            instance.RegisterMapping(typeof(Chart), ServiceLocator.Current.GetInstance<ChartRegionAdapter>());
        }

        //TODO: Makes this a method in Infrasturcture.
        private void LoadViews()
        {
            //TODO: Make this a scan loop that finds *View.xaml files and loads them
            //LoadView(@"UI\Charts\ChartsView.xaml");
            LoadView(@"UI\RentalProperty\Input\RentalPropertyInputView.xaml");
            LoadView(@"UI\RentalProperty\Calculation\CalculationView.xaml");
        }

        private void LoadView(string viewPath)
        {
            var packPath = string.Format(@"pack://application:,,,/{0};component/{1}", GetType().Assembly.GetName().Name,
                                         viewPath);
            var view = new Uri(packPath, UriKind.RelativeOrAbsolute);
            var skinResource = new ResourceDictionary {Source = view};

            var appMergedResourceDictionaries = Application.Current.Resources.MergedDictionaries;
            appMergedResourceDictionaries.Add(skinResource);
        }
    }
}