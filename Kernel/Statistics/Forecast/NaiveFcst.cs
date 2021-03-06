#region Imports

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Statistics;
using Maths;

#endregion

namespace Statistics {
    
    internal class NaiveFcst : TsForecast   {

        #region Fields
        
        private int movingWindow;
        private StatFunctions stat;
        private AverageType average;
        private double mean;
        
        #endregion

        #region Constructor

        internal NaiveFcst(int movingWindow, AverageType average) {
            this.fcstMethod = FcstMethodType.Naive;
            this.movingWindow = movingWindow;
            this.stat = new StatFunctions();
            this.average = average;
        }

        #endregion

        #region TsForecast Implementation

        public override void Calculate()  {
            switch (average) { 
                case AverageType.simple:
                    mean = stat.Mean(hist, hist.Count - movingWindow - 1, hist.Count - 1);
                    break;
                case AverageType.weighted:
                    break;
            }
        }

        public override double[] GetFcst(int horizon)  {
           fcst = new double[horizon];
            for (int i = 0; i < horizon; i++) { fcst[i] = mean; }
            return fcst;
        }

        public override void SetModel(object model)  {
            throw new NotImplementedException();
        }

        public override object GetModel()  {
            throw new NotImplementedException();
        }
        
        #endregion

        #region Private Methods
        #endregion

        #region Enums

        internal enum AverageType { simple, weighted };

        #endregion

    }
}
