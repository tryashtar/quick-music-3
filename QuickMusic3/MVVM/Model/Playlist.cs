using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace QuickMusic3.MVVM.Model;

public class Playlist : ISongSource
{
    private readonly List<ISongSource> Sources = new();

    public LoadableStream this[int index]
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

    public IEnumerator<LoadableStream> GetEnumerator()
    {
        return Sources.SelectMany(x => x).GetEnumerator();
    }
}

public interface ISongSource : IReadOnlyList<LoadableStream>
{
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    public event EventHandler Changed;
}

public class FolderSource : ISongSource
{
    private readonly List<LoadableStream> OriginalOrder;
    private readonly SortedList<Metadata, LoadableStream> SortedOrder = new(MetadataOrderer.Instance);
    public event EventHandler Changed;

    public bool IsSorted { get; private set; } = false;

    public int Count => OriginalOrder.Count;
    public LoadableStream this[int index]
    {
        get
        {
            if (SortedOrder.Count == OriginalOrder.Count)
                return SortedOrder.Values[index];
            return OriginalOrder[index];
        }
    }

    public IEnumerator<LoadableStream> GetEnumerator()
    {
        if (SortedOrder.Count == OriginalOrder.Count)
            return SortedOrder.Values.GetEnumerator();
        return OriginalOrder.GetEnumerator();
    }

    public FolderSource(string path, SearchOption search)
    {
        OriginalOrder = Directory.GetFiles(path, "*", search).Select(x => new LoadableStream(x)).ToList();
        foreach (var item in OriginalOrder)
        {
            item.Metadata.Failed += (s, e) =>
              {
                  lock (OriginalOrder)
                  {
                      OriginalOrder.Remove(item);
                  }
              };
            item.StreamFailed += (s, e) =>
            {
                lock (OriginalOrder)
                {
                    OriginalOrder.Remove(item);
                }
            };
            item.Metadata.Loaded += (s, e) =>
              {
                  lock (SortedOrder)
                  {
                      SortedOrder.Add(item.Metadata, item);
                      if (SortedOrder.Count == OriginalOrder.Count)
                          Changed?.Invoke(this, EventArgs.Empty);
                  }
              };
        }
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