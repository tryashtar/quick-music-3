using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace QuickMusic3.MVVM.Model;

public class Playlist : ISongSource
{
    private readonly List<ISongSource> Sources = new();

    public SongFile this[int index]
    {
        get
        {
            foreach (var item in Sources)
            {
                if (index < item.Count)
                    return item[index];
                index -= item.Count;
            }
            throw new IndexOutOfRangeException();
        }
    }
    public int Count => Sources.Sum(x => x.Count);
    public event EventHandler Changed;

    public void AddSource(ISongSource source)
    {
        Sources.Add(source);
    }

    public IEnumerator<SongFile> GetEnumerator()
    {
        return Sources.SelectMany(x => x).GetEnumerator();
    }
}

public interface ISongSource : IReadOnlyList<SongFile>, INotifyCollectionChanged
{
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

public class FolderSource : ISongSource
{
    private readonly List<SongFile> Streams;
    public event NotifyCollectionChangedEventHandler CollectionChanged;

    public int Count => Streams.Count;
    public SongFile this[int index] => Streams[index];
    public IEnumerator<SongFile> GetEnumerator() => Streams.GetEnumerator();

    public FolderSource(string path, SearchOption search)
    {
        Streams = Directory.GetFiles(path, "*", search).Select(x => new SongFile(x)).ToList();
        foreach (var item in Streams)
        {
            item.Metadata.Failed += (s, e) => MoveIntoPlace(item);
            item.Metadata.Loaded += (s, e) => MoveIntoPlace(item);
            item.Stream.Failed += (s, e) => Remove(item);
        }
    }

    private void MoveIntoPlace(SongFile item)
    {
        int destination = Streams.BinarySearch(item, SongSorter.Instance)
    }

    private void Remove(SongFile item)
    {

    }
}

public class SongSorter : IComparer<SongFile>
{
    public static readonly SongSorter Instance = new();
    public int Compare(SongFile x, SongFile y)
    {
        if (x.Metadata.LoadStatus )
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