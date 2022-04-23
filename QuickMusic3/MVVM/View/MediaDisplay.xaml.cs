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

public partial class MediaDisplay : UserControl
{
    public static readonly DependencyProperty MetadataFontSizeProperty =
            DependencyProperty.Register("MetadataFontSize", typeof(double),
            typeof(MediaDisplay), new FrameworkPropertyMetadata(30d));
    public static readonly DependencyProperty TopRightProperty =
            DependencyProperty.Register("TopRight", typeof(FrameworkElement),
            typeof(MediaDisplay), new FrameworkPropertyMetadata());
    public static readonly DependencyProperty MediaControlsProperty =
            DependencyProperty.Register("MediaControls", typeof(MediaControls),
            typeof(MediaDisplay), new FrameworkPropertyMetadata());
    public double MetadataFontSize
    {
        get { return (double)GetValue(MetadataFontSizeProperty); }
        set { SetValue(MetadataFontSizeProperty, value); }
    }
    public FrameworkElement TopRight
    {
        get { return (FrameworkElement)GetValue(TopRightProperty); }
        set { SetValue(TopRightProperty, value); }
    }
    public MediaControls MediaControls
    {
        get { return (MediaControls)GetValue(MediaControlsProperty); }
        set { SetValue(MediaControlsProperty, value); }
    }

    public MediaDisplay()
    {
        InitializeComponent();
    }
}
