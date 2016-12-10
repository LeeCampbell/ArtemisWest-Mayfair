using System;
using ArtemisWest.PropertyInvestment.Calculator.Entities;
using Microsoft.Practices.Prism.Mvvm;

namespace ArtemisWest.PropertyInvestment.Calculator.UI.RentalProperty.Calculation
{
    public sealed class CalculationViewModel : BindableBase
    {
        private string _title;
        private bool _isDirty;

        public CalculationViewModel(int termInDays)
            : this(termInDays, DateTime.Today)
        {
        }

        public CalculationViewModel(int termInDays, DateTime startDate)
        {
            var termInDays1 = termInDays;
            ResultOverTime = new PositionViewModel[termInDays1];

            var currentDate = startDate;
            for (var i = 0; i < termInDays1; i++)
            {
                ResultOverTime[i] = new PositionViewModel(currentDate);
                currentDate = currentDate.AddDays(1);
            }
        }

        public PositionViewModel[] ResultOverTime { get; }

        public string Title
        {
            get { return _title; }
            private set
            {
                SetProperty(ref _title, value);
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
                SetProperty(ref _isDirty, value);
            }
        }
    }
}
