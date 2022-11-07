using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace QuickMusic3.MVVM.Model;

public class FolderSource : ISongSource
{
    private readonly List<SongFile> Files;
    private readonly HashSet<SongFile> Loaded = new();
    private readonly IComparer<SongFile> Sorter;
    private readonly Dictionary<string, List<SongFile>> Folders = new();
    public event NotifyCollectionChangedEventHandler CollectionChanged;

    public int Count => Files.Count;
    public int IndexOf(SongFile song) => Files.IndexOf(song);
    public SongFile this[int index] => Files[index];
    public IEnumerator<SongFile> GetEnumerator() => Files.GetEnumerator();

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
        Files = directory.GetFiles("*", search)
            .Where(x => !x.Attributes.HasFlag(FileAttributes.Hidden) && !x.Attributes.HasFlag(FileAttributes.System))
            .Where(CheckExtension)
            .Select(x => new SongFile(x.FullName)).ToList();
        foreach (var item in Files)
        {
            item.Metadata.Failed += (s, e) => MoveIntoPlace(item);
            item.Metadata.Loaded += (s, e) => MoveIntoPlace(item);
            var folder = Path.GetDirectoryName(item.FilePath);
            if (!Folders.ContainsKey(folder))
                Folders[folder] = new();
            Folders[folder].Add(item);
        }
        Sorter = new LoadableComparer(this);
    }

    private class LoadableComparer : IComparer<SongFile>
    {
        private readonly FolderSource Parent;
        public LoadableComparer(FolderSource parent)
        {
            Parent = parent;
        }

        public int Compare(SongFile x, SongFile y)
        {
            bool x_loaded = Parent.Loaded.Contains(x);
            bool y_loaded = Parent.Loaded.Contains(y);
            if (x_loaded && !y_loaded)
                return -1;
            if (y_loaded && !x_loaded)
                return 1;
            return SongSorter.Instance.Compare(x, y);
        }
    }

    public void GetInOrder(int index, bool now)
    {
        if (index < 0 || index >= Files.Count)
            Debug.WriteLine($"Tried to get index {index} in order for {Files.Count} streams");
        else if (Folders.TryGetValue(Path.GetDirectoryName(Files[index].FilePath), out var folder))
        {
            foreach (var item in folder)
            {
                if (now)
                    item.Metadata.LoadNow();
                else
                    item.Metadata.LoadBackground();
            }
        }
    }

    private void MoveIntoPlace(SongFile item)
    {
        int old_index, destination;
        lock (Files)
        {
            // in order for the binary search to work correctly, the list must remain sorted at all times
            // however, the metadata is loading in asynchronously on other threads
            // so the first line of defense is the lock, of course
            // but we can't have the binary search checking the comparer against loaded tracks that are still waiting on the lock
            // so we keep track of which tracks are loaded here too
            // any tracks that happen to finish loading but haven't been moved into place yet are considered unloaded by the comparer until it's their turn
            Loaded.Add(item);
            old_index = Files.IndexOf(item);
            if (old_index == -1)
                return;
            Files.RemoveAt(old_index);
            destination = Files.BinarySearch(item, Sorter);
            if (destination < 0)
                destination = ~destination;
            Files.Insert(destination, item);
            Debug.WriteLine("M " + String.Join(' ', Files.Select(x => x.Metadata.IsLoaded ? x.Metadata.Item.TrackNumber.ToString() : x.Metadata.LoadStatus == LoadStatus.Failed ? "F" : ".")));
        }
        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, item, destination, old_index));
    }

    public void Remove(SongFile item)
    {
        int old_index;
        lock (Files)
        {
            old_index = Files.IndexOf(item);
            if (old_index == -1)
                return;
            Files.RemoveAt(old_index);
            Debug.WriteLine("R " + String.Join(' ', Files.Select(x => x.Metadata.IsLoaded ? x.Metadata.Item.TrackNumber.ToString() : x.Metadata.LoadStatus == LoadStatus.Failed ? "F" : ".")));
        }
        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, old_index));
    }
}
