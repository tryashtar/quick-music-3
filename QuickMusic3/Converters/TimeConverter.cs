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
    public class TimeConverter : GenericConverter<TimeSpan, double>
    {
        public override double Convert(TimeSpan value)
        {
            return (double)value.Ticks;
        }

        public override TimeSpan ConvertBack(double value)
        {
            return TimeSpan.FromTicks((long)value);
        }
    }
}
