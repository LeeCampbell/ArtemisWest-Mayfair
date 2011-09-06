using System;
using ArtemisWest.PropertyInvestment.Calculator.UI.RentalProperty.Calculation;
using Microsoft.Practices.Prism.Regions;

namespace ArtemisWest.PropertyInvestment.Calculator.UI.RentalProperty
{
    public sealed class RentalPropertyController
    {
        private static int Counter = 0;

        private readonly IRegionManager _regionManager;
        private readonly RentalPropertyViewModel _viewModel = new RentalPropertyViewModel();

        public RentalPropertyController(IRegionManager regionManager)
        {
            _regionManager = regionManager;

            //TODO: Get from some local cache, like a cookie. ie whatever they typed last.
            _viewModel.Title = string.Format("Series {0}", ++Counter);
            _viewModel.Loaded()
                .Subscribe(_ => { },
                () =>
                {
                    _viewModel.Input.InitialCapitalValue = 440000m;
                    _viewModel.Input.InitialLoanAmount = 413000m;
                    _viewModel.Input.LoanInterestRate = 0.065m;
                });
        }

        public RentalPropertyViewModel ViewModel
        {
            get { return _viewModel; }
        }

        #region Implementation of IRentalPropertyInputPresenter

        public void Show()
        {
            _viewModel.Loaded().Subscribe(_ => { },
                () =>
                {
                    _regionManager.AddToRegion(RegionNames.MainInputRegion, ViewModel.Input);
                    ShowChart(RegionNames.PrincipalRemainingChartRegion, ViewModel.PrincipalRemaining);
                    ShowChart(RegionNames.CapitalValueChartRegion, ViewModel.CapitalValue);
                    ShowChart(RegionNames.TotalExpensesChartRegion, ViewModel.TotalExpenses);
                    ShowChart(RegionNames.MinimumPaymentChartRegion, ViewModel.MinimumPayment);
                    ShowChart(RegionNames.BalanceChartRegion, ViewModel.Balance);
                });
        }

        private void ShowChart(string region, CalculationViewModel calculationViewModel)
        {
            //var view = new CalculationViewControl() {ViewModel = calculationViewModel};
            //_regionManager.AddToRegion(region, view);
            _regionManager.AddToRegion(region, calculationViewModel);
        }

        #endregion
    }
}
