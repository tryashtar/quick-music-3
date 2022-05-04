using Microsoft.Win32;
using NAudio.Wave;
using QuickMusic3.Converters;
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
using TryashtarUtils.Music;

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
    private BaseViewModel Model => (BaseViewModel)DataContext;

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
        this.DataContextChanged += MediaControls_DataContextChanged;

        var value = new Binding("Value") { ElementName = "TimeBar" };
        var total = new Binding("Maximum") { ElementName = "TimeBar" };
        ProgressBinding = new MultiBinding();
        ProgressBinding.Bindings.Add(value);
        ProgressBinding.Bindings.Add(total);
        ProgressBinding.Bindings.Add(new Binding() { Source = false });
        ProgressBinding.Converter = ProgressDivvy.Instance;
        RemainingBinding = new MultiBinding();
        RemainingBinding.Bindings.Add(value);
        RemainingBinding.Bindings.Add(total);
        RemainingBinding.Bindings.Add(new Binding() { Source = true });
        RemainingBinding.Converter = ProgressDivvy.Instance;
    }

    private void MediaControls_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue is BaseViewModel o)
            o.Shared.Player.PropertyChanged -= Player_PropertyChanged;
        if (e.NewValue is BaseViewModel b)
        {
            b.Shared.Player.PropertyChanged += Player_PropertyChanged;
            AddChapters();
        }
    }

    private void Player_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Player.CurrentTrack))
            AddChapters();
    }

    private readonly MultiBinding ProgressBinding;
    private readonly MultiBinding RemainingBinding;

    private void AddChapters()
    {
        ChapterBar.ColumnDefinitions.Clear();
        ChapterBar.Children.Clear();
        var chapters = ((BaseViewModel)this.DataContext).Shared.Player.CurrentTrack?.Metadata?.Item?.Chapters;
        if (chapters == null)
        {
            var left = new ColumnDefinition();
            var right = new ColumnDefinition();
            left.SetBinding(ColumnDefinition.WidthProperty, ProgressBinding);
            right.SetBinding(ColumnDefinition.WidthProperty, RemainingBinding);
            ChapterBar.ColumnDefinitions.Add(left);
            ChapterBar.ColumnDefinitions.Add(right);
            var left_shape = new Rectangle();
            var right_shape = new Rectangle();
            left_shape.SetBinding(Rectangle.FillProperty, "Shared.ActiveTheme.BarFilled");
            right_shape.SetBinding(Rectangle.FillProperty, "Shared.ActiveTheme.BarUnfilled");
            Grid.SetColumn(left_shape, 0);
            Grid.SetColumn(right_shape, 1);
            ChapterBar.Children.Add(left_shape);
            ChapterBar.Children.Add(right_shape);
        }
        else
        {
            for (int i = -1; i < chapters.Chapters.Count; i++)
            {
                TimeSpan start = i < 0 ? TimeSpan.Zero : chapters.Chapters[i].Time;
                TimeSpan end = (i < chapters.Chapters.Count - 1) ? chapters.Chapters[i + 1].Time : ((BaseViewModel)this.DataContext).Shared.Player.CurrentTrack.Metadata.Item.Duration;
                var length = end - start;
                var def = new ColumnDefinition() { Width = new GridLength(length.TotalMilliseconds, GridUnitType.Star) };
                ChapterBar.ColumnDefinitions.Add(def);
                var left_shape = new Rectangle();
                if (i % 2 == 0)
                    left_shape.Fill = Brushes.Red;
                else
                    left_shape.Fill = Brushes.DarkRed;
                ChapterBar.Children.Add(left_shape);
                Grid.SetColumn(left_shape, i + 1);
            }
        }
    }

    private void TimeBar_MouseDown(object sender, MouseButtonEventArgs e)
    {
        PlayDragging = Model.Shared.Player.PlayState == PlaybackState.Playing;
        Model.Shared.Player.Pause();
    }

    private void TimeBar_MouseUp(object sender, MouseButtonEventArgs e)
    {
        if (PlayDragging)
            Model.Shared.Player.Play();
        PlayDragging = false;
    }

    private void Volume_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (e.Delta != 0 && !Model.Shared.Player.Muted)
        {
            float volume = Model.Shared.Player.Volume + (1 / ((float)e.Delta / 3));
            Model.Shared.Player.Volume = Math.Clamp(volume, 0, 1);
        }
    }

    private void Shuffle_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        Model.Shared.Player.FreshShuffle();
    }
}
