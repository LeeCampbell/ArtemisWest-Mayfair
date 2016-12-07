using ArtemisWest.PropertyInvestment.Calculator.UI.RentalProperty.Calculation;
using Microsoft.Practices.Prism.Regions;

namespace ArtemisWest.PropertyInvestment.Calculator.UI.RentalProperty
{
    public sealed class RentalPropertyController
    {
        private static int _counter;

        private readonly IRegionManager _regionManager;
        private readonly RentalPropertyViewModel _viewModel;

        public RentalPropertyController(IRegionManager regionManager, RentalPropertyViewModel viewModel)
        {
            _regionManager = regionManager;
            _viewModel = viewModel;

            //TODO: Get from some local cache, like a cookie. ie whatever they typed last.
            _viewModel.Input.Title = string.Format("Investment {0}", ++_counter);

            _viewModel.Input.InitialCapitalValue = 440000m;
            _viewModel.Input.InitialLoanAmount = 413000m;
            _viewModel.Input.LoanInterestRate = 0.065m;
            _viewModel.Input.WeeklyRentalIncome = 300;
        }

        public RentalPropertyViewModel ViewModel
        {
            get { return _viewModel; }
        }

        #region Implementation of IRentalPropertyInputPresenter

        public void Show()
        {
            _regionManager.AddToRegion(RegionNames.MainInputRegion, ViewModel.Input);
            ShowChart(RegionNames.LiabilityValueChartRegion, ViewModel.CapitalLiabilityValue);
            ShowChart(RegionNames.AssetValueChartRegion, ViewModel.CapitalAssetValue);
            //ShowChart(RegionNames.TotalExpensesChartRegion, ViewModel.TotalExpenses);
            ShowChart(RegionNames.GrossCashBalanceChartRegion, ViewModel.GrossCashflow);
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
