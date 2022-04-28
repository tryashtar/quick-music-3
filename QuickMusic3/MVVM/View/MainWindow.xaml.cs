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

    private BaseViewModel Model => (BaseViewModel)DataContext;

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
                {
                    var playlist = new DispatcherPlaylist(Application.Current.Dispatcher);
                    if (dialog.FileNames.Length == 1 && Path.GetExtension(dialog.FileName)!=".m3u")
                        playlist.AddSource(new FolderSource(Path.GetDirectoryName(dialog.FileName), SearchOption.TopDirectoryOnly, dialog.FileName));
                    else
                    {
                        foreach (var item in SongSourceExtensions.FromFileList(dialog.FileNames, SearchOption.TopDirectoryOnly))
                        {
                            playlist.AddSource(item);
                        }
                    }
                    Model.Shared.Player.Open(playlist);
                    Model.Shared.Player.Play();
                }
            }
        });
        NotifyIcon = (TaskbarIcon)FindResource("TaskbarIcon");
        NotifyIcon.Tag = this;
        NotifyIcon.DataContext = this.DataContext;
        NotifyIcon.LeftClickCommand = ShowWindowCommand;
        var top_right = (Panel)FindResource("PopupTopRight");
        ((Button)LogicalTreeHelper.FindLogicalNode(top_right, "PopupRestoreButton")).Command = ShowWindowCommand;
        ((Button)LogicalTreeHelper.FindLogicalNode(top_right, "PopupCloseButton")).Command = CloseWindowCommand;
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

        }
    }

    private void Window_Drop(object sender, DragEventArgs e)
    {
        string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
        bool holding_shift = e.KeyStates.HasFlag(DragDropKeyStates.ShiftKey);
        var search = holding_shift ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
        var playlist = new DispatcherPlaylist(Application.Current.Dispatcher);
        if (files.Length == 1 && File.Exists(files[0]) && Path.GetExtension(files[0]) != ".m3u")
            playlist.AddSource(new FolderSource(Path.GetDirectoryName(files[0]), search, files[0]));
        else
        {
            foreach (var item in SongSourceExtensions.FromFileList(files, search))
            {
                playlist.AddSource(item);
            }
        }
        Model.Shared.Player.Open(playlist);
        Model.Shared.Player.Play();
    }

    private void Window_DragOver(object sender, DragEventArgs e)
    {
        e.Effects = DragDropEffects.None;
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
            e.Effects = DragDropEffects.Copy;
        e.Handled = true;
    }
}
