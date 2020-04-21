using Newtonsoft.Json.Linq;
using SaintCoinach.Ex.Relational.Definition;
using SaintCoinach.Xiv;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaintCoinach.Ex.Relational.ValueConverters {
    public class TomestoneOrItemReferenceConverter : IValueConverter {
        static Dictionary<int, Item> _tomestoneKeyByRewardIndex;

        #region IValueConverter Members

        public string TargetTypeName => "Item";
        public Type TargetType => typeof(IRelationalRow);

        public object Convert(IDataRow row, object rawValue) {
            if (_tomestoneKeyByRewardIndex == null)
                _tomestoneKeyByRewardIndex = BuildTomestoneRewardIndex(row.Sheet.Collection);

            int key = System.Convert.ToInt32(rawValue);
            if (_tomestoneKeyByRewardIndex.TryGetValue(key, out Item item))
                return item;

            ISheet items = row.Sheet.Collection.GetSheet("Item");
            return items.ContainsRow(key) ? items[key] : rawValue;
        }

        #endregion

        private Dictionary<int, Item> BuildTomestoneRewardIndex(ExCollection coll) {
            Dictionary<int, Item> index = new Dictionary<int, Item>();

            ISheet sheet = coll.GetSheet("TomestonesItem");
            foreach (XivRow row in sheet) {
                int rewardIndex = (int)row.GetRaw(2); // For compatibility only.
                if (rewardIndex > 0)
                    index[rewardIndex] = row.As<Item>();
            }

            return index;
        }

        #region Serialization

        public JObject ToJson() {
            return new JObject() {
                ["type"] = "tomestone"
            };
        }

        public static TomestoneOrItemReferenceConverter FromJson(JToken obj) {
            return new TomestoneOrItemReferenceConverter();
        }

        public void ResolveReferences(SheetDefinition sheetDef) { }

        #endregion
    }
}
