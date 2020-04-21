using SaintCoinach.Ex.Relational.Definition;

namespace SaintCoinach.Ex.Relational {
    public class RelationalColumn : Column {
        private bool _hasDefinition;
        private PositionedDataDefintion _Definition;

        #region Properties

        public new RelationalHeader Header { get { return (RelationalHeader)base.Header; } }

        public PositionedDataDefintion Definition {
            get {
                if (_hasDefinition)
                    return _Definition;

                if (Header.SheetDefinition != null) {
                    if (Header.SheetDefinition.TryGetDefinition(Index, out PositionedDataDefintion definition))
                        _Definition = definition;
                }

                _hasDefinition = true;
                return _Definition;
            }
        }

        public string Name {
            get {
                return Header.SheetDefinition?.GetColumnName(Index);
            }
        }

        public override string ValueType {
            get {
                SheetDefinition def = Header.SheetDefinition;
                if (def == null) return base.ValueType;

                string t = def.GetValueTypeName(Index);
                return t ?? base.ValueType;
            }
        }

        #endregion

        #region Constructors

        #region Constructor

        public RelationalColumn(RelationalHeader header, int index, byte[] buffer, int offset)
            : base(header, index, buffer, offset) { }

        #endregion

        #endregion

        #region Read

        public override object Read(byte[] buffer, IDataRow row) {
            object baseVal = base.Read(buffer, row);

            PositionedDataDefintion def = Definition;
            return def != null ? def.Convert(row, baseVal, Index) : baseVal;
        }

        public override object Read(byte[] buffer, IDataRow row, int offset) {
            object baseVal = base.Read(buffer, row, offset);

            PositionedDataDefintion def = Definition;
            return def != null ? def.Convert(row, baseVal, Index) : baseVal;
        }

        #endregion

        public override string ToString() {
            return Name ?? Index.ToString();
        }
    }
}
