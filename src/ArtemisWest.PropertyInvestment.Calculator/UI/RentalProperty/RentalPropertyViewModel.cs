using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using ArtemisWest.PropertyInvestment.Calculator.Repository;
using ArtemisWest.PropertyInvestment.Calculator.UI.RentalProperty.Calculation;
using ArtemisWest.PropertyInvestment.Calculator.UI.RentalProperty.Input;
using Microsoft.Practices.Prism.ViewModel;

namespace ArtemisWest.PropertyInvestment.Calculator.UI.RentalProperty
{
    public class RentalPropertyViewModel : NotificationObject, IDisposable
    {
        private const int TermInDays = 10958;   //365.25 * 30;
        private readonly DailyCompoundedMortgageRepository _mortgageRepository;

        private readonly RentalPropertyInputViewModel _input = new RentalPropertyInputViewModel();
        private readonly CalculationViewModel _principalRemaining = new CalculationViewModel(TermInDays);
        private readonly CalculationViewModel _minimumPayment = new CalculationViewModel(TermInDays);
        private readonly CalculationViewModel _capitalValue = new CalculationViewModel(TermInDays);
        private readonly CalculationViewModel _totalIncome = new CalculationViewModel(TermInDays);
        private readonly CalculationViewModel _totalExpenses = new CalculationViewModel(TermInDays);
        private readonly CalculationViewModel _balance = new CalculationViewModel(TermInDays);
        private readonly CalculationViewModel _monthlyIncome = new CalculationViewModel(TermInDays);
        private readonly CalculationViewModel _monthlyExpenses = new CalculationViewModel(TermInDays);

        private IDisposable _currentEvaluation;
        private readonly IDisposable _inputChangeSubscription;
        private IConnectableObservable<Unit> _isRepoLoaded;

        public RentalPropertyViewModel()
        {
            _mortgageRepository = new DailyCompoundedMortgageRepository();
            var inputPropertyChanges =
                    Observable.FromEventPattern<PropertyChangedEventHandler, PropertyChangedEventArgs>(
                        eh => eh.Invoke,
                        eh => Input.PropertyChanged += eh,
                        eh => Input.PropertyChanged -= eh);

            _inputChangeSubscription = inputPropertyChanges
                .Subscribe(_ => Reevaluate());

            _isRepoLoaded = _mortgageRepository
                .Load()
                .SubscribeOn(Scheduler.ThreadPool)
                .ObserveOnDispatcher()
                .PublishLast();
            _isRepoLoaded.Connect();
        }

        public IObservable<Unit> Loaded()
        {
            return _isRepoLoaded;
        }

        private void Reevaluate()
        {
            using (_currentEvaluation) { }
            _currentEvaluation = UpdateValues();
        }

