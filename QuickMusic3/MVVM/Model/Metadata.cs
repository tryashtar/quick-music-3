using NAudio.Wave;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace QuickMusic3.MVVM.Model;

public class Metadata
{
    public string Title { get; }
    public string Artist { get; }
    public string Album { get; }
    public BitmapSource Thumbnail { get; }
    public decimal ReplayGain { get; }
    public BitmapSource HighResImage
    {
        get
        {
            using var file = TagLib.File.Create(FilePath);
            return ArtCache.GetHighResEmbeddedImage(file.Tag);
        }
    }

    private readonly string FilePath;
    public Metadata(string path)
    {
        FilePath = path;
        using var file = TagLib.File.Create(path);
        Title = file.Tag.Title ?? Path.GetFileName(path);
        Artist = file.Tag.FirstPerformer;
        Album = file.Tag.Album;
        Thumbnail = ArtCache.GetEmbeddedImage(file.Tag);
        ReplayGain = LoadReplayGain(file);
    }

    private static decimal LoadReplayGain(TagLib.File file)
    {
        const string TRACK_GAIN = "REPLAYGAIN_TRACK_GAIN";
        var ape = (TagLib.Ape.Tag)file.GetTag(TagLib.TagTypes.Ape);
        if (ape != null)
        {
            if (ape.HasItem(TRACK_GAIN))
                return ParseDB(ape.GetItem(TRACK_GAIN).ToString());
        }
        var ogg = (TagLib.Ogg.XiphComment)file.GetTag(TagLib.TagTypes.Xiph);
        if (ogg != null)
        {
            var gain = ogg.GetFirstField(TRACK_GAIN);
            if (gain != null)
                return ParseDB(gain);
        }
        return 0;
    }

    private static decimal ParseDB(string db)
    {
        return decimal.Parse(db[..^" dB".Length]);
    }
}
