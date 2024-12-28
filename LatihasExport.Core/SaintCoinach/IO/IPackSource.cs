using System.Collections.Generic;

namespace SaintCoinach.IO {
    public interface IPackSource : IEnumerable<File> {
        bool FileExists(string path);
        bool TryGetFile(string path, out File value);
        File GetFile(string path);
    }
}
