using System;
using System.Globalization;
using System.Windows.Data;

namespace ArtemisWest.PropertyInvestment.Calculator.Controls
{
    //TODO: Implement correctly.
    public sealed class CurrencyConverter : IValueConverter
    {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.Format(culture, "{0:c}", value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var strValue = (value ?? string.Empty).ToString();
            return Decimal.Parse(strValue, NumberStyles.Currency, culture);
        }

        #endregion
    }
}