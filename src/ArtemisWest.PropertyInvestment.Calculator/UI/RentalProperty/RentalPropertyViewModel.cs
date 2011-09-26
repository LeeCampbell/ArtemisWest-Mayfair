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
            var repoIsLoaded = _mortgageRepository.IsLoaded
                .Where(isLoaded => isLoaded);

            return Observable.CombineLatest(
                    repoIsLoaded,
                    propertyChanges,
                    (isLoaded, propertyChanged) => Unit.Default
                    )
                    .Subscribe(_ => Reevaluate());
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
            var weeklyMortgagePayment = GetMinimumPayment(seed.PrincipalRemaining, TermInDays, annualInterestRate);
            var weeklyRentalIncome = Input.WeeklyRentalIncome;

            return Observable.Range(0, TermInDays)
                .Scan(
                    seed,
                    (acc, i) =>
                    {
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
                        if (principalRemaining < 0)
                        {
                            principalRemaining = 0m;
                        }
                        var minPayment = GetMinimumPayment(acc.PrincipalRemaining, TermInDays - i, annualInterestRate);

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
                .Buffer(50.Milliseconds())
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