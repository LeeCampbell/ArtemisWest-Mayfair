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
using System.Runtime.CompilerServices;
using ArtemisWest.PropertyInvestment.Calculator.Repository.Entities;

namespace ArtemisWest.PropertyInvestment.Calculator.UI.RentalProperty
{
    //TODO: have the ability to choose if extra weekly income should pay down loan or be retained as cash
    //...actually this sounds like some sort of composition feature?
    //  So maybe this could have the flag to apply excess to loan or not.
    //  Then decorate it with a "SavingsAccount" which applies interest to a net cash balance.
    //  However, maybe we should always pay down as much as we can? hmmmm
    public class RentalPropertyViewModel : IViewModelStatus, IDisposable
    {
        private const int TermInDays = 10958;   //365.25 * 30;
        private readonly IDailyCompoundedMortgageRepository _mortgageRepository;
        private readonly ISchedulerProvider _schedulerProvider;

        private readonly SingleAssignmentDisposable _rateLoadSubscription = new SingleAssignmentDisposable();
        private readonly SerialDisposable _currentEvaluation = new SerialDisposable();
        private readonly IDisposable _inputChangeSubscription;

        private IMortgageRates _mortgageRates;
        private ViewModelState _status;

        public RentalPropertyViewModel(IDailyCompoundedMortgageRepository mortgageRepository, ISchedulerProvider schedulerProvider)
        {
            _mortgageRepository = mortgageRepository;
            _schedulerProvider = schedulerProvider;

            BindInputTitleToCharts();

            _inputChangeSubscription = SubscribeToInputChanges();
            Status = ViewModelState.Idle;
        }


