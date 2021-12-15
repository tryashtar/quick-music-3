global using QuickMusic3.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickMusic3.MVVM.Model
{
    public class Player : ObservableObject
    {
        public SongFile CurrentSong;
        private TimeSpan _currentTime;

        public TimeSpan CurrentTime
        {
            get { return _currentTime; }
            set { _currentTime = value; OnPropertyChanged(); }
        }

        public TimeSpan TotalTime { get; set; }
    }
}
