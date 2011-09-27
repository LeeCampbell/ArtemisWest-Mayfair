using Microsoft.Practices.Prism.Regions;

namespace ArtemisWest.PropertyInvestment.Calculator.UI.Charts
{
    public sealed class ChartsPresenter : IChartPresenter
    {
        private readonly IRegionManager _regionManager;
        private readonly ChartRegions _chartRegions;

        public ChartsPresenter(IRegionManager regionManager, ChartRegions chartRegions)
        {
            _regionManager = regionManager;
            _chartRegions = chartRegions;
        }

        #region Implementation of IChartPresenter

        public void Show()
        {
            _regionManager.AddToRegion(RegionNames.MainChartRegion, _chartRegions);
        }

        #endregion
    }
}
