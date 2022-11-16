using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QuickMusic3.MVVM.Model;

public class PlayHistory
{
    private readonly Player Parent;
    private record Entry(ISongSource Source, SongFile Song, TimeSpan Time, PlaybackState State);
    private int CurrentIndex = 0;
    private readonly List<Entry> History;

    public PlayHistory(Player parent)
    {
        Parent = parent;
        History = new();
    }

    private Entry MakeCurrentEntry()
    {
        return new Entry(Parent.RawSource, Parent.CurrentTrack, Parent.CurrentTime, Parent.PlayState);
    }

    public void Add()
    {
        if (CurrentIndex < History.Count - 1)
            History.RemoveRange(CurrentIndex + 1, History.Count - CurrentIndex - 1);
        History.Add(MakeCurrentEntry());
        if (History.Count > 1)
            CurrentIndex++;
    }

    private async Task SwitchToAsync(Entry entry)
    {
        if (Parent.RawSource != entry.Source)
            await Parent.OpenAsync(entry.Source);
        await Parent.SwitchToAsync(entry.Song);
        Parent.CurrentTime = entry.Time;
        if (entry.State == PlaybackState.Stopped || entry.State == PlaybackState.Paused)
            Parent.Pause();
        else if (entry.State == PlaybackState.Playing)
            Parent.Play();
    }

    public async Task ForwardAsync()
    {
        if (History.Count == 0)
            return;
        History[CurrentIndex] = MakeCurrentEntry();
        if (CurrentIndex < History.Count - 1)
        {
            CurrentIndex++;
            await SwitchToAsync(History[CurrentIndex]);
        }
    }

    public async Task BackwardAsync()
    {
        if (History.Count == 0)
            return;
        History[CurrentIndex] = MakeCurrentEntry();
        if (CurrentIndex > 0)
        {
            CurrentIndex--;
            await SwitchToAsync(History[CurrentIndex]);
        }
    }
}
