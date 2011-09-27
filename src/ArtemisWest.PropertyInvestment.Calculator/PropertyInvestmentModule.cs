using System.Windows.Controls.DataVisualization.Charting;
using ArtemisWest.Mayfair.Infrastructure;
using ArtemisWest.PropertyInvestment.Calculator.Controls;
using ArtemisWest.PropertyInvestment.Calculator.UI.RentalProperty;
using ArtemisWest.PropertyInvestment.Calculator.UI.RentalProperty.Input;
using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;

namespace ArtemisWest.PropertyInvestment.Calculator
{
    public sealed class PropertyInvestmentModule : IModule
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

            var singleton = new ContainerControlledLifetimeManager();
            _container.RegisterType<IModule, PropertyInvestmentModule>();
            _container.RegisterType<IRentalPropertyInputPresenter, RentalPropertyInputPresenter>();
            _container.RegisterType<Repository.IDailyCompoundedMortgageRepository, Repository.DailyCompoundedMortgageRepository>(singleton);

            this.LoadViews();

            var chartsPresenter = _container.Resolve<UI.Charts.ChartsPresenter>();
            chartsPresenter.Show();

            var inputControler = _container.Resolve<RentalPropertyController>();
            inputControler.Show();

            var inputControler2 = _container.Resolve<RentalPropertyController>();
            inputControler2.Show();
        }

        #endregion

        private static void ConfigureRegionManager()
        {
            var instance = ServiceLocator.Current.GetInstance<RegionAdapterMappings>();
            instance.RegisterMapping(typeof(Chart), ServiceLocator.Current.GetInstance<ChartRegionAdapter>());
        }
    }
}