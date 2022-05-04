using NAudio.Wave;
using QuickMusic3.MVVM.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TryashtarUtils.Utility;

namespace QuickMusic3.Converters;

public class MultiplyConverter : ParameterConverter<double, double, double>
{
    public override double Convert(double value, double parameter)
    {
        return value * parameter;
    }

    public static readonly MultiplyConverter Instance = new MultiplyConverter();
}

public class GreaterThanConverter : ParameterConverter<int, bool, int>
{
    public override bool Convert(int value, int parameter)
    {
        return value > parameter;
    }
}
