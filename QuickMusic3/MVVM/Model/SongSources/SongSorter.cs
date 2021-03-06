using System.Collections.Generic;
using TryashtarUtils.Utility;

namespace QuickMusic3.MVVM.Model;

public class SongSorter : IComparer<SongFile>
{
    public static readonly SongSorter Instance = new();
    public int Compare(SongFile x, SongFile y)
    {
        int status = LoadStatusOrder(x.Metadata.LoadStatus).CompareTo(LoadStatusOrder(y.Metadata.LoadStatus));
        if (status != 0)
            return status;
        if (!x.Metadata.IsLoaded) // both are unloaded because otherwise previous check would return
            return LogicalStringComparer.Instance.Compare(x.FilePath, y.FilePath);
        var xm = x.Metadata.Item;
        var ym = y.Metadata.Item;
        int album = LogicalStringComparer.Instance.Compare(xm.Album ?? "", ym.Album ?? "");
        if (album != 0)
            return album;
        int disc = xm.DiscNumber.CompareTo(ym.DiscNumber);
        if (disc != 0)
            return disc;
        int track = xm.TrackNumber.CompareTo(ym.TrackNumber);
        if (track != 0)
            return track;
        int title = LogicalStringComparer.Instance.Compare(xm.Title ?? "", ym.Title ?? "");
        if (title != 0)
            return title;
        return LogicalStringComparer.Instance.Compare(x.FilePath, y.FilePath);
    }

    private static int LoadStatusOrder(LoadStatus status)
    {
        return status switch
        {
            LoadStatus.Loading => 0,
            LoadStatus.NotLoaded => 0,
            LoadStatus.Loaded => 1,
            LoadStatus.Failed => 2,
            _ => 3
        };
    }
}