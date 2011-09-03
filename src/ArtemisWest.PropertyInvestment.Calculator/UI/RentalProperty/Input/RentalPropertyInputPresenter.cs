using ArtemisWest.Mayfair.Infrastructure;
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

            //TODO: Get from some local cache, like a cookie. ie whatever they typed last.

            _viewModel.InitialCapitalValue = 440000m;
            _viewModel.InitialLoanAmount = 413000m;
            _viewModel.LoanInterestRate = 0.065m;
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
