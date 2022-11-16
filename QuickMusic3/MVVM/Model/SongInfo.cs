using NAudio.Wave;
using QuickMusic3.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using TryashtarUtils.Music;

namespace QuickMusic3.MVVM.Model;

public sealed class SongInfo : ObservableObject
{
    private bool _available;
    public bool Available { get => _available; private set { if (_available != value) { _available = value; OnPropertyChanged(); } } }
    private string? _currentChapters;
    public string? CurrentChapters { get => _currentChapters; private set { if (_currentChapters != value) { _currentChapters = value; OnPropertyChanged(); } } }
    public HashSet<LyricsEntry> CurrentLines { get; } = new();

    public void Update(SongFile song, TimeSpan timestamp)
    {
        if (Available && !song.Metadata.IsSuccessfullyCompleted)
        {
            Available = false;
            CurrentChapters = null;
            CurrentLines.Clear();
            OnPropertyChanged(nameof(CurrentLines));
        }
        if (song.Metadata.IsSuccessfullyCompleted)
        {
            var meta = song.Metadata.Item;
            Available = true;
            if (meta.Chapters == null)
                CurrentChapters = null;
            else
                CurrentChapters = String.Join('\n', meta.Chapters.ChaptersAtTime(timestamp).Select(x => x.Title));
            if (meta.Lyrics == null)
            {
                if (CurrentLines.Count > 0)
                {
                    CurrentLines.Clear();
                    OnPropertyChanged(nameof(CurrentLines));
                }
            }
            else
            {
                var current_lines = meta.Lyrics.LyricsAtTime(timestamp).ToHashSet();
                if (!CurrentLines.SetEquals(current_lines))
                {
                    CurrentLines.Clear();
                    CurrentLines.UnionWith(current_lines);
                    OnPropertyChanged(nameof(CurrentLines));
                }
            }
        }
    }
}
