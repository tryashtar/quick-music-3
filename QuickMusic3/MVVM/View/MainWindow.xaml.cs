using Hardcodet.Wpf.TaskbarNotification;
using Microsoft.Win32;
using NAudio.Wave;
using QuickMusic3.Core;
using QuickMusic3.MVVM.Model;
using QuickMusic3.MVVM.View;
using QuickMusic3.MVVM.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace QuickMusic3.MVVM.View;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window, INotifyPropertyChanged
{
    private readonly TaskbarIcon NotifyIcon;
    public event PropertyChangedEventHandler PropertyChanged;

    public ICommand BrowseCommand { get; }
    public ICommand HideWindowCommand { get; }
    public ICommand ShowWindowCommand { get; }
    public ICommand CloseWindowCommand { get; }
    public ICommand MaximizeWindowCommand { get; }
    public ICommand MinimizeWindowCommand { get; }

    public Visibility TrayIconVisibility
    {
        get => this.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
    }

    private MainViewModel Model => (MainViewModel)DataContext;

    public MainWindow()
    {
        InitializeComponent();
        HideWindowCommand = new RelayCommand(() => { this.Visibility = Visibility.Collapsed; });
        ShowWindowCommand = new RelayCommand(() => { this.Visibility = Visibility.Visible; this.Activate(); NotifyIcon.TrayPopupResolved.IsOpen = false; });
        CloseWindowCommand = new RelayCommand(() => { this.Close(); NotifyIcon.TrayPopupResolved.IsOpen = false; });
        MaximizeWindowCommand = new RelayCommand(() =>
        {
            if (this.WindowState == WindowState.Maximized)
                this.WindowState = WindowState.Normal;
            else
                this.WindowState = WindowState.Maximized;
        });
        MinimizeWindowCommand = new RelayCommand(() => { this.WindowState = WindowState.Minimized; });
        BrowseCommand = new RelayCommand(() =>
        {
            var dialog = new OpenFileDialog();
            dialog.Multiselect = true;
            if (dialog.ShowDialog() == true)
            {
                if (Path.GetExtension(dialog.FileName) == ".yaml")
                {
                    Properties.Settings.Default.ImportedThemes.Add(dialog.FileName);
                    Model.Shared.OpenTheme(dialog.FileName);
                }
                else
                    OpenPlaylist(dialog.FileNames, SearchOption.TopDirectoryOnly);
            }
        });
        NotifyIcon = (TaskbarIcon)FindResource("TaskbarIcon");
        NotifyIcon.Tag = this;
        NotifyIcon.DataContext = this.DataContext;
        NotifyIcon.LeftClickCommand = ShowWindowCommand;
        var top_right = (Panel)FindResource("PopupTopRight");
        ((Button)LogicalTreeHelper.FindLogicalNode(top_right, "PopupRestoreButton")).Command = ShowWindowCommand;
        ((Button)LogicalTreeHelper.FindLogicalNode(top_right, "PopupCloseButton")).Command = CloseWindowCommand;
        PlaylistList = (ListView)FindResource("PlaylistList");
        Model.Shared.Player.PropertyChanged += Player_PropertyChanged;
        Model.PropertyChanged += Model_PropertyChanged;
        UpdateSize();
        var args = Environment.GetCommandLineArgs();
        if (args.Length > 1)
            OpenPlaylist(args.Skip(1), SearchOption.TopDirectoryOnly);
    }

    private void OpenPlaylist(IEnumerable<string> files, SearchOption search)
    {
        var playlist = new DispatcherPlaylist(Dispatcher);
        var sources = SongSourceExtensions.FromFileList(files, search, true);
        foreach (var item in sources.sources)
        {
            playlist.AddSource(item);
        }
        if (playlist.Count > 0)
        {
            Model.Shared.Player.Open(playlist, sources.first_index);
            Model.Shared.Player.Play();
            Model.GoToDefaultView();
        }
    }

    private bool UpdatingSize = false;
    private void UpdateSize()
    {
        UpdatingSize = true;
        if (Model.ActiveViewModel is PlaylistViewModel)
        {
            this.Width = Properties.Settings.Default.PlaylistWidth;
            this.Height = Properties.Settings.Default.PlaylistHeight;
        }
        else if (Model.ActiveViewModel is NowPlayingViewModel)
        {
            this.Width = Properties.Settings.Default.NowPlayingWidth;
            this.Height = Properties.Settings.Default.NowPlayingHeight;
        }
        UpdatingSize = false;
    }

    private void Model_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainViewModel.ActiveViewModel))
            UpdateSize();
    }

    private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (UpdatingSize || this.WindowState == WindowState.Maximized)
            return;
        if (Model.ActiveViewModel is PlaylistViewModel)
        {
            Properties.Settings.Default.PlaylistWidth = this.Width;
            Properties.Settings.Default.PlaylistHeight = this.Height;
        }
        else if (Model.ActiveViewModel is NowPlayingViewModel)
        {
            Properties.Settings.Default.NowPlayingWidth = this.Width;
            Properties.Settings.Default.NowPlayingHeight = this.Height;
        }
    }

    private readonly ListView PlaylistList;

    private int LastKnownPosition;
    private SongFile LastKnownTrack;
    private void Player_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Player.CurrentTrack))
        {
            Dispatcher.BeginInvoke(() =>
            {
                PlaylistList.ScrollIntoView(Model.Shared.Player.CurrentTrack);
                LastKnownPosition = Model.Shared.Player.PlaylistPosition;
                LastKnownTrack = Model.Shared.Player.CurrentTrack;
            });
        }
        else if (e.PropertyName == nameof(Player.PlaylistPosition))
        {
            Dispatcher.BeginInvoke(() =>
            {
                if (Model.Shared.Player.CurrentTrack == LastKnownTrack)
                {
                    int pos = Model.Shared.Player.PlaylistPosition;
                    var scroller = (ScrollViewer)((Decorator)VisualTreeHelper.GetChild(PlaylistList, 0)).Child;
                    if (Math.Abs(LastKnownPosition - pos) > 20)
                        PlaylistList.ScrollIntoView(Model.Shared.Player.CurrentTrack);
                    else if (LastKnownPosition > pos)
                    {
                        for (int i = 0; i < LastKnownPosition - pos; i++)
                        {
                            scroller.LineUp();
                        }
                    }
                    else if (LastKnownPosition < pos)
                    {
                        for (int i = 0; i < pos - LastKnownPosition; i++)
                        {
                            scroller.LineDown();
                        }
                    }
                }
                LastKnownPosition = Model.Shared.Player.PlaylistPosition;
            });
        }
    }

    private void Window_Closed(object sender, EventArgs e)
    {
        Properties.Settings.Default.Save();
        NotifyIcon.Dispose();
    }

    private void Window_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TrayIconVisibility)));
    }

    private void PlaylistItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
        {
            Model.Shared.Player.SwitchTo((SongFile)((ListViewItem)sender).Content);
            Model.Shared.Player.Play();
        }
    }

    private void Window_Drop(object sender, DragEventArgs e)
    {
        string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
        bool holding_shift = e.KeyStates.HasFlag(DragDropKeyStates.ShiftKey);
        var search = holding_shift ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
        OpenPlaylist(files, search);
    }

    private void Window_DragOver(object sender, DragEventArgs e)
    {
        e.Effects = DragDropEffects.None;
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
            e.Effects = DragDropEffects.Copy;
        e.Handled = true;
    }

    private void PlaylistList_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        ListView listView = sender as ListView;
        GridView gView = listView.View as GridView;

        var workingWidth = listView.ActualWidth - SystemParameters.VerticalScrollBarWidth - gView.Columns[0].ActualWidth - gView.Columns[4].ActualWidth;
        gView.Columns[1].Width = workingWidth * 0.5;
        gView.Columns[2].Width = workingWidth * 0.25;
        gView.Columns[3].Width = workingWidth * 0.25;
    }
}
