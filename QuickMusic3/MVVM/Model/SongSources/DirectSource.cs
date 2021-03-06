using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace QuickMusic3.MVVM.Model;

public class DirectSource : ISongSource
{
    private readonly List<SongFile> Streams;
    public SongFile this[int index] => Streams[index];
    public int Count => Streams.Count;
    public int IndexOf(SongFile song) => Streams.IndexOf(song);
    public event NotifyCollectionChangedEventHandler CollectionChanged { add { } remove { } }
    public IEnumerator<SongFile> GetEnumerator() => Streams.GetEnumerator();

    public DirectSource(IEnumerable<string> paths)
    {
        Streams = paths.Select(x => new SongFile(x)).ToList();
    }

    public void GetInOrder(int index, bool now) { }
}
