using System;
using System.Globalization;
using Xamarin.Forms;

namespace TychoClient.Converters
{
    public class IsEmptyOrNullConverter : IValueConverter
    {
        public bool Invert { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool ret = false;
            if (value is null)
                ret = true;
            else if (value is string s && string.IsNullOrEmpty(s))
                ret = true;
            return Invert ? !ret : ret;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
