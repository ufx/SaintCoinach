using Newtonsoft.Json.Linq;
using System;

namespace SaintCoinach.Ex.Relational.Definition {
    public class PositionedDataDefintion {
        #region Properties

        public IDataDefinition InnerDefinition { get; set; }

        public int Length { get { return InnerDefinition == null ? 0 : InnerDefinition.Length; } }

        public int Index { get; set; }

        #endregion

        public PositionedDataDefintion Clone() {
            PositionedDataDefintion clone = new PositionedDataDefintion {
                Index = Index,
                InnerDefinition = InnerDefinition.Clone()
            };


            return clone;
        }

        #region Things

        public object Convert(IDataRow row, object value, int index) {
            int innerIndex = index - Index;
            if (innerIndex < 0 || innerIndex >= Length)
                throw new ArgumentOutOfRangeException("index");

            return InnerDefinition.Convert(row, value, innerIndex);
        }

        public string GetName(int index) {
            int innerIndex = index - Index;
            if (innerIndex < 0 || innerIndex >= Length)
                throw new ArgumentOutOfRangeException("index");

            return InnerDefinition.GetName(innerIndex);
        }

        public string GetValueTypeName(int index) {
            int innerIndex = index - Index;
            if (innerIndex < 0 || innerIndex >= Length)
                throw new ArgumentOutOfRangeException("index");

            return InnerDefinition.GetValueTypeName(innerIndex);
        }

        public Type GetValueType(int index) {
            int innerIndex = index - Index;
            if (innerIndex < 0 || innerIndex >= Length)
                throw new ArgumentOutOfRangeException("index");

            return InnerDefinition.GetValueType(innerIndex);
        }

        #endregion

        #region Serialization

        public JObject ToJson() {
            JObject obj = InnerDefinition.ToJson();
            if (Index > 0)
                obj.AddFirst(new JProperty("index", Index));
            return obj;
        }

        public static PositionedDataDefintion FromJson(JToken obj) {
            return new PositionedDataDefintion() {
                Index = (int?)obj["index"] ?? 0,
                InnerDefinition = DataDefinitionSerializer.FromJson(obj)
            };
        }

        public void ResolveReferences(SheetDefinition sheetDef) {
            InnerDefinition.ResolveReferences(sheetDef);
        }

        #endregion
    }
}
