using System;
using ArtemisWest.PropertyInvestment.Calculator.Entities;
using Microsoft.Practices.Prism.ViewModel;

namespace ArtemisWest.PropertyInvestment.Calculator.UI.RentalProperty.Calculation
{
    public sealed class CalculationViewModel : NotificationObject
    {
        private readonly int _termInDays;
        private readonly PositionViewModel[] _resultOverTime;
        private string _title;
        private bool _isDirty;

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
            private set
            {
                if (_title != value)
                {
                    _title = value;
                    RaisePropertyChanged(() => Title);
                }
            }
        }

        //As a method to stop people using two way binding. This should only be set by the parent.
        public void SetTitle(string newValue)
        {
            Title = newValue;
        }

        public bool IsDirty
        {
            get { return _isDirty; }
            set
            {
                if (_isDirty != value)
                {
                    _isDirty = value;
                    RaisePropertyChanged(() => IsDirty);
                }
            }
        }
    }
}
