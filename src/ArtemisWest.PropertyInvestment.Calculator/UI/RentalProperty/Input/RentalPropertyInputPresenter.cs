using Microsoft.Practices.Prism.Regions;

namespace ArtemisWest.PropertyInvestment.Calculator.UI.RentalProperty.Input
{
    class RentalPropertyInputPresenter : IRentalPropertyInputPresenter
    {
        private readonly IRegionManager _regionManager;
        private readonly RentalPropertyInputViewModel _viewModel = new RentalPropertyInputViewModel();

        public RentalPropertyInputPresenter(IRegionManager regionManager)
        {
            _regionManager = regionManager;
        }

        public RentalPropertyInputViewModel ViewModel
        {
            get { return _viewModel; }
        }

        #region Implementation of IRentalPropertyInputPresenter

        public void Show()
        {
            _regionManager.AddToRegion(RegionNames.MainInputRegion, ViewModel);
        }

        #endregion
    }
}
