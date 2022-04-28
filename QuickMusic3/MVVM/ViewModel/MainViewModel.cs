using NAudio.Wave;
using QuickMusic3.Core;
using QuickMusic3.MVVM.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace QuickMusic3.MVVM.ViewModel;

// current bugs:
// - we need to fully sort the folder before Upcoming can be truly accurate

// features to port:
// - real-time lyrics
// - remaining shortcuts
// - drag and drop
// - playlist view
// - blank home view
// - more intelligent playlist building
// - themes (data driven!)
// - fallback album art
// - change size when ActiveViewModel changes, save to properties
// - show index and total in titlebar
// - ensure row visible when item changes
// - double-click to change tracks

public class MainViewModel : BaseViewModel
{
    private readonly NowPlayingViewModel NowPlayingVM;
    private readonly PlaylistViewModel PlaylistVM;
    private BaseViewModel active_view_model;
    public BaseViewModel ActiveViewModel
    {
        get { return active_view_model; }
        set { active_view_model = value; OnPropertyChanged(); }
    }
    public ICommand ChangeViewCommand { get; }

    public override SharedState Shared { get; }

    public MainViewModel()
    {
        Shared = new();
        NowPlayingVM = new(Shared);
        PlaylistVM = new(Shared);
        active_view_model = NowPlayingVM;
        ChangeViewCommand = new RelayCommand(() =>
        {
            if (ActiveViewModel == NowPlayingVM)
                ActiveViewModel = PlaylistVM;
            else
                ActiveViewModel = NowPlayingVM;
        });
    }
}
