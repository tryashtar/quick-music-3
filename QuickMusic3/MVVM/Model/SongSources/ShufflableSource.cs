using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace QuickMusic3.MVVM.Model;

public class ShufflableSource : ISongSource
{
    private readonly ISongSource BaseSource;
    private readonly List<SongFile> ShuffledCopy;
    public bool IsShuffled { get; private set; }

    public event NotifyCollectionChangedEventHandler CollectionChanged;
    public SongFile this[int index] => IsShuffled ? ShuffledCopy[index] : BaseSource[index];
    public int Count => BaseSource.Count;
    public int IndexOf(SongFile song) => IsShuffled ? ShuffledCopy.IndexOf(song) : BaseSource.IndexOf(song);
    public IEnumerator<SongFile> GetEnumerator() => IsShuffled ? ShuffledCopy.GetEnumerator() : BaseSource.GetEnumerator();

    public ShufflableSource(ISongSource wrapped)
    {
        BaseSource = wrapped;
        ShuffledCopy = new();
        BaseSource.CollectionChanged += (s, e) =>
        {
            if (!IsShuffled)
                CollectionChanged?.Invoke(this, e);
        };
    }

    public void Shuffle(SongFile front = null)
    {
        ShuffledCopy.Clear();
        ShuffledCopy.AddRange(BaseSource);
        Shuffle(ShuffledCopy);
        if (front != null && ShuffledCopy.Remove(front))
            ShuffledCopy.Insert(0, front);
        IsShuffled = true;
        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    public void Unshuffle()
    {
        ShuffledCopy.Clear();
        IsShuffled = false;
        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    private static readonly Random RNG = new Random();
    private static void Shuffle<T>(IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = RNG.Next(n + 1);
            (list[n], list[k]) = (list[k], list[n]);
        }
    }
}
