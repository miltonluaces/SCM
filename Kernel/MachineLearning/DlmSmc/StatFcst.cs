#region Imports

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Maths;

#endregion

namespace MachineLearning {

    internal class StatFcst : TsForecast {

        #region Fields

        private DLM dlm;
        private ModelDesign md;
        private List<ModelDesign.Harm> harms;
        private int testSetSize;
 
        #endregion

        #region Constructor

        internal StatFcst(int testSetSize) {
            this.fcstMethod = FcstMethodType.DLMKalman;
            this.dlm = new DLM(0, 0);
            this.md = new ModelDesign();
            this.harms = new List<ModelDesign.Harm>();
            this.testSetSize = testSetSize;
        }

        #endregion

        #region Properties

        internal IList<double> Hist {
            get { return hist; }
        }

        #endregion

        #region Internal Methods

        #region ITsForecast Implementation

        public override void Calculate()  {
            if (testSetSize * 2 > hist.Count) { testSetSize = (int)(hist.Count / 2); }
            double perConfHarm = 0.90;
            double alpha = 1 - perConfHarm / 100.0;
            //harms = md.SelectHarmonics(hist, testSetSize, minHarmImprov);
            harms = md.SelectHarmonicsForwBack(hist, testSetSize, alpha);
            this.dlm = md.BuildModel(hist, harms);
            this.dlm.Iterate(hist);
        }

        public override double[] GetFcst(int horizon)  {
            fcst = GetFcstsMean(horizon);
            return fcst;
        }

        public override void SetModel(object model)  {
            throw new NotImplementedException();
        }

        public override object GetModel()  {
            throw new NotImplementedException();
        }
        
        #endregion
        
        #region Fcst Methods
    
        internal double GetFcstMean() { return dlm.GetFcstMean(); }

        internal double GetFcstMean(int k) { return dlm.GetFcstMean(k); }

        internal double[] GetFcstsMean(int k) { return dlm.GetFcstsMean(k); }

        internal double GetFcstVar() { return Math.Sqrt(dlm.GetFcstVar()); }

        internal double GetFcstVar(int k) { return Math.Sqrt(dlm.GetFcstVar(k)); }

        #endregion

        #region Monitoring

        internal double[] GetRetroFcstMean(int t) {
            return dlm.GetRetroFcstsMean(t);
        }

        internal double[] GetRetroFcstsError(int t) {
            return dlm.GetRetroFcstsError(t);
        }

        internal double GetCrossValMae(int t) {
            return dlm.GetCrossValMae(t);
        }
        
        #endregion

        #endregion

        #region Private Methods

        #endregion

    }
}
