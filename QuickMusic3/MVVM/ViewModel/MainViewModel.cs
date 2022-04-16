using NAudio.Wave;
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
// - audio doesn't stop when application closed

public class MainViewModel
{
    public Player Player { get; private set; } = new();
    public ICommand PlayPauseCommand { get; }

    public MainViewModel()
    {
        PlayPauseCommand = new RelayCommand(() =>
        {
            if (Player.PlayState == PlaybackState.Playing)
                Player.Pause();
            else
                Player.Play();
        });
    }
}
