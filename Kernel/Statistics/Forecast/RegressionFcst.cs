#region Imports

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Statistics;
using Maths;

#endregion

namespace Statistics {

    internal class RegressionFcst : TsForecast   {

        #region Fields
        
        private int n;
        private int grade;
        private Polynom poly;
        private List<double> coeffs;
        
        #endregion

        #region Constructor

        public RegressionFcst(int grade) {
            this.fcstMethod = FcstMethodType.Regression;
            this.poly = new Polynom();
            this.grade = grade;
            this.coeffs = null;
        }

        #endregion

        #region TsForecast Implementation

        public override void Calculate()  {
            List<double> Y = this.hist;
            this.n = Y.Count;
            List<double> X = new List<double>();
            for (int i = 0; i < Y.Count; i++) { X.Add((double)i); }
            coeffs = poly.Regression(X, Y, grade);
        }

        public override double[] GetFcst(int horizon)  {
            List<double> X = new List<double>();
            for(int i=n;i<n + horizon;i++) { X.Add((double)i);  }
            List<double> regValues = poly.GetRegValues(X, coeffs);
            fcst = regValues.ToArray();
            return fcst;
        }

        public override void SetModel(object model) {
            throw new NotImplementedException();
        }

        public override object GetModel() {
            throw new NotImplementedException();
        }
        
        #endregion

        #region Private Methods
        #endregion
    }
}
