using NAudio.Wave;
using System;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace QuickMusic3.MVVM.Model;

public class Metadata
{
    public string Title { get; }
    public string Artist { get; }
    public string Album { get; }
    public BitmapSource Thumbnail { get; }
    public BitmapSource HighResImage
    {
        get
        {
            using var file = TagLib.File.Create(Path);
            return ArtCache.GetHighResEmbeddedImage(file.Tag);
        }
    }

    private readonly string Path;
    public Metadata(string path)
    {
        Path = path;
        using var file = TagLib.File.Create(path);
        Title = file.Tag.Title;
        Artist = file.Tag.FirstPerformer;
        Album = file.Tag.Album;
        Thumbnail = ArtCache.GetEmbeddedImage(file.Tag);
    }

}
