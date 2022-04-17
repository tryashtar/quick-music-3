using NAudio.Wave;
using QuickMusic3.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace QuickMusic3.MVVM.Model;

public static class Playlist
{
    public static string[] LoadFiles(string[] paths)
    {
        var result = new List<string>(paths.Length);
        foreach (var item in paths)
        {
            if (Path.GetExtension(item) == ".m3u")
                result.AddRange(LoadPlaylistFile(item));
            else
                result.Add(item);
        }
        return result.ToArray();
    }

    public static string[] LoadPlaylistFile(string path)
    {
        var folder = Path.GetDirectoryName(path);
        var list = new List<string>();
        foreach (var line in File.ReadAllLines(path))
        {
            if (String.IsNullOrWhiteSpace(line))
                continue;
            if (line.StartsWith("#"))
                continue;
            // make sure references load relative to the m3u file itself
            // if reference path is absolute, it just uses that which is perfect
            var linepath = Path.Combine(folder, line);
            if (File.Exists(linepath))
                list.Add(linepath);
        }
        return list.ToArray();
    }
}
