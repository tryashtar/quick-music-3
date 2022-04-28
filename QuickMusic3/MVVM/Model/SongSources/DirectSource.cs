using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace QuickMusic3.MVVM.Model;

public class DirectSource : ISongSource
{
    private readonly List<SongFile> Streams;
    public SongFile this[int index] => Streams[index];
    public int Count => Streams.Count;
    public event NotifyCollectionChangedEventHandler CollectionChanged;
    public IEnumerator<SongFile> GetEnumerator() => Streams.GetEnumerator();

    public DirectSource(IEnumerable<string> paths)
    {
        Streams = paths.Select(x => new SongFile(x)).ToList();
    }
}
