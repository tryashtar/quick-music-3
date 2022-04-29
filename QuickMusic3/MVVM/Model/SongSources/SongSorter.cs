using System.Collections.Generic;

namespace QuickMusic3.MVVM.Model;

public class SongSorter : IComparer<SongFile>
{
    public static readonly SongSorter Instance = new();
    public int Compare(SongFile x, SongFile y)
    {
        if (!x.Metadata.IsLoaded || !y.Metadata.IsLoaded)
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
}