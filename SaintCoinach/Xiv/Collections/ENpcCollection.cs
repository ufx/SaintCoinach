using System.Collections;
using System.Collections.Generic;

namespace SaintCoinach.Xiv.Collections {
    public class ENpcCollection : IEnumerable<ENpc> {
        #region Fields

        private readonly Dictionary<int, ENpc> _Inner = new Dictionary<int, ENpc>();
        private Dictionary<int, List<ENpc>> _ENpcDataMap;

        #endregion

        #region Properties

        public XivCollection Collection { get; private set; }
        public IXivSheet<ENpcBase> BaseSheet { get; private set; }
        public IXivSheet<ENpcResident> ResidentSheet { get; private set; }

        #endregion

        #region Constructors

        public ENpcCollection(XivCollection xivCollection) {
            Collection = xivCollection;
            BaseSheet = xivCollection.GetSheet<ENpcBase>();
            ResidentSheet = xivCollection.GetSheet<ENpcResident>();
        }

        #endregion

        #region IEnumerable<ENpc> Members

        public IEnumerator<ENpc> GetEnumerator() {
            return new Enumerator(this);
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        #endregion

        #region Get

        public ENpc this[int key] {
            get { return Get(key); }
        }
        public ENpc Get(int key) {
            if (_Inner.ContainsKey(key))
                return _Inner[key];

            ENpc enpc = new ENpc(this, key);
            _Inner.Add(key, enpc);
            return enpc;
        }

        #endregion

        #region Enumerator

        private class Enumerator : IEnumerator<ENpc> {
            #region Fields

            private readonly IEnumerator<ENpcBase> _BaseEnumerator;
            private readonly ENpcCollection _Collection;

            #endregion

            #region Constructors

            #region Constructor

            public Enumerator(ENpcCollection collection) {
                _Collection = collection;
                _BaseEnumerator = collection.BaseSheet.GetEnumerator();
            }

            #endregion

            #endregion

            #region IEnumerator<ENpc> Members

            public ENpc Current { get; private set; }

            #endregion

            #region IDisposable Members

            public void Dispose() {
                _BaseEnumerator.Dispose();
            }

            #endregion

            #region IEnumerator Members

            object IEnumerator.Current { get { return Current; } }

            public bool MoveNext() {
                bool result = _BaseEnumerator.MoveNext();
                Current = result ? _Collection.Get(_BaseEnumerator.Current.Key) : null;
                return result;
            }

            public void Reset() {
                Current = null;
                _BaseEnumerator.Reset();
            }

            #endregion
        }

        #endregion

        #region Find

        public IEnumerable<ENpc> FindWithData(int value) {
            if (_ENpcDataMap == null)
                _ENpcDataMap = BuildDataMap();
            if (_ENpcDataMap.ContainsKey(value))
                return _ENpcDataMap[value];
            return new ENpc[0];
        }

        private Dictionary<int, List<ENpc>> BuildDataMap() {
            Dictionary<int, List<ENpc>> dataMap = new Dictionary<int, List<ENpc>>();

            foreach (ENpc npc in this) {
                for (int i = 0; i < ENpcBase.DataCount; ++i) {
                    int val = npc.Base.GetRawData(i);
                    if (val == 0) continue;
                    if (!dataMap.TryGetValue(val, out List<ENpc> l))
                        dataMap.Add(val, l = new List<ENpc>());
                    l.Add(npc);
                }
            }

            return dataMap;
        }

        #endregion
    }
}
