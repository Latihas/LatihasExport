using System.IO;

namespace SaintCoinach.IO {
    public class Index2File : IIndexFile {
        #region Properties

        public PackIdentifier PackId { get; private set; }
        public uint FileKey { get; private set; }
        public uint Offset { get; private set; }

        /// <summary>
        ///     In which .dat* file the data is located.
        /// </summary>
        public byte DatFile { get; private set; }

        #endregion
        
        #region Constructor

        public Index2File(PackIdentifier packId, BinaryReader reader) {
            PackId = packId;
            FileKey = reader.ReadUInt32();

            var baseOffset = reader.ReadInt32();
            DatFile = (byte)((baseOffset & 0x7) >> 1);
            Offset = (uint)((baseOffset & 0xFFFFFFF8) << 3);
        }

        #endregion

        #region IEquatable<IIndexFile> Members

        public override int GetHashCode() {
            return (int)(((DatFile << 24) | PackId.GetHashCode()) ^ Offset);
        }
        public override bool Equals(object obj) {
            if (obj is IIndexFile)
                return Equals((IIndexFile)obj);
            return false;
        }
        public bool Equals(IIndexFile other) {
            return other.PackId.Equals(PackId)
                && other.DatFile == DatFile
                && other.Offset == Offset;
        }

        #endregion
    }
}
