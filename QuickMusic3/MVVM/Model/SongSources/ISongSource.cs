using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;

namespace QuickMusic3.MVVM.Model;

public interface ISongSource : IReadOnlyList<SongFile>, INotifyCollectionChanged
{
    int IndexOf(SongFile file);
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

public static class SongSourceExtensions
{
    public static (List<ISongSource> sources, int first_index) FromFileList(IEnumerable<string> files, SearchOption search, bool expand_single)
    {
        var results = new List<ISongSource>();
        var batch = new List<string>();
        void process_batch()
        {
            if (batch.Count > 0)
                results.Add(new DirectSource(batch));
            batch.Clear();
        }
        foreach (string file in files)
        {
            bool is_file = File.Exists(file);
            if (is_file && Path.GetExtension(file) == ".m3u")
            {
                process_batch();
                results.Add(new PlaylistFileSource(file));
            }
            else if (is_file)
                batch.Add(file);
            else if (Directory.Exists(file))
            {
                process_batch();
                results.Add(new FolderSource(file, search));
            }
        }
        if (expand_single && results.Count == 0 && batch.Count == 1)
        {
            var folder = new FolderSource(Path.GetDirectoryName(batch[0]), search);
            results.Add(folder);
            for (int i = 0; i < folder.Count; i++)
            {
                if (folder[i].FilePath == batch[0])
                    return (results, i);
            }
            return (results, 0);
        }
        process_batch();
        return (results, 0);
    }
}
