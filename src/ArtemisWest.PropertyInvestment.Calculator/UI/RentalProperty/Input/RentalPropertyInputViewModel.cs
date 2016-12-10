using Microsoft.Practices.Prism.Mvvm;

namespace ArtemisWest.PropertyInvestment.Calculator.UI.RentalProperty.Input
{
    public class RentalPropertyInputViewModel : BindableBase
    {
        private decimal _initialCapitalValue;
        public decimal InitialCapitalValue
        {
            get { return _initialCapitalValue; }
            set { SetProperty(ref _initialCapitalValue, value); }
        }

        private decimal _initialLoanAmount;
        public decimal InitialLoanAmount
        {
            get { return _initialLoanAmount; }
            set { SetProperty(ref _initialLoanAmount, value); }
        }

        private decimal _loanInterestRate;
        public decimal LoanInterestRate
        {
            get { return _loanInterestRate; }
            set { SetProperty(ref _loanInterestRate, value); }
        }

        private decimal _captialGrowth;
        public decimal CaptialGrowth
        {
            get { return _captialGrowth; }
            set { SetProperty(ref _captialGrowth, value); }
        }

        private decimal _weeklyRentalIncome;
        public decimal WeeklyRentalIncome
        {
            get { return _weeklyRentalIncome; }
            set { SetProperty(ref _weeklyRentalIncome, value); }
        }

        private string _title;
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }
        //TODO: Add loan term. Changes to this would obviously need to correct the length of the Balances collections. There4 they could not be arrays anymore.

    }
}