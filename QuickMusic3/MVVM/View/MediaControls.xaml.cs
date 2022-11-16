using Microsoft.Win32;
using NAudio.Wave;
using QuickMusic3.Converters;
using QuickMusic3.Core;
using QuickMusic3.MVVM.Model;
using QuickMusic3.MVVM.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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

    public event PropertyChangedEventHandler? PropertyChanged;
    public BaseViewModel Model => (BaseViewModel)DataContext;

    private bool playdragging;
    public bool PlayDragging
    {
        get { return playdragging; }
        set { playdragging = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PlayDragging))); }
    }

    private readonly StackPanel ProgressGrid;
    private readonly StackPanel RemainingGrid;

    private NestedListener<SongFile>? Listener;
    public MediaControls()
    {
        InitializeComponent();
        TimeBar.AddHandler(Slider.PreviewMouseLeftButtonDownEvent, new MouseButtonEventHandler(TimeBar_MouseDown), true);
        TimeBar.AddHandler(Slider.PreviewMouseLeftButtonUpEvent, new MouseButtonEventHandler(TimeBar_MouseUp), true);
        ProgressGrid = (StackPanel)FindResource("ProgressGrid");
        RemainingGrid = (StackPanel)FindResource("RemainingGrid");
        this.DataContextChanged += (s, e) =>
        {
            if (Listener != null)
                Listener.Changed -= Listener_Changed;
            if (DataContext is BaseViewModel)
            {
                Listener = new NestedListener<SongFile>(Model.Shared.Player, nameof(Player.Stream), nameof(PlaylistStream.CurrentTrack));
                Listener.Changed += Listener_Changed;
            }
        };
    }

    private void Listener_Changed(object? sender, SongFile e)
    {
        Dispatcher.BeginInvoke(() => AddChapters());
    }

    private void AddChapters()
    {
        var track = ((BaseViewModel)this.DataContext).Shared.Player.Stream.CurrentTrack;
        if (track == null)
            return;
        var chapters = track.Metadata?.Item?.Chapters;
        ProgressGrid.Children.Clear();
        RemainingGrid.Children.Clear();
        if (chapters != null)
        {
            var duration = ((BaseViewModel)this.DataContext).Shared.Player.TotalTime;
            for (int i = -1; i < chapters.Chapters.Count; i++)
            {
                var length = chapters.Chapters[i].End - chapters.Chapters[i].Start;
                var proportion = length / duration;
                var left_bar = new Border() { BorderThickness = new(1, 0, 1, 0) };
                var right_bar = new Border() { BorderThickness = new(1, 0, 1, 0) };
                var binding = new Binding("ActualWidth") { Source = TimeBar, Converter = MultiplyConverter.Instance, ConverterParameter = proportion };
                left_bar.SetBinding(Border.BorderBrushProperty, "Shared.ActiveTheme.Background");
                right_bar.SetBinding(Border.BorderBrushProperty, "Shared.ActiveTheme.Background");
                left_bar.SetBinding(Border.WidthProperty, binding);
                right_bar.SetBinding(Border.WidthProperty, binding);
                ProgressGrid.Children.Add(left_bar);
                RemainingGrid.Children.Add(right_bar);
            }
        }
    }

    private void TimeBar_MouseDown(object? sender, MouseButtonEventArgs e)
    {
        PlayDragging = Model.Shared.Player.PlayState == PlaybackState.Playing;
        Model.Shared.Player.Pause();
    }

    private void TimeBar_MouseUp(object? sender, MouseButtonEventArgs e)
    {
        if (PlayDragging)
            Model.Shared.Player.Play();
        PlayDragging = false;
    }

    private void Volume_PreviewMouseWheel(object? sender, MouseWheelEventArgs e)
    {
        if (e.Delta != 0 && !Model.Shared.Player.Muted)
        {
            float volume = Model.Shared.Player.Volume + (1 / ((float)e.Delta / 3));
            Model.Shared.Player.Volume = Math.Clamp(volume, 0, 1);
        }
    }

    private async void Shuffle_MouseRightButtonDown(object? sender, MouseButtonEventArgs e)
    {
        await Model.Shared.Player.FreshShuffleAsync();
    }
}
