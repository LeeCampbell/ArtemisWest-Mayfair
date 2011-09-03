using System;
using System.Windows.Data;

namespace ArtemisWest.PropertyInvestment.Calculator.Controls
{
    public sealed class DivisionConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double numerator = ConvertToDouble(value, culture);
            double denominator = ConvertToDouble(parameter, culture);

            return numerator / denominator;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion

        private static double ConvertToDouble(object value, IFormatProvider culture)
        {
            var result = default(double);
            try
            {
                var source = value as IConvertible;
                if (source != null)
                    result = source.ToDouble(culture);
            }
            catch
            {
            }
            return result;
        }
    }
}