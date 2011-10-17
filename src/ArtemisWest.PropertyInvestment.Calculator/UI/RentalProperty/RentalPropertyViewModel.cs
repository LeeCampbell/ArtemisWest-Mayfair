using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Reactive.Linq;
using ArtemisWest.Mayfair.Infrastructure;
using ArtemisWest.PropertyInvestment.Calculator.Repository;
using ArtemisWest.PropertyInvestment.Calculator.UI.RentalProperty.Calculation;
using ArtemisWest.PropertyInvestment.Calculator.UI.RentalProperty.Input;
using Microsoft.Practices.Prism.ViewModel;
using Unit = System.Reactive.Unit;
using System.Reactive.Disposables;

namespace ArtemisWest.PropertyInvestment.Calculator.UI.RentalProperty
{
    public class RentalPropertyViewModel : NotificationObject, IDisposable
    {
        private const int TermInDays = 10958;   //365.25 * 30;
        private readonly IDailyCompoundedMortgageRepository _mortgageRepository;
        private readonly ISchedulerProvider _schedulerProvider;

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

        public RentalPropertyViewModel(IDailyCompoundedMortgageRepository mortgageRepository, ISchedulerProvider schedulerProvider)
        {
            _mortgageRepository = mortgageRepository;
            _schedulerProvider = schedulerProvider;

            BindInputTitleToCharts();

            _inputChangeSubscription = SubscribeToInputChanges();

            _mortgageRepository.Load();
        }

