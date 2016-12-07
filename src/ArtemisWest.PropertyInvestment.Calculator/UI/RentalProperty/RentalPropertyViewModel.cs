using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Reactive.Linq;
using ArtemisWest.Mayfair.Infrastructure;
using ArtemisWest.PropertyInvestment.Calculator.Repository;
using ArtemisWest.PropertyInvestment.Calculator.UI.RentalProperty.Calculation;
using ArtemisWest.PropertyInvestment.Calculator.UI.RentalProperty.Input;
using Unit = System.Reactive.Unit;
using System.Reactive.Disposables;

namespace ArtemisWest.PropertyInvestment.Calculator.UI.RentalProperty
{
    public class RentalPropertyViewModel : IDisposable
    {
        private const int TermInDays = 10958;   //365.25 * 30;
        private readonly IDailyCompoundedMortgageRepository _mortgageRepository;
        private readonly ISchedulerProvider _schedulerProvider;

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

        public RentalPropertyInputViewModel Input { get; } = new RentalPropertyInputViewModel();

        public CalculationViewModel CapitalLiabilityValue { get; } = new CalculationViewModel(TermInDays);

        public CalculationViewModel MinimumPayment { get; } = new CalculationViewModel(TermInDays);

        public CalculationViewModel CapitalAssetValue { get; } = new CalculationViewModel(TermInDays);

        public CalculationViewModel GrossCashflowIncome { get; } = new CalculationViewModel(TermInDays);

        public CalculationViewModel GrossCashflowExpenses { get; } = new CalculationViewModel(TermInDays);

        public CalculationViewModel GrossCashflow { get; } = new CalculationViewModel(TermInDays);

        public CalculationViewModel Balance { get; } = new CalculationViewModel(TermInDays);

        public CalculationViewModel MonthlyIncome { get; } = new CalculationViewModel(TermInDays);

        public CalculationViewModel MonthlyExpenses { get; } = new CalculationViewModel(TermInDays);

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
            return Observable.Create<Accumulator>(o =>
                {
                    //Check if the Rate is within the Loaded range
                    var annualInterestRate = Input.LoanInterestRate;
                    if (!IsRateValid(annualInterestRate))
                    {
                        return Disposable.Empty;
                    }

                    var yearlyGrowth = Input.CaptialGrowth;
                    var seed = new Accumulator
                    {
                        CapitalAssetValue = Input.InitialCapitalValue,
                        CapitalLiabilityValue = Input.InitialLoanAmount,
                        PrincipalBalance = Input.InitialLoanAmount
                    };

                    var weeklyMortgagePayment = GetMinimumPayment(seed.CapitalLiabilityValue, TermInDays, annualInterestRate);
                    var weeklyRentalIncome = Input.WeeklyRentalIncome;

                    return Observable.Range(0, TermInDays)
                        .Scan(
                            seed,
                            (acc, i) =>
                            {
                                var date = DateTime.Today.AddDays(i);
                                var daysInYear = date.DaysInYear();
                                var dailyGrowth = 1m + (yearlyGrowth/daysInYear);

                                var interestAccrued = acc.InterestAccrued +
                                                        (annualInterestRate*
                                                        acc.CapitalLiabilityValue)*
                                                        (1M/daysInYear);
                                var interestCharged = 0m;
                                var loanBalance = acc.CapitalLiabilityValue;
                                var dailyIncome = 0m;
                                if (date.Day == 1)
                                {
                                    interestCharged = interestAccrued;
                                    interestAccrued = 0m;
                                    loanBalance = loanBalance + interestCharged;
                                }
                                if (date.DayOfWeek == DayOfWeek.Friday)
                                {
                                    var mortgagePayment = Math.Min(loanBalance, weeklyMortgagePayment);
                                    loanBalance = loanBalance - mortgagePayment;
                                    dailyIncome = weeklyRentalIncome;
                                }
                                decimal minPayment;
                                if (loanBalance < 0)
                                {
                                    loanBalance = 0m;
                                    minPayment = 0m;
                                }
                                else
                                {
                                    minPayment = GetMinimumPayment(acc.CapitalLiabilityValue, TermInDays - i, annualInterestRate);
                                }

                                return new Accumulator
                                {
                                    Index = i,
                                    CapitalAssetValue = acc.CapitalAssetValue*dailyGrowth,
                                    CapitalLiabilityValue = loanBalance,
                                    InterestAccrued = interestAccrued,
                                    InterestCharged = interestCharged,
                                    GrossCashflowIncome = acc.GrossCashflowIncome + dailyIncome,
                                    GrossCashflowExpenses = acc.GrossCashflowExpenses + interestCharged,
                                    MinimumPayment = minPayment,
                                };
                            }
                        ).Subscribe(o);
                })
                .Buffer(50.Milliseconds(), 1000, _schedulerProvider.ThreadPool) //Every 50ms or every 1000 events, which ever is more often.
                .SubscribeOn(_schedulerProvider.ThreadPool)
                .ObserveOn(_schedulerProvider.Dispatcher)
                .Subscribe(kvps =>
                {
                    Debug.WriteLine("Received {0} in batch", kvps.Count);
                    foreach (var kvp in kvps)
                    {
                        CapitalAssetValue.ResultOverTime[kvp.Index].Value = kvp.CapitalAssetValue;
                        CapitalLiabilityValue.ResultOverTime[kvp.Index].Value = kvp.CapitalLiabilityValue;
                        GrossCashflowExpenses.ResultOverTime[kvp.Index].Value = kvp.GrossCashflowExpenses;
                        GrossCashflowIncome.ResultOverTime[kvp.Index].Value = kvp.GrossCashflowIncome;
                        GrossCashflow.ResultOverTime[kvp.Index].Value = kvp.GrossCashflowIncome - kvp.GrossCashflowExpenses;
                        Balance.ResultOverTime[kvp.Index].Value = kvp.CapitalAssetValue - kvp.CapitalLiabilityValue + kvp.GrossCashflowIncome - kvp.GrossCashflowExpenses;
                        MinimumPayment.ResultOverTime[kvp.Index].Value = kvp.MinimumPayment;
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
            CapitalAssetValue.IsDirty =
                GrossCashflowExpenses.IsDirty =
                MinimumPayment.IsDirty =
                GrossCashflowIncome.IsDirty =
                CapitalLiabilityValue.IsDirty =
                Balance.IsDirty = true;
        }

        private static Unit IgnoreValues<T1, T2>(T1 a, T2 b)
        {
            return Unit.Default;
        }

        private sealed class Accumulator
        {
            public int Index { get; set; }
            public decimal CapitalAssetValue { get; set; }       //Assets
            public decimal CapitalLiabilityValue { get; set; }   //Liabilities
            public decimal PrincipalBalance { get; set; }   
            public decimal InterestBalance { get; set; }   
            public decimal MinimumPayment { get; set; }
            public decimal InterestAccrued { get; set; }        //Accrueing in the background,but not charged yet, so want incur interest itself. Normally accrued interest is charged/Cystalized at EOM.
            public decimal InterestCharged { get; set; }
            public decimal GrossCashflowExpenses { get; set; }
            public decimal GrossCashflowIncome { get; set; }        //Cashflow, could be negative if expenses exceed income.
        }
    }
}