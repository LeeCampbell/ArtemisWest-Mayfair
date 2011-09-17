using Microsoft.Practices.Prism.ViewModel;

namespace ArtemisWest.PropertyInvestment.Calculator.UI.RentalProperty.Input
{
    public class RentalPropertyInputViewModel : NotificationObject
    {
        private decimal _initialCapitalValue;
        public decimal InitialCapitalValue
        {
            get { return _initialCapitalValue; }
            set
            {
                if (_initialCapitalValue != value)
                {
                    _initialCapitalValue = value;
                    RaisePropertyChanged(() => InitialCapitalValue);
                }
            }
        }

        private decimal _initialLoanAmount;
        public decimal InitialLoanAmount
        {
            get { return _initialLoanAmount; }
            set
            {
                if (_initialLoanAmount != value)
                {
                    _initialLoanAmount = value;
                    RaisePropertyChanged(() => InitialLoanAmount);
                }
            }
        }

        private decimal _loanInterestRate;
        public decimal LoanInterestRate
        {
            get { return _loanInterestRate; }
            set
            {
                if (_loanInterestRate != value)
                {
                    _loanInterestRate = value;
                    RaisePropertyChanged(() => LoanInterestRate);
                }
            }
        }

        private decimal _captialGrowth;
        public decimal CaptialGrowth
        {
            get { return _captialGrowth; }
            set
            {
                if (_captialGrowth != value)
                {
                    _captialGrowth = value;
                    RaisePropertyChanged(() => CaptialGrowth);
                }
            }
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
                }
            }
        }
        //TODO: Add loan term. Changes to this would obviously need to correct the length of the Balances collections. There4 they could not be arrays anymore.
    }
}