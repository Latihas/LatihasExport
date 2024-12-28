using System.Collections;
using System.Collections.Generic;

namespace SaintCoinach.IO {
    partial class Index2Source {
        public class Enumerator : IEnumerator<File> {
            private readonly Index2Source _Source;
            private readonly IEnumerator<Index2File> _FileEnumerator;

            public Enumerator(Index2Source source) {
                _Source = source;
                _FileEnumerator = source.Index.Files.Values.GetEnumerator();
            }

            #region IEnumerator<File> Members

            public File Current {
                get { return _Source.GetFile(_FileEnumerator.Current.FileKey); }
            }

            #endregion

            #region IDisposable Members

            public void Dispose() {
                _FileEnumerator.Dispose();
            }

            #endregion

            #region IEnumerator Members

            object IEnumerator.Current {
                get { return Current; }
            }

            public bool MoveNext() {
                return _FileEnumerator.MoveNext();
            }

            public void Reset() {
                _FileEnumerator.Reset();
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
