using Hardcodet.Wpf.TaskbarNotification;
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

namespace QuickMusic3;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window, INotifyPropertyChanged
{
    private readonly TaskbarIcon NotifyIcon;
    public event PropertyChangedEventHandler PropertyChanged;

    public ICommand HideWindowCommand { get; }
    public ICommand ShowWindowCommand { get; }
    public ICommand CloseWindowCommand { get; }

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
}
