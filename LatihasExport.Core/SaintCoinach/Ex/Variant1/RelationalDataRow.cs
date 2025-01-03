﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using SaintCoinach.Ex.Relational;

namespace SaintCoinach.Ex.Variant1 {
    public class RelationalDataRow : DataRow, IRelationalDataRow {
        #region Fields
        private ConcurrentDictionary<string, WeakReference<object>> _ValueReferences = new ConcurrentDictionary<string, WeakReference<object>>();
        #endregion

        public new IRelationalDataSheet Sheet { get { return (IRelationalDataSheet)base.Sheet; } }

        public override string ToString() {
            var defCol = Sheet.Header.DefaultColumn;
            return defCol == null
                       ? string.Format("{0}#{1}", Sheet.Header.Name, Key)
                       : string.Format("{0}", this[defCol.Index]);
        }

        #region Constructors

        public RelationalDataRow(IDataSheet sheet, int key, int offset) : base(sheet, key, offset) { }

        #endregion

        #region IRelationalRow Members

        IRelationalSheet IRelationalRow.Sheet { get { return Sheet; } }

        public object DefaultValue {
            get {
                var defCol = Sheet.Header.DefaultColumn;
                return defCol == null ? null : this[defCol.Index];
            }
        }

        public object this[string columnName] {
            get {
                var valRef = _ValueReferences.GetOrAdd(columnName, c => new WeakReference<object>(GetColumnValue(c)));
                if (valRef.TryGetTarget(out var val))
                    return val;

                val = GetColumnValue(columnName);
                valRef.SetTarget(val);
                return val;
            }
        }

        private object GetColumnValue(string columnName) {
            var col = Sheet.Header.FindColumn(columnName);
            if (col == null)
                throw new KeyNotFoundException();
            return this[col.Index];
        }

        object IRelationalRow.GetRaw(string columnName) {
            var column = Sheet.Header.FindColumn(columnName);
            if (column == null)
                throw new KeyNotFoundException();
            return column.ReadRaw(Sheet.GetBuffer(), this);
        }

        #endregion
    }
}
