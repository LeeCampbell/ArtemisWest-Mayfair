using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Data;

namespace ArtemisWest.Mayfair.Shell.Styles
{
    public class DebugConverter : IValueConverter
    {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Debug.WriteLine("DebugConverter.Convert({0}, {1}, {2}, {3})", value, targetType, parameter, culture);
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Debug.WriteLine("DebugConverter.ConvertBack({0}, {1}, {2}, {3})", value, targetType, parameter, culture);
            return value;
        }

        #endregion
    }
}
