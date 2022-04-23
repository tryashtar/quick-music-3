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
    private readonly FileSystemWatcher ThemeWatcher;

    private Theme active_theme;
    public Theme ActiveTheme
    {
        get { return active_theme; }
        private set { active_theme = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ActiveTheme))); }
    }

    public ICommand BrowseCommand { get; }
    public ICommand HideWindowCommand { get; }
    public ICommand ShowWindowCommand { get; }
    public ICommand CloseWindowCommand { get; }
    public ICommand MaximizeWindowCommand { get; }
    public ICommand MinimizeWindowCommand { get; }
    public ICommand ChangeThemeCommand { get; }

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
        ChangeThemeCommand = new RelayCommand(() =>
        {
            var all_themes = Theme.DefaultThemes.Keys.Concat(Properties.Settings.Default.ImportedThemes).ToList();
            int index = all_themes.IndexOf(Properties.Settings.Default.SelectedTheme);
            index++;
            if (index >= all_themes.Count)
                index = 0;
            OpenTheme(all_themes[index]);
        });
        BrowseCommand = new RelayCommand(() =>
        {
            var dialog = new OpenFileDialog();
            dialog.Multiselect = true;
            if (dialog.ShowDialog() == true)
            {
                if (Path.GetExtension(dialog.FileName) == ".yaml")
                {
                    Properties.Settings.Default.ImportedThemes.Add(dialog.FileName);
                }
                else
                {
                    Model.Player.OpenFiles(Playlist.LoadFiles(dialog.FileNames));
                    Model.Player.Play();
                }
            }
        });
        ThemeWatcher = new();
        ThemeWatcher.Changed += (s, e) => Dispatcher.Invoke(() => OpenTheme(Properties.Settings.Default.SelectedTheme));
        if (Properties.Settings.Default.ImportedThemes == null)
            Properties.Settings.Default.ImportedThemes = new();
        if (String.IsNullOrEmpty(Properties.Settings.Default.SelectedTheme))
            Properties.Settings.Default.SelectedTheme = Theme.DefaultThemes.Keys.First();
        OpenTheme(Properties.Settings.Default.SelectedTheme);
        NotifyIcon = (TaskbarIcon)FindResource("TaskbarIcon");
        NotifyIcon.Tag = this;
        ((FrameworkElement)NotifyIcon.TrayPopup).Tag = this;
        NotifyIcon.LeftClickCommand = ShowWindowCommand;
        var top_right = (Panel)FindResource("IconTopRight");
        ((Button)LogicalTreeHelper.FindLogicalNode(top_right, "ShowButton")).Command = ShowWindowCommand;
        ((Button)LogicalTreeHelper.FindLogicalNode(top_right, "CloseButton")).Command = CloseWindowCommand;
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

    private void OpenTheme(string theme)
    {
        Properties.Settings.Default.SelectedTheme = theme;
        if (Theme.DefaultThemes.TryGetValue(theme, out var def))
        {
            ActiveTheme = def;
            ThemeWatcher.EnableRaisingEvents = false;
            return;
        }
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .Build();
        try
        {
            using var file = File.OpenText(theme);
            ActiveTheme = deserializer.Deserialize<Theme>(file);
        }
        catch (FileNotFoundException ex)
        {
            ThemeWatcher.EnableRaisingEvents = false;
            Properties.Settings.Default.ImportedThemes.Remove(theme);
            return;
        }
        catch
        {
            ThemeWatcher.EnableRaisingEvents = false;
            return;
        }
        ThemeWatcher.Path = Path.GetDirectoryName(theme);
        ThemeWatcher.Filter = Path.GetFileName(theme);
        ThemeWatcher.EnableRaisingEvents = true;
    }
}
