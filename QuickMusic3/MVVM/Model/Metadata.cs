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
    }

}
