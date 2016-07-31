using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using VoidRewardParser.Entities;

namespace VoidRewardParser.Converters
{
    public class RarityToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch ((Rarity)value)
            {
                case Rarity.Common:
                    return new SolidColorBrush(Colors.White);
                case Rarity.Uncommon:
                    return new SolidColorBrush(Colors.SkyBlue);
                case Rarity.Rare:
                    return new SolidColorBrush(Colors.Goldenrod);
            }
            return new SolidColorBrush(Colors.White);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
