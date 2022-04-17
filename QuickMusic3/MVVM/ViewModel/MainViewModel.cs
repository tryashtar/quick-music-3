﻿using NAudio.Wave;
using QuickMusic3.Core;
using QuickMusic3.MVVM.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace QuickMusic3.MVVM.ViewModel;

// current bugs:
// - if you click the extra hitbox of slider, it doesn't fire MouseDown
// - if you click the handle of a slider, it doesn't jump to the center of the cursor
// - slider jittery for short tracks

// features to port:
// - tray
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
    public Player Player { get; private set; } = new();
    public ICommand PlayPauseCommand { get; }
    public ICommand NextCommand { get; }
    public ICommand PrevCommand { get; }
    public ICommand ChangeRepeatCommand { get; }
    public ICommand ChangeMuteCommand { get; }
    public ICommand ChangeShuffleCommand { get; }
    public ICommand IncreaseVolumeCommand { get; }
    public ICommand DecreaseVolumeCommand { get; }

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
        PrevCommand = new RelayCommand(() => Player.Prev());
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
        IncreaseVolumeCommand = new RelayCommand(() => { Player.Volume = Math.Clamp(Player.Volume + 1 / 20f, 0, 1); });
        DecreaseVolumeCommand = new RelayCommand(() => { Player.Volume = Math.Clamp(Player.Volume - 1 / 20f, 0, 1); });
    }
}
