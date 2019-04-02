using SaintCoinach.Ex.Relational;

namespace SaintCoinach.Xiv {
    public class NpcEquip : XivRow {
        #region Properties
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
        #endregion
        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="NpcEquip" /> class.
        /// </summary>
        /// <param name="sheet"><see cref="IXivSheet" /> containing this object.</param>
        /// <param name="sourceRow"><see cref="IRelationalRow" /> to read data from.</param>
        public NpcEquip(IXivSheet sheet, IRelationalRow sourceRow) : base(sheet, sourceRow) { }

        #endregion
    }
}
