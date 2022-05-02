using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;

namespace QuickMusic3.MVVM.Model;

public class FolderSource : ISongSource
{
    private readonly List<SongFile> Streams;
    public event NotifyCollectionChangedEventHandler CollectionChanged;

    public int Count => Streams.Count;
    public int IndexOf(SongFile song) => Streams.IndexOf(song);
    public SongFile this[int index] => Streams[index];
    public IEnumerator<SongFile> GetEnumerator() => Streams.GetEnumerator();

    private bool CheckExtension(FileInfo info)
    {
        string ext = info.Extension;
        return ext switch
        {
            ".mp3" or ".aac" or ".aiff" or ".flac" or ".m4a" or ".ogg" or ".wav" or ".wma" => true,
            _ => false
        };
    }

    public FolderSource(string path, SearchOption search)
    {
        var directory = new DirectoryInfo(path);
        Streams = directory.GetFiles("*", search)
            .Where(x => !x.Attributes.HasFlag(FileAttributes.Hidden) && !x.Attributes.HasFlag(FileAttributes.System))
            .Where(CheckExtension)
            .Select(x => new SongFile(x.FullName)).ToList();
        foreach (var item in Streams)
        {
            item.Metadata.Failed += (s, e) => MoveIntoPlace(item);
            item.Metadata.Loaded += (s, e) => MoveIntoPlace(item);
            item.Stream.Failed += (s, e) => Remove(item);
        }
    }

    private void MoveIntoPlace(SongFile item)
    {
        int old_index, destination;
        lock (Streams)
        {
            old_index = Streams.IndexOf(item);
            if (old_index == -1)
                return;
            Streams.RemoveAt(old_index);
            destination = Streams.BinarySearch(item, SongSorter.Instance);
            if (destination < 0)
                destination = ~destination;
            Streams.Insert(destination, item);
            //Debug.WriteLine("M " + String.Join(' ', Streams.Select(x => x.Metadata.IsLoaded ? x.Metadata.Item.TrackNumber.ToString() : x.Metadata.LoadStatus == LoadStatus.Failed ? "F" : ".")));
        }
        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, item, destination, old_index));
    }

    private void Remove(SongFile item)
    {
        int old_index;
        lock (Streams)
        {
            old_index = Streams.IndexOf(item);
            Streams.RemoveAt(old_index);
            //Debug.WriteLine("R " + String.Join(' ', Streams.Select(x => x.Metadata.IsLoaded ? x.Metadata.Item.TrackNumber.ToString() : x.Metadata.LoadStatus == LoadStatus.Failed ? "F" : ".")));
        }
        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, old_index));
    }
}
