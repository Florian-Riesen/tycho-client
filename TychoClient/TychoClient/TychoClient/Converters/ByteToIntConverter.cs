using System;
using System.Globalization;
using Xamarin.Forms;

namespace TychoClient.Converters
{
    public class ByteToIntConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            byte b = (byte)value;

            return (int)b;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int i = (int)value;
            return (byte)i;
        }
    }
}
