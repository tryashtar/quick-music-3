﻿using NAudio.Wave;
using QuickMusic3.Core;
using QuickMusic3.MVVM.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace QuickMusic3.MVVM.ViewModel;

// current bugs:
// down button isn't CustomButton (both it and Browse should be owned by Window)

// features to port:
// - real-time lyrics
// - remaining shortcuts
// - drag and drop
// - playlist view
// - blank home view
// - more intelligent playlist building
// - themes (data driven!)
// - fallback album art

public class MainViewModel
{
    public Player Player { get; } = new();

    public ICommand PlayPauseCommand { get; }
    public ICommand NextCommand { get; }
    public ICommand PrevCommand { get; }
    public ICommand ChangeRepeatCommand { get; }
    public ICommand ChangeMuteCommand { get; }
    public ICommand ChangeShuffleCommand { get; }
    public ICommand ChangeVolumeCommand { get; }
    public ICommand SeekCommand { get; }

    public MainViewModel()
    {
        PlayPauseCommand = new RelayCommand(() =>
        {
            if (Player.PlayState == PlaybackState.Playing)
                Player.Pause();
            else
                Player.Play();
        });
        NextCommand = new RelayCommand(() => Player.Next());
        PrevCommand = new RelayCommand(() =>
        {
            if (Player.CurrentTime > TimeSpan.FromSeconds(2))
                Player.CurrentTime = TimeSpan.Zero;
            else
                Player.Prev();
        });
        ChangeRepeatCommand = new RelayCommand(() =>
        {
            if (Player.RepeatMode == RepeatMode.RepeatAll)
                Player.RepeatMode = RepeatMode.RepeatOne;
            else if (Player.RepeatMode == RepeatMode.RepeatOne)
                Player.RepeatMode = RepeatMode.PlayAll;
            else
                Player.RepeatMode = RepeatMode.RepeatAll;
        });
        ChangeMuteCommand = new RelayCommand(() => { Player.Muted = !Player.Muted; });
        ChangeShuffleCommand = new RelayCommand(() => { Player.Shuffle = !Player.Shuffle; });
        ChangeVolumeCommand = new RelayCommand<float>(n => { Player.Volume = Math.Clamp(Player.Volume + n, 0, 1); });
        SeekCommand = new RelayCommand<double>(n => Player.CurrentTime += TimeSpan.FromSeconds(n));
    }
}
