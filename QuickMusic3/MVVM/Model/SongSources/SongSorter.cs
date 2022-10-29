using System.Collections.Generic;
using System.IO;
using TryashtarUtils.Utility;

namespace QuickMusic3.MVVM.Model;

public class SongSorter : IComparer<SongReference>
{
    public static readonly SongSorter Instance = new();
    public int Compare(SongReference x, SongReference y)
    {
        if (x.Song != null && y.Song != null && x.Song.Metadata.IsLoaded && y.Song.Metadata.IsLoaded)
        {
            var xm = x.Song.Metadata.Item;
            var ym = y.Song.Metadata.Item;
            int album = LogicalStringComparer.Instance.Compare(xm.Album ?? "", ym.Album ?? "");
            if (album != 0)
                return album;
            int disc = xm.DiscNumber.CompareTo(ym.DiscNumber);
            if (disc != 0)
                return disc;
            int track = xm.TrackNumber.CompareTo(ym.TrackNumber);
            if (track != 0)
                return track;
            if (Path.GetDirectoryName(x.FilePath) == Path.GetDirectoryName(y.FilePath))
            {
                int title = LogicalStringComparer.Instance.Compare(xm.Title ?? "", ym.Title ?? "");
                if (title != 0)
                    return title;
            }
        }
        return LogicalStringComparer.Instance.Compare(x.FilePath, y.FilePath);
    }
}