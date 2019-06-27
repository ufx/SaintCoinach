﻿using SaintCoinach.Ex.Relational;

namespace SaintCoinach.Xiv {
    public class NpcEquip : XivRow {
        #region Properties
        public Quad ModelMain { get { return AsQuad("Model{MainHand}"); } }
        public Stain DyeMain { get { return As<Stain>("Dye{MainHand}"); } }

        public Quad ModelSub { get { return AsQuad("Model{OffHand}"); } }
        public Stain DyeOff { get { return As<Stain>("Dye{OffHand}"); } }
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
        public Stain DyeHead { get { return As<Stain>("Dye{Head}"); } }
        public Stain DyeBody { get { return As<Stain>("Dye{Body}"); } }
        public Stain DyeHands { get { return As<Stain>("Dye{Hands}"); } }
        public Stain DyeLegs { get { return As<Stain>("Dye{Legs}"); } }
        public Stain DyeFeet { get { return As<Stain>("Dye{Feet}"); } }
        public Stain DyeEars { get { return As<Stain>("Dye{Ears}"); } }
        public Stain DyeNeck { get { return As<Stain>("Dye{Neck}"); } }
        public Stain DyeWrists { get { return As<Stain>("Dye{Wrists}"); } }
        public Stain DyeLeftRing { get { return As<Stain>("Dye{LeftRing}"); } }
        public Stain DyeRightRing { get { return As<Stain>("Dye{RightRing}"); } }

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
