using System.Collections;
using System.Collections.Generic;

namespace SaintCoinach.Ex {
    partial class PartialDataSheet<T> {
        private class Enumerator : IEnumerator<T> {
            #region Fields
            private PartialDataSheet<T> _Sheet;
            private IEnumerator<KeyValuePair<int, int>> _InnerEnumerator;
            #endregion

            #region Constructor
            public Enumerator(PartialDataSheet<T> sheet) {
                _Sheet = sheet;
                _InnerEnumerator = sheet._RowOffsets.GetEnumerator();
            }
            #endregion

            #region IEnumerator<T> Members

            public T Current {
                get {
                    var inner = _InnerEnumerator.Current;
                    return _Sheet._Rows.GetOrAdd(inner.Key, k => _Sheet.CreateRow(k, inner.Value));
                }
            }

            #endregion

            #region IDisposable Members

            public void Dispose() {
                _InnerEnumerator.Dispose();
            }

            #endregion

            #region IEnumerator Members

            object IEnumerator.Current {
                get { return Current; }
            }

            public bool MoveNext() {
                return _InnerEnumerator.MoveNext();
            }

            public void Reset() {
                _InnerEnumerator.Reset();
            }

            #endregion
        }
    }
}
