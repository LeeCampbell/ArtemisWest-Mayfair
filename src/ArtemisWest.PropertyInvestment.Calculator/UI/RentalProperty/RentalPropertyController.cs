using ArtemisWest.PropertyInvestment.Calculator.UI.RentalProperty.Calculation;
using Microsoft.Practices.Prism.Regions;

namespace ArtemisWest.PropertyInvestment.Calculator.UI.RentalProperty
{
    public sealed class RentalPropertyController
    {
        private static int Counter = 0;

        private readonly IRegionManager _regionManager;
        private readonly RentalPropertyViewModel _viewModel;

        public RentalPropertyController(IRegionManager regionManager, RentalPropertyViewModel viewModel)
        {
            _regionManager = regionManager;
            _viewModel = viewModel;

            //TODO: Get from some local cache, like a cookie. ie whatever they typed last.
            _viewModel.Title = string.Format("Series {0}", ++Counter);

            _viewModel.Input.InitialCapitalValue = 440000m;
            _viewModel.Input.InitialLoanAmount = 413000m;
            _viewModel.Input.LoanInterestRate = 0.065m;
        }

        public RentalPropertyViewModel ViewModel
        {
            get { return _viewModel; }
        }

        #region Implementation of IRentalPropertyInputPresenter

        public void Show()
        {
            _regionManager.AddToRegion(RegionNames.MainInputRegion, ViewModel.Input);
            ShowChart(RegionNames.PrincipalRemainingChartRegion, ViewModel.PrincipalRemaining);
            ShowChart(RegionNames.CapitalValueChartRegion, ViewModel.CapitalValue);
            ShowChart(RegionNames.TotalExpensesChartRegion, ViewModel.TotalExpenses);
            ShowChart(RegionNames.MinimumPaymentChartRegion, ViewModel.MinimumPayment);
            ShowChart(RegionNames.BalanceChartRegion, ViewModel.Balance);
        }

        private void ShowChart(string region, CalculationViewModel calculationViewModel)
        {
            _regionManager.AddToRegion(region, calculationViewModel);
        }

        #endregion
    }
}
