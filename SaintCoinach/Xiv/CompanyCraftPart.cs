using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaintCoinach.Xiv {
    public class CompanyCraftPart : XivRow {
        #region Fields
        private CompanyCraftProcess[] _CraftProcesses;
        #endregion

        #region Properties

        public CompanyCraftType CompanyCraftType { get { return As<CompanyCraftType>(); } }

        public IEnumerable<CompanyCraftProcess> CompanyCraftProcesses { get { return _CraftProcesses ?? (_CraftProcesses = BuildCraftProcesses()); } }

        #endregion

        #region Constructors

        public CompanyCraftPart(IXivSheet sheet, SaintCoinach.Ex.Relational.IRelationalRow sourceRow) : base(sheet, sourceRow) { }

        #endregion

        private CompanyCraftProcess[] BuildCraftProcesses() {
            const int Count = 3;

            List<CompanyCraftProcess> procs = new List<CompanyCraftProcess>();

            for(int i = 0; i < Count; ++i) {
                CompanyCraftProcess proc = As<CompanyCraftProcess>(i);
                if (proc == null || proc.Key == 0)
                    continue;

                procs.Add(proc);
            }

            return procs.ToArray();
        }
    }
}
