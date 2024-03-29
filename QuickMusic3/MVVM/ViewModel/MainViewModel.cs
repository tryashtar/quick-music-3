﻿using NAudio.Wave;
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
// - error when playlist starts or becomes empty
// - too much memory usage!
// - text wraps too early when small ... once this is fixed, add max height to popup

// features to port:
// - more intelligent playlist building

public class MainViewModel : BaseViewModel
{
    private readonly HomeViewModel HomeVM;
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
        HomeVM = new(Shared);
        NowPlayingVM = new(Shared);
        PlaylistVM = new(Shared);
        active_view_model = HomeVM;
        ChangeViewCommand = new RelayCommand(() =>
        {
            // can't change from home
            if (ActiveViewModel == NowPlayingVM)
            {
                ActiveViewModel = PlaylistVM;
                Properties.Settings.Default.DefaultView = 1;
            }
            else if (ActiveViewModel == PlaylistVM)
            {
                ActiveViewModel = NowPlayingVM;
                Properties.Settings.Default.DefaultView = 0;
            }
        });
    }

    public void GoToDefaultView()
    {
        if (Properties.Settings.Default.DefaultView == 1)
            ActiveViewModel = PlaylistVM;
        else
            ActiveViewModel = NowPlayingVM;
    }
}
