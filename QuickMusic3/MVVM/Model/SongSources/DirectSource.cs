using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace QuickMusic3.MVVM.Model;

public class DirectSource : ISongSource
{
    private readonly List<SongFile> Files;
    public SongFile this[int index] => Files[index];
    public int Count => Files.Count;
    public int IndexOf(SongFile song) => Files.IndexOf(song);
    public event NotifyCollectionChangedEventHandler? CollectionChanged;
    public IEnumerator<SongFile> GetEnumerator() => Files.GetEnumerator();

    public DirectSource(IEnumerable<string> paths)
    {
        Files = paths.Select(x => new SongFile(x)).ToList();
    }

    public void Remove(SongFile song)
    {
        int index = Files.IndexOf(song);
        if (index != -1)
        {
            Files.RemoveAt(index);
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, song, index));
        }
    }

    public Task GetInOrderAsync(int index) { return Task.CompletedTask; }
}
