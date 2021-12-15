using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace QuickMusic3.Converters
{
    [ValueConversion(typeof(TimeSpan), typeof(double))]
    public class TimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            TimeSpan time = (TimeSpan)value;
            return (double)time.Ticks;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double ticks = (double)value;
            return TimeSpan.FromTicks((long)ticks);
        }
    }
}
