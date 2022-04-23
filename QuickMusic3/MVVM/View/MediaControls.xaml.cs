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

public partial class MediaControls : UserControl, INotifyPropertyChanged
{
    public static readonly DependencyProperty VolumeWidthProperty =
            DependencyProperty.Register("VolumeWidth", typeof(double),
            typeof(MediaControls), new FrameworkPropertyMetadata(100d));
    public static readonly DependencyProperty BottomRightProperty =
            DependencyProperty.Register("BottomRight", typeof(FrameworkElement),
            typeof(MediaControls), new FrameworkPropertyMetadata());
    public double VolumeWidth
    {
        get { return (double)GetValue(VolumeWidthProperty); }
        set { SetValue(VolumeWidthProperty, value); }
    }
    public FrameworkElement BottomRight
    {
        get { return (FrameworkElement)GetValue(BottomRightProperty); }
        set { SetValue(BottomRightProperty, value); }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    private MainViewModel Model => (MainViewModel)DataContext;

    private bool playdragging;
    public bool PlayDragging
    {
        get { return playdragging; }
        set { playdragging = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PlayDragging))); }
    }

    public MediaControls()
    {
        InitializeComponent();
        TimeBar.AddHandler(Slider.PreviewMouseLeftButtonDownEvent, new MouseButtonEventHandler(TimeBar_MouseDown), true);
        TimeBar.AddHandler(Slider.PreviewMouseLeftButtonUpEvent, new MouseButtonEventHandler(TimeBar_MouseUp), true);
    }

    private void TimeBar_MouseDown(object sender, MouseButtonEventArgs e)
    {
        PlayDragging = Model.Player.PlayState == PlaybackState.Playing;
        Model.Player.Pause();
    }

    private void TimeBar_MouseUp(object sender, MouseButtonEventArgs e)
    {
        if (PlayDragging)
            Model.Player.Play();
        PlayDragging = false;
    }

    private void Volume_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (e.Delta != 0 && !Model.Player.Muted)
        {
            float volume = Model.Player.Volume + (1 / ((float)e.Delta / 3));
            Model.Player.Volume = Math.Clamp(volume, 0, 1);
        }
    }
}
