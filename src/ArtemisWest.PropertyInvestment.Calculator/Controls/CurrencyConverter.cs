using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Data;

namespace ArtemisWest.PropertyInvestment.Calculator.Controls
{
    //TODO: Use ArtemisWest.Presentation.Controls.CurrencyConverter
    public sealed class CurrencyConverter : IValueConverter
    {
        private string _format;
        private int _precision;

        public CurrencyConverter()
        {
            Precision = 0;
        }
        
        public int Precision
        {
            get { return _precision; }
            set
            {
                _precision = value;
                _format = @"{0:c" + _precision + @"}";
            }
        }

        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //return string.Format(culture, _format, value);
            return string.Format(CultureInfo.CurrentCulture, _format, value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var strValue = (value ?? string.Empty).ToString();
            //return Decimal.Parse(strValue, NumberStyles.Currency, culture);
            return Decimal.Parse(strValue, NumberStyles.Currency, CultureInfo.CurrentCulture);
        }

        #endregion
    }
}