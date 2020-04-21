using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaintCoinach.Ex.Variant1 {
    using Relational;
    using System.Collections.Concurrent;

    public class RelationalDataRow : DataRow, IRelationalDataRow {
        #region Fields
        private ConcurrentDictionary<string, WeakReference<object>> _ValueReferences = new ConcurrentDictionary<string, WeakReference<object>>();
        #endregion

        public new IRelationalDataSheet Sheet { get { return (IRelationalDataSheet)base.Sheet; } }

        public override string ToString() {
            RelationalColumn defCol = Sheet.Header.DefaultColumn;
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
                RelationalColumn defCol = Sheet.Header.DefaultColumn;
                return defCol == null ? null : this[defCol.Index];
            }
        }

        public object this[string columnName] {
            get {
                WeakReference<object> valRef = _ValueReferences.GetOrAdd(columnName, c => new WeakReference<object>(GetColumnValue(c)));
                if (valRef.TryGetTarget(out object val))
                    return val;

                val = GetColumnValue(columnName);
                valRef.SetTarget(val);
                return val;
            }
        }

        private object GetColumnValue(string columnName) {
            RelationalColumn col = Sheet.Header.FindColumn(columnName);
            if (col == null)
                throw new KeyNotFoundException();
            return this[col.Index];
        }

        object IRelationalRow.GetRaw(string columnName) {
            RelationalColumn column = Sheet.Header.FindColumn(columnName);
            if (column == null)
                throw new KeyNotFoundException();
            return column.ReadRaw(Sheet.GetBuffer(), this);
        }

        #endregion
    }
}
