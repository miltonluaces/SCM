using System.Collections.Generic;
using System;

namespace Maths {
    
    internal abstract class TsForecast : ITsForecast  {

        #region Fields

        protected FcstMethodType fcstMethod;
        protected FcstResType fcstRes;
        protected List<double> hist;
        protected double[] fcst;
        protected List<double> testSet;
        protected int validPrd;

        #endregion

        #region Constructor

        internal TsForecast() { }

        #endregion

        #region ITsForecast Implementation

        FcstMethodType ITsForecast.GetFcstMethod()  {
            return fcstMethod;
        }

        void ITsForecast.LoadData(IList<double> hist, int iniIndex)  {
            this.hist = new List<double>();
            for (int i = iniIndex; i < hist.Count; i++) { this.hist.Add(hist[i]); }
        }

        void ITsForecast.LoadValid(IList<double> hist, int validPrd) {
            this.validPrd = validPrd;
            this.testSet = new List<double>();
            this.hist = new List<double>();
            for(int i = 0; i < hist.Count - validPrd; i++) { this.hist.Add(hist[i]); }
            for (int i = hist.Count - validPrd; i < hist.Count; i++) { this.testSet.Add(hist[i]); }
        }

        void ITsForecast.SetFcstRes(FcstResType fcstRes)  {
            this.fcstRes = fcstRes;
        }
        
        FcstResType ITsForecast.GetFcstRes()  {
            return fcstRes;
        }

        double ITsForecast.GetMetric(FcstMetric metric) {
            double[] fcst = GetFcst(validPrd);
            double sumDif = 0, sumTSet = 0, sumFcst = 0, sumSqTSet = 0, sumSqFcst = 0, sumProd = 0;
            for(int i=0;i<fcst.Length;i++) {
                switch (metric) {
                    case FcstMetric.ME: sumDif += (fcst[i] - testSet[i]);  break;
                    case FcstMetric.MAE: sumDif += Math.Abs(fcst[i] - testSet[i]); break;
                    case FcstMetric.MAPE: sumDif += Math.Abs(fcst[i] - testSet[i]); sumFcst += fcst[i]; break;
                    case FcstMetric.MSE: sumDif += Math.Pow(fcst[i] - testSet[i], 2); break;
                    case FcstMetric.R2: 
                        sumTSet += testSet[i]; sumSqTSet += Math.Pow(testSet[i], 2); 
                        sumFcst += fcst[i]; sumSqFcst += Math.Pow(fcst[i], 2); 
                        break;
                }
            }

            switch (metric) {
                case FcstMetric.ME:
                    return sumDif / (double)validPrd;
                case FcstMetric.MAE:
                    return sumDif / (double)validPrd;
                case FcstMetric.MAPE:
                    return sumDif / (sumFcst * (double)validPrd);
                case FcstMetric.MSE:
                    return Math.Sqrt(sumDif) / (double)validPrd;
                case FcstMetric.R2:
                double meanFcst = sumFcst/validPrd;
                double meanTSet = sumTSet / validPrd;
                double sdFcst = Math.Sqrt((validPrd * sumSqFcst - (sumFcst * sumFcst)) / (validPrd * validPrd - 1));
                double sdTSet = Math.Sqrt((validPrd * sumSqTSet - (sumTSet * sumTSet)) / (validPrd * validPrd - 1));
                for (int i = 0; i < validPrd; i++) { sumProd += ((fcst[i] - meanFcst) * (testSet[i] - meanTSet)); }
                return Math.Pow((sumProd / validPrd) / (sdTSet * sdFcst), 2);
            }
            return -1;
        }
   
        public abstract void Calculate();         
        public abstract double[] GetFcst(int horizon);
        public abstract void SetModel(object model);
        public abstract object GetModel();
        
        #endregion
    }
}
