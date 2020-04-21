using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotSquish {
    internal class ClusterFit : ColourFit {
        const int MaxIterations = 8;

        #region Fields
        private int _IterationCount;
        private Vector3 _Principle;
        private byte[] _Order = new byte[16 * MaxIterations];
        private Vector4[] _PointsWeight = new Vector4[16];
        private Vector4 _XSumWSum;
        private Vector4 _Metric;
        private Vector4 _BestError;
        #endregion

        #region Constructor
        protected ClusterFit(ColourSet colours, SquishOptions flags) : base(colours, flags) {
            // Set the iteration count.
            this._IterationCount = flags.HasFlag(SquishOptions.ColourIterativeClusterFit) ? MaxIterations : 1;

            // Initialise the best error.
            this._BestError = new Vector4(float.MaxValue);

            // Initialize the metric
            bool perceptual = flags.HasFlag(SquishOptions.ColourMetricPerceptual);
            if (perceptual)
                this._Metric = new Vector4(0.2126f, 0.7152f, 0.0722f, 0.0f);
            else
                this._Metric = new Vector4(1.0f);

            // Get the covariance matrix.
            Sym3x3 covariance = Sym3x3.ComputeWeightedCovariance(colours.Count, colours.Points, colours.Weights);

            // Compute the principle component
            this._Principle = Sym3x3.ComputePrincipledComponent(covariance);
        }
        #endregion

        #region Methods
        protected bool ConstructOrdering(Vector3 axis, int iteration) {
            // Build the list of dot products.
            float[] dps = new float[16];
            int ordOff = 16 * iteration;
            for (int i = 0; i < _Colours.Count; ++i) {
                dps[i] = Vector3.Dot(_Colours.Points[i], axis);
                this._Order[ordOff + i] = (byte)i;
            }

            // Stable sort using them.
            for (int i = 0; i < _Colours.Count; ++i) {
                for (int j = i; j > 0 && dps[j] < dps[j - 1]; --j) {
                    float _dps = dps[j];
                    byte _order = _Order[ordOff + j];

                    dps[j] = dps[j - 1];
                    dps[j - 1] = _dps;
                    _Order[ordOff + j] = _Order[ordOff + j - 1];
                    _Order[ordOff + j - 1] = _order;
                }
            }

            // Check this ordering is unique
            for (int it = 0; it < iteration; ++it) {
                int prevOff = 16 * it;
                bool same = true;
                for (int i = 0; i < _Colours.Count; ++i) {
                    if (_Order[ordOff + i] != _Order[prevOff + i]) {
                        same = false;
                        break;
                    }
                }
                if (same)
                    return false;
            }

            // Copy the ordering and weight all the points
            this._XSumWSum = new Vector4(0f);
            for (int i = 0; i < _Colours.Count; ++i) {
                byte j = _Order[ordOff + i];
                Vector4 p = new Vector4(_Colours.Points[j].X, _Colours.Points[j].Y, _Colours.Points[j].Z, 1f);
                Vector4 w = new Vector4(_Colours.Weights[j]);
                Vector4 x = p * w;
                this._PointsWeight[i] = x;
                this._XSumWSum += x;
            }
            return true;
        }

        protected override void Compress3(byte[] block) {
            // Declare variables
            int count = _Colours.Count;
            Vector4 zero = new Vector4(0f);
            Vector4 half = new Vector4(.5f);
            Vector4 one = new Vector4(1f);
            Vector4 two = new Vector4(2f);
            Vector4 half_half2 = new Vector4(.5f, .5f, .5f, .25f);
            Vector4 grid = new Vector4(31f, 63f, 31f, 0f);
            Vector4 gridrcp = new Vector4(1f / 31f, 1f / 63f, 1f / 31f, 0f);

            // Prepare the ordering using the principle axis.
            ConstructOrdering(_Principle, 0);

            // Check all possible clusters and iterate on the total order.
            Vector4 bestStart = zero;
            Vector4 bestEnd = zero;
            Vector4 bestError = this._BestError;
            byte[] bestIndices = new byte[16];
            //var bestIteration = 0;
            //int besti = 0, bestj = 0;

            //// Loop over iterations (we avoid the case that all points in first or last cluster)
            //for (int iterationIndex = 0; ; ) {
            //    throw new NotImplementedException();
            //}

            throw new NotImplementedException();
        }
        protected override void Compress4(byte[] block) {
            throw new NotImplementedException();
        }
        #endregion
    }
}
