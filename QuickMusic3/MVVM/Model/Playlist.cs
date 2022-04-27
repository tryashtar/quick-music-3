using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace QuickMusic3.MVVM.Model;

public class Playlist
{
    public ObservableCollection<LoadableStream> ActiveList { get; } = new();
    private readonly object ListLock = new();
    public void AddSource(ISongSource source)
    {
        IEnumerable<LoadableStream> songs;
        if (source.DoesSorting && source.IsSorted)
            songs = source.GetSorted();
        else
            songs = source.GetSongs();
        int first_index = ActiveList.Count;
        if (source.DoesSorting && !source.IsSorted)
            source.SortingDone += (s, e) =>
            {
                var sorted = source.GetSorted().ToArray();
                lock (ListLock)
                {
                    for (int i = 0; i < sorted.Length; i++)
                    {
                        ActiveList[first_index + i] = sorted[i];
                    }
                }
            };
        lock (ListLock)
        {
            foreach (var item in songs)
            {
                ActiveList.Add(item);
            }
        }
    }
}

public interface ISongSource
{
    public bool DoesSorting { get; }
    public bool IsSorted { get; }
    public IEnumerable<LoadableStream> GetSorted();
    public IEnumerable<LoadableStream> GetSongs();
    public event EventHandler SortingDone;
}

public class FolderSource : ISongSource
{
    private readonly LoadableStream[] OriginalOrder;
    private readonly SortedList<Metadata, LoadableStream> SortedOrder = new(MetadataOrderer.Instance);
    private readonly object SortedLock = new();
    public FolderSource(string path, SearchOption search)
    {
        OriginalOrder = Directory.GetFiles(path, "*", search).Select(x => new LoadableStream(x)).ToArray();
        foreach (var item in OriginalOrder)
        {
            item.Metadata.Loaded += (s, e) =>
              {
                  lock (SortedLock)
                  {
                      SortedOrder.Add(item.Metadata, item);
                  }
                  if (SortedOrder.Count == OriginalOrder.Length)
                  {
                      IsSorted = true;
                      SortingDone?.Invoke(this, EventArgs.Empty);
                  }
              };
        }
    }

    public bool DoesSorting => true;
    public bool IsSorted { get; private set; } = false;
    public event EventHandler SortingDone;

    public IEnumerable<LoadableStream> GetSongs()
    {
        return OriginalOrder;
    }

    public IEnumerable<LoadableStream> GetSorted()
    {
        return SortedOrder.Values;
    }
}

public class MetadataOrderer : IComparer<Metadata>
{
    public static readonly MetadataOrderer Instance = new();
    public int Compare(Metadata x, Metadata y)
    {
        int album = (x.Album ?? "").CompareTo(y.Album ?? "");
        if (album != 0)
            return album;
        int disc = x.DiscNumber.CompareTo(y.DiscNumber);
        if (disc != 0)
            return disc;
        int track = x.TrackNumber.CompareTo(y.TrackNumber);
        if (track != 0)
            return track;
        int num = (x.Title ?? "").CompareTo(y.Title ?? "");
        if (num == 0)
            return 1;
        return num;
    }
}