using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaintCoinach.Text {
    using Expressions;
    using Nodes;
    using Xiv;

    public class DefaultEvaluationFunctionProvider : IEvaluationFunctionProvider {
        public delegate IExpression GenericTagEvaluator(EvaluationParameters parameters, Nodes.GenericElement element);

        #region Fields
        private Dictionary<TagType, GenericTagEvaluator> _GenericTagEvaluators = new Dictionary<TagType, GenericTagEvaluator>();
        private GenericTagEvaluator _DefaultGenericTagEvaluator;
        #endregion

        #region Properties
        public XivCollection Data { get; private set; }
        public GenericTagEvaluator DefaultGenericTagEvaluator {
            get { return _DefaultGenericTagEvaluator; }
            set {
                if (value == null)
                    throw new ArgumentNullException("value");
                _DefaultGenericTagEvaluator = value;
            }
        }
        #endregion

        #region Constructor
        public DefaultEvaluationFunctionProvider(XivCollection data) {
            if (data == null)
                throw new ArgumentNullException("data");
            this.Data = data;

            this.DefaultGenericTagEvaluator = EvaluateDefaultGenericElement;

            SetEvaluator(TagType.Time, EvaluateTime);
            SetEvaluator(TagType.Sheet, EvaluateSheet);
            SetEvaluator(TagType.SheetDe, EvaluateSheetWithAttributive);
            SetEvaluator(TagType.SheetEn, EvaluateSheetWithAttributive);
            SetEvaluator(TagType.SheetFr, EvaluateSheetWithAttributive);
            SetEvaluator(TagType.SheetJa, EvaluateSheetWithAttributive);
            SetEvaluator(TagType.Value, (p, e) => e.Content.TryEvaluate(p));
            SetEvaluator(TagType.TwoDigitValue, EvaluateTwoDigitValue);
            SetEvaluator(TagType.ZeroPaddedValue, EvaluateZeroPaddedValue);
        }
        #endregion

        #region Evals
        public void SetEvaluator(TagType tag, GenericTagEvaluator evaluator) {
            if (_GenericTagEvaluators.ContainsKey(tag))
                _GenericTagEvaluators[tag] = evaluator;
            else
                _GenericTagEvaluators.Add(tag, evaluator);
        }
        #endregion

        #region Generic

        public IExpression EvaluateGenericElement(EvaluationParameters parameters, Nodes.GenericElement element) {
            GenericTagEvaluator eval;
            _GenericTagEvaluators.TryGetValue(element.Tag, out eval);
            return (eval ?? DefaultGenericTagEvaluator)(parameters, element);
        }
        protected virtual IExpression EvaluateDefaultGenericElement(EvaluationParameters parameters, GenericElement element) {
            List<IExpression> items = new List<IExpression>();
            bool hasArgs = element.Arguments.Any();
            items.Add(new GenericExpression(StringTokens.TagOpen + element.Tag.ToString()));
            if (hasArgs)
                items.Add(new SurroundedExpression(StringTokens.ArgumentsOpen, new ExpressionCollection(element.Arguments.Select(_ => _.TryEvaluate(parameters))) { Separator = StringTokens.ArgumentsSeperator }, StringTokens.ArgumentsClose));

            if (element.Content == null) {
                items.Add(new GenericExpression(StringTokens.ElementClose + StringTokens.TagClose));
            } else {
                items.Add(new GenericExpression(StringTokens.TagClose));
                items.Add(element.Content.TryEvaluate(parameters));
                items.Add(new GenericExpression(StringTokens.TagOpen + StringTokens.ElementClose + element.Tag.ToString() + StringTokens.TagClose));
            }

            return new ExpressionCollection(items);
        }
        protected virtual IExpression EvaluateTwoDigitValue(EvaluationParameters parameters, Nodes.GenericElement element) {
            IExpression eval = element.Content.TryEvaluate(parameters);
            int intVal = ToInteger(eval);
            return new GenericExpression(intVal.ToString("D2"));
        }
        protected virtual IExpression EvaluateZeroPaddedValue(EvaluationParameters parameters, Nodes.GenericElement element) {
            IExpression lenEval = element.Arguments.First().TryEvaluate(parameters);
            int len = ToInteger(lenEval);
            IExpression eval = element.Content.TryEvaluate(parameters);
            int intVal = ToInteger(eval);

            return new GenericExpression(intVal.ToString("D" + len.ToString()));
        }
        protected virtual IExpression EvaluateTime(EvaluationParameters parameters, Nodes.GenericElement element) {
            /* Appears to set values for an input time.
             * - 222 / DEh  Year
             * - 221 / DDh  Month
             * - 220 / DCh  Day of week
             * - 219 / DBh  Day of month
             * - 218 / DAh  Hour
             * - 217 / D9h  Minute
             */

            IExpression argEval = element.Arguments.First().TryEvaluate(parameters);
            int argInt = ToInteger(argEval);

            DateTime utcTime = EorzeaDateTime.Zero.AddSeconds(argInt);
            DateTime localTime = utcTime.ToLocalTime();

            parameters.TopLevelParameters[0xDE] = localTime.Year;
            parameters.TopLevelParameters[0xDD] = localTime.Month;
            parameters.TopLevelParameters[0xDC] = 1 + (int)localTime.DayOfWeek;
            parameters.TopLevelParameters[0xDB] = localTime.Day;
            parameters.TopLevelParameters[0xDA] = localTime.Hour;
            parameters.TopLevelParameters[0xD9] = localTime.Minute;

            return GenericExpression.Empty;
        }

        protected virtual IExpression EvaluateSheet(EvaluationParameters parameters, Nodes.GenericElement element) {
            IExpression[] evalArgs = element.Arguments.Select(_ => _.TryEvaluate(parameters)).ToArray();
            if (evalArgs.Length < 2)
                throw new InvalidOperationException();
            string sheetName = evalArgs[0].ToString();
            int rowKey = ToInteger(evalArgs[1]);
            int colKey = 0;
            if (evalArgs.Length > 2)
                colKey = ToInteger(evalArgs[2]);

            XivRow row = Data.GetSheet(sheetName)[rowKey];
            object value = row[colKey];
            if (value is INode) {
                EvaluationParameters innerParams = new EvaluationParameters(parameters);
                innerParams.InputParameters.Clear();
                for (int i = 3; i < evalArgs.Length; ++i)
                    innerParams.InputParameters[i - 2] = evalArgs[i];


                value = EvaluationHelper.TryEvaluate((INode)value, innerParams);
            }

            return new GenericExpression(new ObjectWithDisplay(value, row));
        }

        const string AttributiveSheetName = "Attributive";
        static readonly Dictionary<TagType, int> AttributiveColumnOffsets = new Dictionary<TagType, int> {
            { TagType.SheetJa, 0 },
            { TagType.SheetEn, 2 },
            { TagType.SheetDe, 8 },
            { TagType.SheetFr, 24 },
        };
        static readonly Dictionary<TagType, Ex.Language> TagToLanguageMap = new Dictionary<TagType, Ex.Language> {
            { TagType.SheetJa, Ex.Language.Japanese },
            { TagType.SheetEn, Ex.Language.English },
            { TagType.SheetDe, Ex.Language.German },
            { TagType.SheetFr, Ex.Language.French },
        };
        protected virtual IExpression EvaluateSheetWithAttributive(EvaluationParameters parameters, Nodes.GenericElement element) {
            IExpression[] evalArgs = element.Arguments.Select(_ => _.TryEvaluate(parameters)).ToArray();
            if (evalArgs.Length < 3)
                throw new InvalidOperationException();

            Ex.Language lang = TagToLanguageMap[element.Tag];

            string sheetName = evalArgs[0].ToString();
            int attributiveRowKey = ToInteger(evalArgs[1]);
            int rowKey = ToInteger(evalArgs[2]);

            int columnKey = 0;
            if (evalArgs.Length > 3)
                columnKey = ToInteger(evalArgs[3]);

            int attributiveColumnKey = AttributiveColumnOffsets[element.Tag];
            if (evalArgs.Length > 4)
                attributiveColumnKey += ToInteger(evalArgs[4]);

            XivRow row = Data.GetSheet(sheetName)[rowKey];
            object value;
            if (row is Ex.IMultiRow)
                value = ((Ex.IMultiRow)row)[columnKey, lang];
            else
                value = row[columnKey];

            XivRow attributiveRow = Data.GetSheet(AttributiveSheetName)[attributiveRowKey];
            object attributiveValue;
            if (attributiveRow is Ex.IMultiRow)
                attributiveValue = ((Ex.IMultiRow)attributiveRow)[attributiveColumnKey, lang];
            else
                attributiveValue = attributiveRow[attributiveColumnKey];

            EvaluationParameters innerParams = new EvaluationParameters(parameters);
            innerParams.InputParameters.Clear();
            for (int i = 5; i < evalArgs.Length; ++i)
                innerParams.InputParameters[i - 2] = evalArgs[i];

            if (value is INode)
                value = EvaluationHelper.TryEvaluate((INode)value, innerParams);
            if (attributiveValue is INode)
                attributiveValue = EvaluationHelper.TryEvaluate((INode)attributiveValue, innerParams);

            return new SurroundedExpression(new ObjectWithDisplay(attributiveValue, attributiveRow), new ObjectWithDisplay(value, row), null);
        }

        #endregion

        #region Compare
        public bool Compare(EvaluationParameters parameters, DecodeExpressionType comparisonType, INode left, INode right) {
            IExpression evalLeft = left.TryEvaluate(parameters);
            IExpression evalRight = right.TryEvaluate(parameters);

            switch (comparisonType) {
                case DecodeExpressionType.GreaterThanOrEqualTo: {
                    int iLeft = ToInteger(evalLeft);
                    int iRight = ToInteger(evalRight);
                        return iLeft >= iRight;
                    }
                case DecodeExpressionType.GreaterThan: {
                    int iLeft = ToInteger(evalLeft);
                    int iRight = ToInteger(evalRight);
                        return iLeft > iRight;
                    }
                case DecodeExpressionType.LessThanOrEqualTo: {
                    int iLeft = ToInteger(evalLeft);
                    int iRight = ToInteger(evalRight);
                        return iLeft <= iRight;
                    }
                case DecodeExpressionType.LessThan: {
                    int iLeft = ToInteger(evalLeft);
                    int iRight = ToInteger(evalRight);
                        return iLeft < iRight;
                    }
                case DecodeExpressionType.Equal:
                    return ExpressionsEqual(evalLeft, evalRight);
                case DecodeExpressionType.NotEqual:
                    return !ExpressionsEqual(evalLeft, evalRight);
                default:
                    throw new NotSupportedException();
            }
        }

        protected bool ExpressionsEqual(IExpression left, IExpression right) {
            object checkLeft = GetFinalObect(left);
            object checkRight = GetFinalObect(right);
            return EqualsEx(checkLeft, checkRight);
        }

        protected object GetFinalObect(IExpression expr) {
            object result = expr;
            if (expr is IValueExpression) {
                result = ((IValueExpression)expr).Value;

                while (result is ObjectWithDisplay)
                    result = ((ObjectWithDisplay)result).Object;

                if (result is IExpression)
                    result = GetFinalObect((IExpression)result);
            }

            return result;
        }

        protected bool EqualsEx(object left, object right) {
            if (left == null && right == null)
                return true;
            if (left == null || right == null)
                return false;

            if (object.ReferenceEquals(left, right))
                return true;
            if (object.Equals(left, right))
                return true;

            if (left is Ex.IRow && right is Ex.IRow) {
                Ex.IRow lRow = left as Ex.IRow;
                Ex.IRow rRow = right as Ex.IRow;

                return lRow.Key == rRow.Key && lRow.Sheet.Name == rRow.Sheet.Name;
            }

            int iLeft, iRight;
            if (TryConvert(left, out iLeft) && TryConvert(right, out iRight))
                return iLeft == iRight;

            if (!(left is string) && !(left is string))
                return false;
            string sLeft = left.ToString();
            string sRight = right.ToString();

            return string.Equals(sLeft, sRight);
        }
        
        #endregion

        #region IEvaluationFunctionProvider Members

        public bool ToBoolean(IExpression value) {
            if (value == null)
                return false;
            IValueExpression asValue = value as IValueExpression;
            if (asValue != null) {
                bool boolVal;
                if (TryConvert(asValue.Value, out boolVal))
                    return boolVal;
            }

            string str = value.ToString().Trim();

            bool b;
            if (bool.TryParse(str, out b))
                return b;

            int i;
            if (int.TryParse(str, out i))
                return i != 0;

            return str.Length > 0;
        }

        public int ToInteger(IExpression value) {
            int result;
            if (!TryConvertToInteger(value, out result))
                throw new InvalidOperationException();
            return result;
        }
        public bool TryConvertToInteger(IExpression value, out int result) {
            if (value == null) {
                result = 0;
                return true;
            }
            IValueExpression asValue = value as IValueExpression;
            if (asValue != null) {
                int intVal;
                if (TryConvert(asValue.Value, out intVal)) {
                    result = intVal;
                    return true;
                }
            }

            string str = value.ToString().Trim();

            bool b;
            if (bool.TryParse(str, out b)) {
                result = b ? 1 : 0;
                return true;
            }

            if (int.TryParse(str, out result))
                return true;

            result = 0;
            return false;
        }

        bool TryConvert(object value, out bool result) {
            while (value is ObjectWithDisplay)
                value = (ObjectWithDisplay)value;

            if (value == null) {
                result = false;
                return true;
            }

            if (value is IExpression) {
                result = ToBoolean((IExpression)value);
                return true;
            }

            string asStr = value as string;
            if (asStr == null) {
                if (value is bool) {
                    result = (bool)value;
                    return true;
                }
                if (value is int) {
                    result = ((int)value) != 0;
                    return true;
                }

                asStr = value.ToString();
            }

            if (bool.TryParse(asStr, out result))
                return true;

            int intVal;
            if (int.TryParse(asStr, out intVal)) {
                result = intVal != 0;
                return true;
            }

            result = false;
            return false;
        }

        bool TryConvert(object value, out int result) {
            while (value is ObjectWithDisplay)
                value = (ObjectWithDisplay)value;

            if (value == null) {
                result = 0;
                return true;
            }

            if (value is IExpression) {
                result = ToInteger((IExpression)value);
                return true;
            }

            string asStr = value as string;
            if (asStr == null) {
                if (value is bool) {
                    result = ((bool)value) ? 1 : 0;
                    return true;
                }
                if (value is int) {
                    result = (int)value;
                    return true;
                }
                if (value is Ex.IRow) {
                    result = ((Ex.IRow)value).Key;
                    return true;
                }

                asStr = value.ToString();
            }

            bool boolVal;
            if (bool.TryParse(asStr, out boolVal)) {
                result = boolVal ? 1 : 0;
                return true;
            }

            if (int.TryParse(asStr, out result))
                return true;


            result = 0;
            return false;
        }

        #endregion
    }
}
