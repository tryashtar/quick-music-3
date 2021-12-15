using QuickMusic3.MVVM.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickMusic3.MVVM.ViewModel
{
    public class PlayerViewModel
    {
        public Player Player { get; } = new();
        public PlayerViewModel()
        {
            Player.CurrentSong = new SongFile();
            Player.TotalTime = TimeSpan.FromMinutes(5);
        }
    }
}
