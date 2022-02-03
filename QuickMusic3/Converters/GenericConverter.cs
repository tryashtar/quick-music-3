using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace QuickMusic3.Converters
{
    public abstract class GenericConverter<TFrom, TTo> : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Convert((TFrom)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ConvertBack((TTo)value);
        }

        public abstract TTo Convert(TFrom value);
        public abstract TFrom ConvertBack(TTo value);
    }
}
