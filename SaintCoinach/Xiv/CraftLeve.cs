using System.Collections.Generic;

using SaintCoinach.Ex.Relational;

namespace SaintCoinach.Xiv {
    public class CraftLeve : XivRow {
        #region Fields

        private CraftLeveItem[] _Items;

        #endregion

        #region Properties

        public Leve Leve { get { return As<Leve>(); } }
        public int Repeats { get { return AsInt32("Repeats"); } }
        public IEnumerable<CraftLeveItem> Items { get { return _Items ?? (_Items = BuildItems()); } }

        #endregion

        #region Constructors

        #region Constructor

        public CraftLeve(IXivSheet sheet, IRelationalRow sourceRow) : base(sheet, sourceRow) { }

        #endregion

        #endregion

        #region Build

        private CraftLeveItem[] BuildItems() {
            const int Count = 4;

            List<CraftLeveItem> items = new List<CraftLeveItem>();

            for (int i = 0; i < Count; ++i) {
                int count = AsInt32("ItemCount", i);
                if (count == 0)
                    continue;

                Item item = As<Item>("Item", i);
                if (item.Key == 0)
                    continue;

                items.Add(new CraftLeveItem(item, count, false));
            }

            return items.ToArray();
        }

        #endregion
    }
}
