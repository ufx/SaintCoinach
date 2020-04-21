using System.Collections.Generic;

using SaintCoinach.Ex.Relational;

namespace SaintCoinach.Xiv {
    /// <summary>
    ///     Class for bonuses granted by consumable items.
    /// </summary>
    public class ItemFood : XivRow, IParameterObject {
        #region Fields

        /// <summary>
        ///     <see cref="ParameterCollection" /> of the bonuses granted by the current consumable.
        /// </summary>
        private ParameterCollection _Parameters;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ItemFood" /> class.
        /// </summary>
        /// <param name="sheet"><see cref="IXivSheet" /> containing this object.</param>
        /// <param name="sourceRow"><see cref="IRelationalRow" /> to read data from.</param>
        public ItemFood(IXivSheet sheet, IRelationalRow sourceRow) : base(sheet, sourceRow) { }

        #endregion

        /// <summary>
        ///     Gets the parameter bonuses granted the current consumable.
        /// </summary>
        /// <value>The parameter bonuses granted the current consumable.</value>
        public IEnumerable<Parameter> Parameters { get { return _Parameters ?? (_Parameters = BuildParameters()); } }

        #region Build

        /// <summary>
        ///     Build a <see cref="ParameterCollection" /> for the bonuses granted by the current consumable.
        /// </summary>
        /// <returns>A <see cref="ParameterCollection" /> for the bonuses granted by the current consumable.</returns>
        private ParameterCollection BuildParameters() {
            const int Count = 3;

            ParameterCollection parameters = new ParameterCollection();
            for (int i = 0; i < Count; ++i) {
                BaseParam param = As<BaseParam>(i);
                if (param.Key == 0)
                    continue;

                bool isRel = AsBoolean("IsRelative", i);
                int val = AsInt32("Value", i);
                int valHq = AsInt32("Value{HQ}", i);

                if (isRel) {
                    int max = AsInt32("Max", i);
                    int maxHq = AsInt32("Max{HQ}", i);

                    parameters.AddParameterValue(param,
                        max == 0
                            ? new ParameterValueRelative(ParameterType.Base, val / 100.0, i)
                            : new ParameterValueRelativeLimited(ParameterType.Base, val / 100.0, max, i));

                    if (maxHq == max && valHq == val) continue;

                    parameters.AddParameterValue(param,
                        maxHq == 0
                            ? new ParameterValueRelative(ParameterType.Hq, valHq / 100.0, i)
                            : new ParameterValueRelativeLimited(ParameterType.Hq, valHq / 100.0, maxHq, i));
                } else {
                    parameters.AddParameterValue(param, new ParameterValueFixed(ParameterType.Base, val, i));
                    if (val != valHq)
                        parameters.AddParameterValue(param, new ParameterValueFixed(ParameterType.Hq, valHq, i));
                }
            }
            return parameters;
        }

        #endregion
    }
}
