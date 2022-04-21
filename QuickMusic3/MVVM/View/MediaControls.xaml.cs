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
/// <summary>
/// Interaction logic for MediaControls.xaml
/// </summary>
public partial class MediaControls : UserControl, INotifyPropertyChanged
{
    public static readonly DependencyProperty ButtonSizeProperty =
            DependencyProperty.Register("ButtonSize", typeof(Size),
            typeof(MediaControls), new FrameworkPropertyMetadata(new Size(40, 40)));
    public static readonly DependencyProperty BrowseVisibilityProperty =
            DependencyProperty.Register("BrowseVisibility", typeof(Visibility),
            typeof(MediaControls), new FrameworkPropertyMetadata(Visibility.Visible));
    public static readonly DependencyProperty VolumeWidthProperty =
            DependencyProperty.Register("VolumeWidth", typeof(double),
            typeof(MediaControls), new FrameworkPropertyMetadata(100d));
    public static readonly DependencyProperty MetadataFontSizeProperty =
            DependencyProperty.Register("MetadataFontSize", typeof(double),
            typeof(MediaControls), new FrameworkPropertyMetadata(30d));
    public static readonly DependencyProperty TopRightProperty =
            DependencyProperty.Register("TopRight", typeof(FrameworkElement),
            typeof(MediaControls), new FrameworkPropertyMetadata());
    public static readonly DependencyProperty ThemeProperty =
            DependencyProperty.Register("Theme", typeof(Theme),
            typeof(MediaControls), new FrameworkPropertyMetadata());
    public Size ButtonSize
    {
        get { return (Size)GetValue(ButtonSizeProperty); }
        set { SetValue(ButtonSizeProperty, value); }
    }
    public Visibility BrowseVisibility
    {
        get { return (Visibility)GetValue(BrowseVisibilityProperty); }
        set { SetValue(BrowseVisibilityProperty, value); }
    }
    public double VolumeWidth
    {
        get { return (double)GetValue(VolumeWidthProperty); }
        set { SetValue(VolumeWidthProperty, value); }
    }
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
    public Theme Theme
    {
        get { return (Theme)GetValue(ThemeProperty); }
        set { SetValue(ThemeProperty, value); }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    private MainViewModel Model => (MainViewModel)DataContext;

    private bool playdragging;
    public bool PlayDragging
    {
        get { return playdragging; }
        set { playdragging = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PlayDragging))); }
    }

    public ICommand BrowseCommand { get; }

    public MediaControls()
    {
        InitializeComponent();
        TimeBar.AddHandler(Slider.PreviewMouseLeftButtonDownEvent, new MouseButtonEventHandler(TimeBar_MouseDown), true);
        TimeBar.AddHandler(Slider.PreviewMouseLeftButtonUpEvent, new MouseButtonEventHandler(TimeBar_MouseUp), true);
        BrowseCommand = new RelayCommand(() =>
        {
            var dialog = new OpenFileDialog();
            dialog.Multiselect = true;
            if (dialog.ShowDialog() == true)
            {
                Model.Player.OpenFiles(Playlist.LoadFiles(dialog.FileNames));
                Model.Player.Play();
            }
        });
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
