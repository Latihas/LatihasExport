using System;

namespace SaintCoinach.IO {
    public interface IIndexFile : IEquatable<IIndexFile> {
        PackIdentifier PackId { get; }
        uint FileKey { get; }
        uint Offset { get; }
        byte DatFile { get; }
    }
}
