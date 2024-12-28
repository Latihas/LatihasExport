using System.Collections;
using System.Collections.Generic;

namespace SaintCoinach.IO {
    partial class IndexSource {
        public class Enumerator : IEnumerator<File> {
            private readonly IndexSource _Source;
            private IEnumerator<IndexDirectory> _DirectoryEnumerator;
            private IEnumerator<IndexFile> _FileEnumerator;

            public Enumerator(IndexSource source) {
                _Source = source;
                _DirectoryEnumerator = source.Index.Directories.Values.GetEnumerator();
            }

            #region IEnumerator<File> Members

            public File Current {
                get {
                    var index = _FileEnumerator.Current;
                    return _Source.GetFile(index.DirectoryKey, index.FileKey);
                }
            }

            #endregion

            #region IDisposable Members

            public void Dispose() {
                if (_DirectoryEnumerator != null)
                    _DirectoryEnumerator.Dispose();
                _DirectoryEnumerator = null;

                if (_FileEnumerator != null)
                    _FileEnumerator.Dispose();
                _FileEnumerator = null;
            }

            #endregion

            #region IEnumerator Members

            object IEnumerator.Current {
                get { return Current; }
            }

            public bool MoveNext() {
                while (_FileEnumerator == null || !_FileEnumerator.MoveNext()) {
                    if (_FileEnumerator != null)
                        _FileEnumerator.Dispose();
                    _FileEnumerator = null;

                    if (!_DirectoryEnumerator.MoveNext())
                        return false;

                    _FileEnumerator = _DirectoryEnumerator.Current.Files.Values.GetEnumerator();
                }
                return true;
            }

            public void Reset() {
                _DirectoryEnumerator.Reset();
                if (_FileEnumerator != null)
                    _FileEnumerator.Dispose();
                _FileEnumerator = null;
            }

            #endregion
        }

        public IEnumerator<File> GetEnumerator() {
            return new Enumerator(this);
        }
        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }
}
