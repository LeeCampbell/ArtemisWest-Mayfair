using System;
using System.Globalization;
using System.Windows.Data;

namespace ArtemisWest.PropertyInvestment.Calculator.Controls
{
    //TODO: Do this correctly
    public sealed class PercentageConverter : IValueConverter
    {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.Format("{0:p2}", value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var strValue = (value ?? string.Empty).ToString();
            strValue = strValue.Replace("%", string.Empty);
            decimal percentage = Decimal.Parse(strValue, NumberStyles.Any, CultureInfo.CurrentCulture);

            return percentage / 100m;
        }

        #endregion
    }
}
