using System;
using System.Windows;
using ArtemisWest.PropertyInvestment.Calculator.Entities;
using Microsoft.Practices.Prism.ViewModel;

namespace ArtemisWest.PropertyInvestment.Calculator.UI.RentalProperty.Calculation
{
    public sealed class CalculationViewModel : NotificationObject
    {
        private readonly int _termInDays;
        private readonly PositionViewModel[] _resultOverTime;
        private string _title;

        public CalculationViewModel(int termInDays)
            : this(termInDays, DateTime.Today)
        {
        }

        public CalculationViewModel(int termInDays, DateTime startDate)
        {
            _termInDays = termInDays;
            _resultOverTime = new PositionViewModel[_termInDays];

            var currentDate = startDate;
            for (var i = 0; i < _termInDays; i++)
            {
                _resultOverTime[i] = new PositionViewModel(currentDate);
                currentDate = currentDate.AddDays(1);
            }
        }

        public PositionViewModel[] ResultOverTime
        {
            get { return _resultOverTime; }
        }

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

    }
}
