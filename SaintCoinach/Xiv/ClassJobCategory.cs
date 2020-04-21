using System;
using System.Collections.Generic;

using SaintCoinach.Ex.Relational;

namespace SaintCoinach.Xiv {
    public class ClassJobCategory : XivRow {
        #region Fields

        private ClassJob[] _ClassJobs;

        #endregion

        #region Properties

        public Text.XivString Name { get { return AsString("Name"); } }
        public IEnumerable<ClassJob> ClassJobs { get { return _ClassJobs ?? (_ClassJobs = BuildClassJobs()); } }

        #endregion

        #region Constructors

        #region Constructor

        public ClassJobCategory(IXivSheet sheet, IRelationalRow sourceRow) : base(sheet, sourceRow) { }

        #endregion

        #endregion

        #region Build

        private ClassJob[] BuildClassJobs() {
            const int ColumnOffset = 1;

            List<ClassJob> cjs = new List<ClassJob>();
            IXivSheet<ClassJob> cjSheet = Sheet.Collection.GetSheet<ClassJob>();
            foreach (ClassJob cj in cjSheet) {
                bool isValid = Convert.ToBoolean(this[ColumnOffset + cj.Key]);
                if (isValid)
                    cjs.Add(cj);
            }
            return cjs.ToArray();
        }

        #endregion

        public override string ToString() {
            return Name;
        }
    }
}
