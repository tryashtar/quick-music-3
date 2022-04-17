using NAudio.Wave;
using System;
using System.Threading.Tasks;

namespace QuickMusic3.MVVM.Model;

public class Metadata
{
    public string Title { get; }
    public string Artist { get; }
    public string Album { get; }
    public Metadata(string path)
    {
        using var file = TagLib.File.Create(path);
        Title = file.Tag.Title;
        Artist = file.Tag.FirstPerformer;
        Album = file.Tag.Album;
    }
}
