using System.Collections;
using System.Collections.Generic;

namespace SaintCoinach.Ex {
    partial class DataSheet<T> {
        private class Enumerator : IEnumerator<T> {
            #region Fields
            private DataSheet<T> _Sheet;
            private IEnumerator<ISheet<T>> _PartialSheetsEnumerator;
            private IEnumerator<T> _RowEnumerator;
            #endregion

            #region Constructor
            public Enumerator(DataSheet<T> sheet) {
                _Sheet = sheet;
                sheet.CreateAllPartialSheets();

                _PartialSheetsEnumerator = sheet._PartialSheets.Values.GetEnumerator();
            }
            #endregion

            #region IEnumerator<T> Members

            public T Current {
                get { return _RowEnumerator.Current; }
            }

            #endregion

            #region IDisposable Members

            public void Dispose() {
                _PartialSheetsEnumerator.Dispose();
                if (_RowEnumerator != null)
                    _RowEnumerator.Dispose();
                _RowEnumerator = null;
            }

            #endregion

            #region IEnumerator Members

            object IEnumerator.Current {
                get { return Current; }
            }

            public bool MoveNext() {
                while (_RowEnumerator == null || !_RowEnumerator.MoveNext()) {
                    if (!_PartialSheetsEnumerator.MoveNext())
                        return false;

                    _RowEnumerator = _PartialSheetsEnumerator.Current.GetEnumerator();
                }
                return true;
            }

            public void Reset() {
                _PartialSheetsEnumerator.Reset();
                if (_RowEnumerator != null)
                    _RowEnumerator.Dispose();
                _RowEnumerator = null;
            }

            #endregion
        }
    }
}
