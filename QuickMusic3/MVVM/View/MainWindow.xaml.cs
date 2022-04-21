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
using System.Windows.Shapes;
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

    private Theme active_theme;
    public Theme ActiveTheme
    {
        get { return active_theme; }
        private set { active_theme = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ActiveTheme))); }
    }

    public ICommand HideWindowCommand { get; }
    public ICommand ShowWindowCommand { get; }
    public ICommand CloseWindowCommand { get; }
    public ICommand ChangeThemeCommand { get; }

    public Visibility TrayIconVisibility
    {
        get => this.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
    }

    public MainWindow()
    {
        InitializeComponent();
        HideWindowCommand = new RelayCommand(() => { this.Visibility = Visibility.Collapsed; });
        ShowWindowCommand = new RelayCommand(() => { this.Visibility = Visibility.Visible; this.Activate(); NotifyIcon.TrayPopupResolved.IsOpen = false; });
        CloseWindowCommand = new RelayCommand(() => { this.Close(); NotifyIcon.TrayPopupResolved.IsOpen = false; });
        ChangeThemeCommand = new RelayCommand(() =>
        {
            Properties.Settings.Default.ThemeIndex++;
            if (Properties.Settings.Default.ThemeIndex >= Properties.Settings.Default.ImportedThemes.Count)
                Properties.Settings.Default.ThemeIndex = 0;
            OpenTheme(Properties.Settings.Default.ImportedThemes[Properties.Settings.Default.ThemeIndex]);
        });
        if (Properties.Settings.Default.ImportedThemes == null)
            Properties.Settings.Default.ImportedThemes = new();
        if (Properties.Settings.Default.ImportedThemes.Count > 0)
            OpenTheme(Properties.Settings.Default.ImportedThemes[Properties.Settings.Default.ThemeIndex]);
        NotifyIcon = (TaskbarIcon)FindResource("TaskbarIcon");
        NotifyIcon.Tag = this;
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

    private void OpenTheme(string path)
    {
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .Build();
        using var file = File.OpenRead(path);
        using var reader = new StreamReader(file);
        ActiveTheme = deserializer.Deserialize<Theme>(reader);
    }
}