        public decimal GetMinimumPayment(decimal principal, int termInDays, decimal interestRate)
        {
            if (principal <= 0)
            {
                return 0m;
            }
            decimal term = termInDays / 365.25m;
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

        #region Implementation of IDisposable

        public void Dispose()
        {
            _inputChangeSubscription.Dispose();
            _currentEvaluation.Dispose();
        }

        #endregion

        private void BindInputTitleToCharts()
        {
            Input.WhenPropertyChanges(vm => vm.Title, newValue => Balance.SetTitle(newValue));
            Balance.SetTitle(Input.Title);
        }

        private IDisposable SubscribeToInputChanges()
        {
            var inputPropertyChanges =
                Observable.FromEventPattern<PropertyChangedEventHandler, PropertyChangedEventArgs>(
                    eh => eh.Invoke,
                    eh => Input.PropertyChanged += eh,
                    eh => Input.PropertyChanged -= eh);

            var propertyChanges = inputPropertyChanges.Where(e => e.EventArgs.PropertyName != "Title");
            
            return propertyChanges
                .CombineLatest(_mortgageRepository.MinimumRate, IgnoreValues)
                .CombineLatest(_mortgageRepository.MaximumRate, IgnoreValues)
                .CombineLatest(_mortgageRepository.IsLoaded, IgnoreValues)
                .Subscribe(_ => Reevaluate());
        }

        private void Reevaluate()
        {
            using (_currentEvaluation) { }
            _currentEvaluation = UpdateValues();
        }

        private IDisposable UpdateValues()
        {
            Debug.WriteLine("  UpdateValues()");
            
            SetChartsAsDirty();

            //Check if the Rate is within the Loaded range
            var annualInterestRate = Input.LoanInterestRate;
            if (!IsRateValid(annualInterestRate))
            {
                return Disposable.Empty;
            }

            var yearlyGrowth = Input.CaptialGrowth;
            var seed = new Accumulator
            {
                CaptialValue = Input.InitialCapitalValue,
                PrincipalRemaining = Input.InitialLoanAmount
            };

            var weeklyMortgagePayment = GetMinimumPayment(seed.PrincipalRemaining, TermInDays, annualInterestRate);
            var weeklyRentalIncome = Input.WeeklyRentalIncome;

            return Observable.Range(0, TermInDays)
                .Scan(
                    seed,
                    (acc, i) =>
                    {
                        //TODO: put in some logging to find out why the tails de-values?
                        DateTime date = DateTime.Today.AddDays(i);
                        var daysInYear = date.DaysInYear();
                        var dailyGrowth = 1m + (yearlyGrowth / daysInYear);
                        var interestAccrued = acc.InterestAccrued + (annualInterestRate * acc.PrincipalRemaining) * (1M / daysInYear);
                        var interestCharged = 0m;
                        var principalRemaining = acc.PrincipalRemaining;
                        var dailyIncome = 0m;
                        if (date.Day == 1)
                        {
                            interestCharged = interestAccrued;
                            interestAccrued = 0m;
                            principalRemaining = principalRemaining + interestCharged;
                        }
                        if (date.DayOfWeek == DayOfWeek.Friday)
                        {
                            principalRemaining = principalRemaining - weeklyMortgagePayment;
                            dailyIncome = weeklyRentalIncome - weeklyMortgagePayment;
                        }
                        decimal minPayment;
                        if (principalRemaining < 0)
                        {
                            principalRemaining = 0m;
                            minPayment = 0m;
                            //TODO: Start logging  = true
                        }
                        else
                        {
                            minPayment = GetMinimumPayment(acc.PrincipalRemaining, TermInDays - i, annualInterestRate);
                        }

                        return new Accumulator
                        {
                            Index = i,
                            CaptialValue = acc.CaptialValue * dailyGrowth,
                            PrincipalRemaining = principalRemaining,
                            TotalIncome = acc.TotalIncome + dailyIncome,
                            InterestAccrued = interestAccrued,
                            InterestCharged = interestCharged,
                            TotalInterestCharged = acc.TotalInterestCharged + interestCharged,
                            MinimumPayment = minPayment,
                        };
                    }
                )
                .Buffer(50.Milliseconds(), 1000, _schedulerProvider.ThreadPool) //Every 50ms or every 1000 events, which ever is more often.
                .SubscribeOn(_schedulerProvider.ThreadPool)
                .ObserveOn(_schedulerProvider.Dispatcher)
                .Subscribe(kvps =>
                {
                    Debug.WriteLine("Recieved {0} in batch", kvps.Count);
                    foreach (var kvp in kvps)
                    {
                        CapitalValue.ResultOverTime[kvp.Index].Value = kvp.CaptialValue;
                        TotalExpenses.ResultOverTime[kvp.Index].Value = kvp.TotalInterestCharged;   //?May need to re-assess this.
                        MinimumPayment.ResultOverTime[kvp.Index].Value = kvp.MinimumPayment;
                        TotalIncome.ResultOverTime[kvp.Index].Value = kvp.TotalIncome;              //?...Or this.
                        PrincipalRemaining.ResultOverTime[kvp.Index].Value = kvp.PrincipalRemaining;
                        Balance.ResultOverTime[kvp.Index].Value = kvp.CaptialValue - kvp.PrincipalRemaining + kvp.TotalIncome; //?... and this.
                    }
                },
                () => Debug.WriteLine("UpdateValues Completed."));
        }

        private bool IsRateValid(decimal annualInterestRate)
        {
            var minimumRate = _mortgageRepository.MinimumRate.First();
            var maximumRate = _mortgageRepository.MaximumRate.First();

            if (annualInterestRate < minimumRate ||  maximumRate < annualInterestRate)
            {
                Debug.WriteLine("    --Short circuit. {0} {1} {2} ", minimumRate, annualInterestRate, maximumRate);
                return false;
            }
            return true;
        }

        private void SetChartsAsDirty()
        {
            CapitalValue.IsDirty =
                TotalExpenses.IsDirty =
                MinimumPayment.IsDirty =
                TotalIncome.IsDirty =
                PrincipalRemaining.IsDirty =
                Balance.IsDirty = true;
        }

        private static Unit IgnoreValues<T1, T2>(T1 a, T2 b)
        {
            return Unit.Default;
        }

        private sealed class Accumulator
        {
            public int Index { get; set; }
            public decimal CaptialValue { get; set; }       //Assets
            public decimal PrincipalRemaining { get; set; } //Liabilities
            public decimal MinimumPayment { get; set; }
            public decimal InterestAccrued { get; set; }
            public decimal InterestCharged { get; set; }
            public decimal TotalInterestCharged { get; set; }
            public decimal TotalIncome { get; set; }        //Cashflow, could be negative if expenses exceed income.
        }
    }
}