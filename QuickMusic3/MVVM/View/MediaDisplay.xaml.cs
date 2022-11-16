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
using System.Windows.Threading;
using TryashtarUtils.Music;

namespace QuickMusic3.MVVM.View;

public partial class MediaDisplay : UserControl
{
    public static readonly DependencyProperty MetadataFontSizeProperty =
            DependencyProperty.Register(nameof(MetadataFontSize), typeof(double),
            typeof(MediaDisplay), new FrameworkPropertyMetadata(30d));
    public static readonly DependencyProperty TopRightProperty =
            DependencyProperty.Register(nameof(TopRight), typeof(FrameworkElement),
            typeof(MediaDisplay), new FrameworkPropertyMetadata());
    public static readonly DependencyProperty MediaControlsProperty =
            DependencyProperty.Register(nameof(MediaControls), typeof(MediaControls),
            typeof(MediaDisplay), new FrameworkPropertyMetadata());
    public static readonly DependencyProperty LyricsEnabledProperty =
            DependencyProperty.Register(nameof(LyricsEnabled), typeof(bool),
            typeof(MediaDisplay), new FrameworkPropertyMetadata(true));
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
    public bool LyricsEnabled
    {
        get { return (bool)GetValue(LyricsEnabledProperty); }
        set { SetValue(LyricsEnabledProperty, value); }
    }

    public MediaDisplay()
    {
        InitializeComponent();
        this.DataContextChanged += MediaDisplay_DataContextChanged;
    }

    private void MediaDisplay_DataContextChanged(object? sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue is BaseViewModel o)
            o.Shared.Player.PropertyChanged -= Player_PropertyChanged;
        if (e.NewValue is BaseViewModel b)
        {
            b.Shared.Player.PropertyChanged += Player_PropertyChanged;
            LastLineIndex = -1;
            Dispatcher.BeginInvoke(() => ScrollLine(), DispatcherPriority.Background);
            Dispatcher.BeginInvoke(() => LyricsScroller.ScrollToTop(), DispatcherPriority.Background);
        }
    }

    private void Player_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Player.CurrentLine))
            Dispatcher.BeginInvoke(() => ScrollLine(), DispatcherPriority.Background);
        else if (e.PropertyName == nameof(Player.CurrentTrack))
        {
            LastLineIndex = -1;
            Dispatcher.BeginInvoke(() => LyricsScroller.ScrollToTop(), DispatcherPriority.Background);
        }
    }

    private int LastLineIndex = -1;
    private void ScrollLine()
    {
        if (this.DataContext == null)
            return;
        var current = ((BaseViewModel)this.DataContext).Shared.Player.CurrentLine;
        var element = (FrameworkElement)LyricsBox.ItemContainerGenerator.ContainerFromItem(current);
        if (element != null)
        {
            int index = ((BaseViewModel)this.DataContext).Shared.Player.CurrentTrack.Metadata.Item.Lyrics.Lines.IndexOf(current);
            if (LastLineIndex != -1 && index != -1 && index > LastLineIndex)
            {
                for (int i = 0; i < index - LastLineIndex; i++)
                {
                    LyricsScroller.LineDown();
                }
            }
            else if (LastLineIndex != -1 && index != -1 && index < LastLineIndex)
            {
                for (int i = 0; i < LastLineIndex - index; i++)
                {
                    LyricsScroller.LineUp();
                }
            }
            element.BringIntoView();
            LastLineIndex = index;
        }
    }

    private void Lyric_MouseDown(object? sender, MouseButtonEventArgs e)
    {
        var player = ((BaseViewModel)this.DataContext).Shared.Player;
        if (player.CurrentTrack.Metadata.Item.Lyrics.Synchronized)
        {
            var entry = (LyricsEntry)((FrameworkElement)sender).DataContext;
            player.CurrentTime = entry.Time;
            LastLineIndex = player.CurrentTrack.Metadata.Item.Lyrics.Lines.IndexOf(entry);
        }
    }
}
