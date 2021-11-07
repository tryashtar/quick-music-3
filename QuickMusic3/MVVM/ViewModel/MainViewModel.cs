using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickMusic3.MVVM.ViewModel
{
    public class MainViewModel
    {
        public ObservableCollection<PlaylistEntry> Playlist { get; } = new();
        public MainViewModel()
        {
            for (int i = 0; i < 30000; i++)
            {
                Playlist.Add(new PlaylistEntry() { Title = i.ToString(), Artist = "art", Album = "whatever", Art = @"D:\Documents\Random Scripts\icon generator\templates\Toby Fox\pixel_Deltarune.png" });

            }
            Playlist.Add(new PlaylistEntry() { Title = "test", Artist = "art", Album = "whatever" });
        }
    }

    public class PlaylistEntry
    {
        public string Title { get; set; }
        public string Artist { get; set; }
        public string Album { get; set; }
        public string Art { get; set; }
    }
}
