using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace VideoClient.Converter
{
    public class InverseBooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                var boolValue = (bool)value;
                if (!boolValue) return Visibility.Visible;
                else return Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (((Visibility)value).Equals(Visibility.Collapsed)) return true;
                else return false;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
