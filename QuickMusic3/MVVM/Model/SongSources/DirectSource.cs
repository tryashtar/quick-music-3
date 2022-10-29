using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;

namespace QuickMusic3.MVVM.Model;

public class DirectSource : ISongSource
{
    private readonly List<SongReference> Files;
    public SongReference this[int index] => Files[index];
    public int Count => Files.Count;
    public int IndexOf(SongFile song) => Files.FindIndex(x => x.Song == song);
    public event NotifyCollectionChangedEventHandler CollectionChanged;
    public IEnumerator<SongReference> GetEnumerator() => Files.GetEnumerator();

    public DirectSource(IEnumerable<string> paths)
    {
        Files = paths.Select(x => new SongReference(x)).ToList();
    }

    public void Remove(SongReference song)
    {
        int index = Files.IndexOf(song);
        if (index != -1)
        {
            Files.RemoveAt(index);
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, song, index));
        }
    }

    public void GetInOrder(int index, bool now) { }
}
