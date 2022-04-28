using System.Collections.Generic;

namespace QuickMusic3.MVVM.Model;

public class SongSorter : IComparer<SongFile>
{
    public static readonly SongSorter Instance = new();
    public int Compare(SongFile x, SongFile y)
    {
        int status = LoadStatusOrder(x.Metadata.LoadStatus).CompareTo(LoadStatusOrder(y.Metadata.LoadStatus));
        if (status != 0)
            return status;
        if (x.Metadata.LoadStatus != LoadStatus.Loaded)
            return x.FilePath.CompareTo(y.FilePath);
        var xm = x.Metadata.Item;
        var ym = y.Metadata.Item;
        int album = (xm.Album ?? "").CompareTo(ym.Album ?? "");
        if (album != 0)
            return album;
        int disc = xm.DiscNumber.CompareTo(ym.DiscNumber);
        if (disc != 0)
            return disc;
        int track = xm.TrackNumber.CompareTo(ym.TrackNumber);
        if (track != 0)
            return track;
        return (xm.Title ?? "").CompareTo(ym.Title ?? "");
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