using System.Collections.Generic;
using System.IO;
using TryashtarUtils.Utility;

namespace QuickMusic3.MVVM.Model;

public class SongSorter : IComparer<SongFile>
{
    public static readonly SongSorter Instance = new();
    public int Compare(SongFile x, SongFile y)
    {
        if (x.Metadata.IsSuccessfullyCompleted && y.Metadata.IsSuccessfullyCompleted)
        {
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