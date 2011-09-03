using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using ArtemisWest.Mayfair.Infrastructure;
using Microsoft.Practices.Prism.Regions;

namespace ArtemisWest.PropertyInvestment.Calculator.UI.RentalProperty
{
    public sealed class RentalPropertyController
    {
        private readonly IRegionManager _regionManager;
        private readonly RentalPropertyViewModel _viewModel = new RentalPropertyViewModel();

        public RentalPropertyController(IRegionManager regionManager)
        {
            _regionManager = regionManager;

            //TODO: Get from some local cache, like a cookie. ie whatever they typed last.

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
                    _regionManager.AddToRegion(RegionNames.MainChartRegion, ViewModel.PrincipalRemaining);
                    _regionManager.AddToRegion(RegionNames.MainChartRegion, ViewModel.CapitalValue);
                    _regionManager.AddToRegion(RegionNames.MainChartRegion, ViewModel.TotalExpenses);
                    _regionManager.AddToRegion(RegionNames.MainChartRegion, ViewModel.MinimumPayment);
                    _regionManager.AddToRegion(RegionNames.MainChartRegion, ViewModel.Balance);
                });
        }

        #endregion
    }
}
