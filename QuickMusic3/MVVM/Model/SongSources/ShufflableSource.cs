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

    public async Task GetInOrderAsync(int index)
    {
        if (!IsShuffled)
            await BaseSource.GetInOrderAsync(index);
    }

    public void Remove(SongFile song)
    {
        BaseSource.Remove(song);
    }

    public ShufflableSource(ISongSource wrapped)
    {
        BaseSource = wrapped;
        ShuffledCopy = new();
        BaseSource.CollectionChanged += (s, e) =>
        {
            if (!IsShuffled)
                CollectionChanged?.Invoke(this, e);
            else
            {
                if (e.Action == NotifyCollectionChangedAction.Remove)
                {
                    foreach (var item in e.OldItems)
                    {
                        int index = ShuffledCopy.IndexOf((SongFile)item);
                        if (index != -1)
                        {
                            ShuffledCopy.RemoveAt(index);
                            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
                        }
                    }
                }
                else if (e.Action != NotifyCollectionChangedAction.Move) // ignore moves
                    throw new NotSupportedException();
            }
        };
    }

    public void Shuffle(int? first_index = null)
    {
        ShuffledCopy.Clear();
        ShuffledCopy.AddRange(BaseSource);
        Shuffle(ShuffledCopy);
        if (first_index.HasValue)
        {
            var item = BaseSource[first_index.Value];
            ShuffledCopy.RemoveAt(ShuffledCopy.IndexOf(item));
            ShuffledCopy.Insert(0, item);
        }
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
