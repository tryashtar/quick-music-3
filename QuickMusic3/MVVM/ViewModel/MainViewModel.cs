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
// - error when playlist starts or becomes empty
// - AddResamples breaks if the stream gets unloaded and reloaded
// - too much memory usage!
// - eternal header disposal debug assert

// features to port:
// - real-time lyrics
// - remaining shortcuts
// - blank home view
// - more intelligent playlist building
// - fallback album art
// - show index and total in titlebar

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
