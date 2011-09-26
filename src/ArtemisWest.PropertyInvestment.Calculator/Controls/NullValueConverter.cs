using System;
using System.Globalization;
using System.Windows.Data;

namespace ArtemisWest.PropertyInvestment.Calculator.Controls
{
    public sealed class NullValueConverter : IValueConverter
    {
        public static readonly NullValueConverter Instance = new NullValueConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}