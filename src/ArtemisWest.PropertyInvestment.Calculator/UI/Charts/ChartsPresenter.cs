using Microsoft.Practices.Prism.Regions;

namespace ArtemisWest.PropertyInvestment.Calculator.UI.Charts
{
    public class ChartsPresenter : IChartPresenter
    {
        private readonly IRegionManager _regionManager;
        private readonly ChartRegionsView _chartRegionsView;

        public ChartsPresenter(IRegionManager regionManager, ChartRegionsView chartRegionsView)
        {
            _regionManager = regionManager;
            _chartRegionsView = chartRegionsView;
        }

        #region Implementation of IChartPresenter

        public void Show()
        {
            _regionManager.AddToRegion(RegionNames.MainChartRegion, _chartRegionsView);
        }

        #endregion
    }

    public interface IChartPresenter
    {
        void Show();
    }
}
