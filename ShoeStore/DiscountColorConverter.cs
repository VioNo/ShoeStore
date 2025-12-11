using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows;


namespace ShoeStore
{
    public class DiscountColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double discount && discount > 0)
            {
                if (discount > 15)
                {
                    return new SolidColorBrush(Color.FromRgb(76, 175, 80));
                }
                else
                {
                    return new SolidColorBrush(Color.FromRgb(255, 82, 82));
                }
            }
            return Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}