        private IDisposable UpdateValues()
        {
            var yearlyGrowth = Input.CaptialGrowth;
            var seed = new Accumulator
                           {
                               CaptialValue = Input.InitialCapitalValue,
                               PrincipalRemaining = Input.InitialLoanAmount
                           };

            var annualInterestRate = Input.LoanInterestRate;
            var monthlyPayment = GetMinimumPayment(seed.PrincipalRemaining, TermInDays, annualInterestRate);

            return Observable.Range(0, TermInDays)
                .Scan(
                    seed,
                    (acc, i) =>
                    {
                        DateTime date = DateTime.Today.AddDays(i);
                        var daysInYear = date.DaysInYear();
                        var dailyGrowth = 1m + yearlyGrowth / daysInYear;
                        var interestAccrued = acc.InterestAccrued + (annualInterestRate * acc.PrincipalRemaining) * (1M / daysInYear);
                        var interestCharged = 0m;
                        var principalRemaining = acc.PrincipalRemaining;
                        if (date.Day == 1)
                        {
                            interestCharged = interestAccrued;
                            interestAccrued = 0m;
                            principalRemaining = principalRemaining + interestCharged;
                        }
                        if(date.DayOfWeek==DayOfWeek.Friday)
                        {
                            principalRemaining = principalRemaining - monthlyPayment;
                        }
                        if(principalRemaining<0)
                        {
                            principalRemaining = 0m;
                        }
                        var minPayment = GetMinimumPayment(acc.PrincipalRemaining, TermInDays - i, annualInterestRate);

                        return new Accumulator
                                   {
                                       Index = i,
                                       CaptialValue = acc.CaptialValue * dailyGrowth,
                                       InterestAccrued = interestAccrued,
                                       InterestCharged = interestCharged,
                                       TotalInterestCharged = acc.TotalInterestCharged + interestCharged,
                                       MinimumPayment = minPayment,
                                       PrincipalRemaining = principalRemaining
                                   };
                    }
                )
                .Buffer(500)    //Batch calls to the Dispatcher, to lighten the load on the Charting control.
                .SubscribeOn(Scheduler.ThreadPool)
                .ObserveOn(DispatcherScheduler.Instance)
                .Subscribe(kvps =>
                               {
                                   foreach (var kvp in kvps)
                                   {
                                       CapitalValue.ResultOverTime[kvp.Index].Value = kvp.CaptialValue;
                                       TotalExpenses.ResultOverTime[kvp.Index].Value = kvp.TotalInterestCharged;
                                       MinimumPayment.ResultOverTime[kvp.Index].Value = kvp.MinimumPayment;
                                       PrincipalRemaining.ResultOverTime[kvp.Index].Value = kvp.PrincipalRemaining;
                                       Balance.ResultOverTime[kvp.Index].Value = kvp.CaptialValue - kvp.PrincipalRemaining;
                                   }
                               },
                               () => Debug.WriteLine("UpdateValues Completed."));
        }

        private sealed class Accumulator
        {
            public int Index { get; set; }
            public decimal CaptialValue { get; set; }
            public decimal PrincipalRemaining { get; set; }
            public decimal MinimumPayment { get; set; }
            public decimal InterestAccrued { get; set; }
            public decimal InterestCharged { get; set; }
            public decimal TotalInterestCharged { get; set; }
        }

        public decimal GetMinimumPayment(decimal principal, int termInDays, decimal interestRate)
        {
            if(principal<=0)
            {
                return 0m;
            }
            double term = termInDays / 365.0;
            var absoluteMin = _mortgageRepository.GetMinimumMonthlyPayment(principal, term, interestRate);
            return Math.Round(absoluteMin, 2, MidpointRounding.AwayFromZero);
        }

        public RentalPropertyInputViewModel Input
        {
            get { return _input; }
        }

        public CalculationViewModel PrincipalRemaining
        {
            get { return _principalRemaining; }
        }

        public CalculationViewModel MinimumPayment
        {
            get { return _minimumPayment; }
        }

        public CalculationViewModel CapitalValue
        {
            get { return _capitalValue; }
        }

        public CalculationViewModel TotalIncome
        {
            get { return _totalIncome; }
        }

        public CalculationViewModel TotalExpenses
        {
            get { return _totalExpenses; }
        }

        public CalculationViewModel Balance
        {
            get { return _balance; }
        }

        public CalculationViewModel MonthlyIncome
        {
            get { return _monthlyIncome; }
        }

        public CalculationViewModel MonthlyExpenses
        {
            get { return _monthlyExpenses; }
        }

        private string _title;

        public string Title
        {
            get { return _title; }
            set
            {
                if (_title != value)
                {
                    _title = value;
                    RaisePropertyChanged(() => Title);

                    _principalRemaining.Title = value;
                    _minimumPayment.Title = value;
                    _capitalValue.Title = value;
                    _totalIncome.Title = value;
                    _totalExpenses.Title = value;
                    _balance.Title = value;
                    _monthlyIncome.Title = value;
                    _monthlyExpenses.Title = value;
                }
            }
        }

        #region Implementation of IDisposable

        public void Dispose()
        {
            _inputChangeSubscription.Dispose();
            _currentEvaluation.Dispose();
        }

        #endregion
    }
}