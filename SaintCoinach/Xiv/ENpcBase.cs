using System;
using System.Collections.Generic;
using SaintCoinach.Ex.Relational;

namespace SaintCoinach.Xiv {
    public class ENpcBase : XivRow {
        #region Static

        public const int DataCount = 32;

        #endregion

        #region Fields

        private IRelationalRow[] _AssignedData;

        #endregion

        #region Properties
        public int ModelID { get { return AsInt32("ModelID"); } }
        public int Race { get { return AsInt32("Race"); } }
        public int Gender { get { return AsInt32("Gender"); } }
        public int BodyType { get { return AsInt32("BodyType"); } }
        public int Height { get { return AsInt32("Height"); } }
        public int Tribe { get { return AsInt32("Tribe"); } }
        public int Face { get { return AsInt32("Face"); } }
        public int HairStyle { get { return AsInt32("HairStyle"); } }
        public int HairHighlight { get { return AsInt32("HairHighlight"); } }
        public int SkinColor { get { return AsInt32("SkinColor"); } }
        public int EyeHeterochromia { get { return AsInt32("EyeHeterochromia"); } }
        public int HairColor { get { return AsInt32("HairColor"); } }
        public int HairHighlightColor { get { return AsInt32("HairHighlightColor"); } }
        public int FacialFeature { get { return AsInt32("FacialFeature"); } }
        public int FacialFeatureColor { get { return AsInt32("FacialFeatureColor"); } }
        public int Eyebrows { get { return AsInt32("Eyebrows"); } }
        public int EyeColor { get { return AsInt32("EyeColor"); } }
        public int EyeShape { get { return AsInt32("EyeShape"); } }
        public int Nose { get { return AsInt32("Nose"); } }
        public int Jaw { get { return AsInt32("Jaw"); } }
        public int Mouth { get { return AsInt32("Mouth"); } }
        public int LipColor { get { return AsInt32("LipColor"); } }
        public int BustOrTone1 { get { return AsInt32("BustOrTone1"); } }

        public int ExtraFeature1 { get { return AsInt32("ExtraFeature1"); } }
        public int ExtraFeature2OrBust { get { return AsInt32("ExtraFeature2OrBust"); } }
        public int FacePaint { get { return AsInt32("FacePaint"); } }
        public int FacePaintColor { get { return AsInt32("FacePaintColor"); } }


        public Quad ModelMain { get { return AsQuad("Model{MainHand}"); } }
        public int DyeMain { get { return AsInt32("Dye{MainHand}"); } }
        public Quad ModelSub { get { return AsQuad("Model{OffHand}"); } }
        public int DyeOff { get { return AsInt32("Dye{OffHand}"); } }
        public int[] ModelHead { get { return convertToIntArray("Model{Head}"); } }
        public int[] ModelBody { get { return convertToIntArray("Model{Body}"); } }
        public int[] ModelHands { get { return convertToIntArray("Model{Hands}"); } }
        public int[] ModelLegs { get { return convertToIntArray("Model{Legs}"); } }
        public int[] ModelFeet { get { return convertToIntArray("Model{Feet}"); } }
        public int[] ModelEars { get { return convertToIntArray("Model{Ears}"); } }
        public int[] ModelNeck { get { return convertToIntArray("Model{Neck}"); } }
        public int[] ModelWrists { get { return convertToIntArray("Model{Wrists}"); } }
        public int[] ModelLeftRing { get { return convertToIntArray("Model{LeftRing}"); } }
        public int[] ModelRightRing { get { return convertToIntArray("Model{RightRing}"); } }
        public int DyeHead { get { return AsInt32("Dye{Head}"); } }
        public int DyeBody { get { return AsInt32("Dye{Body}"); } }
        public int DyeHands { get { return AsInt32("Dye{Hands}"); } }
        public int DyeLegs { get { return AsInt32("Dye{Legs}"); } }
        public int DyeFeet { get { return AsInt32("Dye{Feet}"); } }
        public int DyeEars { get { return AsInt32("Dye{Ears}"); } }
        public int DyeNeck { get { return AsInt32("Dye{Neck}"); } }
        public int DyeWrists { get { return AsInt32("Dye{Wrists}"); } }
        public int DyeLeftRing { get { return AsInt32("Dye{LeftRing}"); } }
        public int DyeRightRing { get { return AsInt32("Dye{RightRing}"); } }
        public NpcEquip NpcEquip { get { return As<NpcEquip>(); } }

        public IEnumerable<IRelationalRow> AssignedData { get { return _AssignedData ?? (_AssignedData = BuildAssignedData()); } }

        #endregion

        #region Constructors

        public ENpcBase(IXivSheet sheet, IRelationalRow sourceRow) : base(sheet, sourceRow) { }
        
        #endregion

        public IRelationalRow GetData(int index) {
            return As<IRelationalRow>("ENpcData", index);
        }
        public int GetRawData(int index) {
            var fulCol = BuildColumnName("ENpcData", index);
            var raw = ((IRelationalRow)this).GetRaw(fulCol);
            return Convert.ToInt32(raw);
        }

        private IRelationalRow[] BuildAssignedData() {
            var data = new List<IRelationalRow>();

            for (var i = 0; i < ENpcBase.DataCount; ++i) {
                var val = GetData(i);
                if (val != null)
                    data.Add(val);
            }

            return data.ToArray();
        }
    }
}
