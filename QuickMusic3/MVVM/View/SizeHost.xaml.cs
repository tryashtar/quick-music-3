using Microsoft.Win32;
using NAudio.Wave;
using QuickMusic3.Core;
using QuickMusic3.MVVM.Model;
using QuickMusic3.MVVM.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace QuickMusic3.MVVM.View;

public partial class SizeHost : ContentControl
{
    public static readonly DependencyProperty ButtonSizeProperty =
            DependencyProperty.Register("ButtonSize", typeof(Size),
            typeof(SizeHost), new FrameworkPropertyMetadata(new Size(40, 40)));
    public Size ButtonSize
    {
        get { return (Size)GetValue(ButtonSizeProperty); }
        set { SetValue(ButtonSizeProperty, value); }
    }

    public SizeHost()
    {
        InitializeComponent();
    }
}
