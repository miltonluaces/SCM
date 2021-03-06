#region Imports

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Maths;

#endregion

namespace Statistics {

    internal class HoltWintersFcst : TsForecast  {

        #region Fields

        private double a0;
        private double b0;
        private double[] a;
        private double[] b;
        private double[] L;
        private double[] T;
        private int n;
        
        #endregion

        #region Constructor

        internal HoltWintersFcst(double a0, double b0) {
            this.fcstMethod = FcstMethodType.HoltWinters;
            this.a0 = a0;
            this.b0 = b0;
        }

        #endregion

        #region TsForecast Implementation

        public override void Calculate()  {
            List<double> Y = this.hist;
            n = Y.Count;
            L = new double[n];
            T = new double[n];
            a = new double[n];
            b = new double[n];
    
            L[1] = Y[1];
            T[1] = Y[1] - Y[0];
            for (int t = 2; t < Y.Count; t++)  {
                if (Y[t - 1] == 0) { a[t] = a0; }
                else { a[t] = AbsTrunk((Y[t - 1] - L[t - 1]) / Y[t - 1]); }
                if (Y[t - 1] - Y[t - 2] == 0) { b[t] = b0; }
                else { b[t] = AbsTrunk((Y[t - 1] - Y[t - 2] - T[t - 1]) / (Y[t - 1] - Y[t - 2])); }

                L[t] = a[t] * (L[t-1] + T[t-1]) + (1-a[t]) * Y[t-1];
                T[t] = b[t] * T[t-1] + (1-b[t]) * (L[t]-L[t-1]);
            }
        }
        
        public override double[] GetFcst(int horizon)  {
            fcst = new double[horizon];
            for (int k = 0; k < horizon; k++) {  fcst[k] =  L[n - 1] + k * T[n - 1];  }
            return fcst;
        }

        public override void SetModel(object model)   {
            throw new NotImplementedException();
        }

        public override object GetModel()   {
            throw new NotImplementedException();
        }

        #endregion
         
        #region Private Methods

        private double AbsTrunk(double parameter) {
            double absParam = Math.Abs(parameter);
            if (absParam >= 1) { absParam = 0.99999; }
            if (absParam <= 0) { absParam = 0.11111; }
            return absParam;
        }
        
        #endregion

    }
}