        public ViewModelState Status
        {
            get { return _status; }
            private set
            {
                if (_status != value)
                {
                    _status = value;
                    OnPropertyChanged();
                }
            }
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

        public void Load()
        {
            Status = ViewModelState.Busy;
            _rateLoadSubscription.Disposable = _mortgageRepository.Load()
                .SubscribeOn(_schedulerProvider.Background)
                .ObserveOn(_schedulerProvider.Foreground)
                .Subscribe(mr =>
                {
                    _mortgageRates = mr;
                    Status = ViewModelState.Idle;
                    Reevaluate();
                });
        }

        #region Implementation of IDisposable

        public void Dispose()
        {
            _rateLoadSubscription.Dispose();
            _inputChangeSubscription.Dispose();
            _currentEvaluation.Dispose();
        }

        #endregion

        private void BindInputTitleToCharts()
        {
            Input.WhenPropertyChanges(vm => vm.Title, newValue => Balance.SetTitle(newValue));
            Balance.SetTitle(Input.Title);
        }

        private decimal GetMinimumPayment(decimal principal, int termInDays, decimal interestRate)
        {
            if (principal <= 0)
            {
                return 0m;
            }
            decimal term = termInDays / 365.25m;

            var absoluteMin = _mortgageRates.GetMinimumMonthlyPayment(principal, term, interestRate);
            return Math.Round(absoluteMin, 2, MidpointRounding.AwayFromZero);
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
                .Subscribe(_ => Reevaluate());
        }

        private void Reevaluate()
        {
            _currentEvaluation.Disposable = UpdateValues();
        }

        private IDisposable UpdateValues()
        {
            Debug.WriteLine("  UpdateValues()");

            SetChartsAsDirty();
            //We haven't finished loading yet.
            if (_mortgageRates == null)
                return Disposable.Empty;

            return Observable.Create<Accumulator>(o =>
            {
                //Check if the Rate is within the Loaded range
                var annualInterestRate = Input.LoanInterestRate;
                if (!_mortgageRates.IsRateValid(annualInterestRate))
                {
                    return Disposable.Empty;
                }

                var seed = new Accumulator(
                    Input.InitialCapitalValue,
                    Input.InitialLoanAmount,
                    annualInterestRate,
                    Input.CaptialGrowth,
                    (loanBalance) => GetMinimumPayment(loanBalance, TermInDays, annualInterestRate));

                var weeklyMortgagePayment = GetMinimumPayment(seed.LoanBalance, TermInDays, annualInterestRate);
                var weeklyRentalIncome = Input.WeeklyRentalIncome;

                return Observable.Range(0, TermInDays)
                    .Scan(seed, (prev, i) =>
                         {
                             var date = DateTime.Today.AddDays(i);
                             decimal daysInYear = date.DaysInYear();

                             var acc = prev.CloneForIndex(i);
                             acc.AccrueInterestForDay(daysInYear);
                             if (date.Day == 1)
                             {
                                 acc.ChargeInterest();
                             }
                             if (date.DayOfWeek == DayOfWeek.Friday)
                             {
                                 acc.MakePayment(weeklyMortgagePayment);
                                 acc.CollectRent(weeklyRentalIncome);
                             }
                             acc.ApplyCapitalGrowth(daysInYear);

                             return acc;
                         }
                    ).Subscribe(o);
            })
            .Buffer(50.Milliseconds(), 1000, _schedulerProvider.Background) //Every 50ms or every 1000 events, which ever is more often.
            .SubscribeOn(_schedulerProvider.Background)
            .ObserveOn(_schedulerProvider.Foreground)
            .Subscribe(snapshots =>
            {
                Debug.WriteLine("Received {0} in batch", snapshots.Count);

                foreach (var snapshot in snapshots)
                {
                    //Debug.WriteLine($"{snapshot.CapitalAssetValue}, {snapshot.PrincipalBalance}, {snapshot.GrossCashflowIncome}, {snapshot.GrossCashflowExpenses}");

                    CapitalAssetValue.ResultOverTime[snapshot.Index].Value = snapshot.CapitalAssetValue;
                    CapitalLiabilityValue.ResultOverTime[snapshot.Index].Value = snapshot.PrincipalBalance;   //Not LoanBalance which is principal + interest. however interest is already displayed as an expense -LC
                    GrossCashflowExpenses.ResultOverTime[snapshot.Index].Value = snapshot.GrossCashflowExpenses;
                    GrossCashflowIncome.ResultOverTime[snapshot.Index].Value = snapshot.GrossCashflowIncome;
                    GrossCashflow.ResultOverTime[snapshot.Index].Value = snapshot.NetCashBalance;
                    Balance.ResultOverTime[snapshot.Index].Value = snapshot.NetBalance;
                    MinimumPayment.ResultOverTime[snapshot.Index].Value = snapshot.CalculateMinimumPayment();
                }
            },
            () => Debug.WriteLine("UpdateValues Completed."));
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
            private readonly decimal _annualInterestRate;
            private readonly decimal _yearlyGrowthRate;
            private readonly Func<decimal, decimal> _calculateMinimumPayment;

            public Accumulator(
                decimal capitalAssetValue,
                decimal principalBalance,
                decimal annualInterestRate, decimal yearlyGrowthRate, Func<decimal, decimal> calculateMinimumPayment)
            {
                CapitalAssetValue = capitalAssetValue;
                PrincipalBalance = principalBalance;
                _annualInterestRate = annualInterestRate;
                _yearlyGrowthRate = yearlyGrowthRate;
                _calculateMinimumPayment = calculateMinimumPayment;
            }

            public int Index { get; private set; }
            public decimal CapitalAssetValue { get; private set; }       //Assets
            public decimal PrincipalBalance { get; private set; }
            public decimal GrossCashflowExpenses { get; private set; }
            public decimal GrossCashflowIncome { get; private set; }        //Cashflow, could be negative if expenses exceed income.
            private decimal InterestAccrued { get; set; }        //Accruing in the background,but not charged yet, so want incur interest itself. Normally accrued interest is charged/Cystalized at EOM.
            private decimal InterestCharged { get; set; }

            public decimal LoanBalance => PrincipalBalance + InterestCharged;
            public decimal NetCashBalance => GrossCashflowIncome - GrossCashflowExpenses;
            public decimal NetBalance => CapitalAssetValue - PrincipalBalance + NetCashBalance;


            public void AccrueInterestForDay(decimal daysInYear)
            {
                InterestAccrued += (LoanBalance * _annualInterestRate / daysInYear);
            }

            public void ChargeInterest()
            {
                InterestCharged += InterestAccrued;
                GrossCashflowExpenses += InterestAccrued;
                InterestAccrued = 0;
            }

            public void MakePayment(decimal amount)
            {
                var remainingPaymentToAllocate = amount;

                var interestPayment = Math.Min(InterestCharged, remainingPaymentToAllocate);
                remainingPaymentToAllocate -= interestPayment;
                InterestCharged -= interestPayment;

                var principalPayment = Math.Min(PrincipalBalance, remainingPaymentToAllocate);
                remainingPaymentToAllocate -= principalPayment;
                PrincipalBalance -= principalPayment;

                //If we have paid down all Charged interest and Principal, then we should see if there is any accrued interest that should be paid off too.
                if (remainingPaymentToAllocate > 0)
                {
                    ChargeInterest();
                    if (PrincipalBalance > 0 || InterestCharged > 0)
                        MakePayment(remainingPaymentToAllocate);
                }
            }

            public void CollectRent(decimal amount)
            {
                GrossCashflowIncome += amount;
            }


            public void ApplyCapitalGrowth(decimal daysInYear)
            {
                var dailyGrowth = 1m + (_yearlyGrowthRate / daysInYear);
                CapitalAssetValue *= dailyGrowth;
            }

            public decimal CalculateMinimumPayment()
            {
                return LoanBalance <= 0
                    ? 0m
                    : _calculateMinimumPayment(LoanBalance);
            }

            public Accumulator CloneForIndex(int index)
            {
                return new Accumulator(CapitalAssetValue, PrincipalBalance, _annualInterestRate, _yearlyGrowthRate, _calculateMinimumPayment)
                {
                    Index = index,
                    InterestAccrued = InterestAccrued,
                    InterestCharged = InterestCharged,
                    GrossCashflowIncome = GrossCashflowIncome,
                    GrossCashflowExpenses = GrossCashflowExpenses,
                };
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}