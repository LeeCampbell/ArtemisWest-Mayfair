using System;
using System.Windows;

namespace ArtemisWest.PropertyInvestment.Calculator.Entities
{
    public class PositionViewModel : DependencyObject
    {
        private readonly DateTime _date;

        public event EventHandler ValueChanged;

        public PositionViewModel(DateTime date)
        {
            _date = date;
        }

        public DateTime Date
        {
            get { return _date; }
        }

        #region Value
        public decimal Value
        {
            get { return (decimal)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", 
                                        typeof(decimal), 
                                        typeof(PositionViewModel), 
                                        new UIPropertyMetadata(OnValueChanged));
        private static void OnValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var self = (PositionViewModel)obj;
            self.InvokeValueChanged(EventArgs.Empty);
        }
        #endregion

        public void Update(Position source)
        {
            SetValues(source, this);
        }

        #region Private members

        private static void SetValues(Position source, PositionViewModel target)
        {
            if (source.Date != target.Date)
            {
                throw new InvalidOperationException();
            }
            target.Value = source.Value;
        }

        void InvokeValueChanged(EventArgs e)
        {
            EventHandler handler = ValueChanged;
            if (handler != null) handler(this, e);
        }
        #endregion
    }
}
