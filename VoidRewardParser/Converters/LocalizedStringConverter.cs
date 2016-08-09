using System;
using System.Globalization;
using System.Windows.Data;
using VoidRewardParser.Logic;

namespace VoidRewardParser.Converters
{
    public class LocalizedStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return LocalizationManager.Localize(value as string);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
