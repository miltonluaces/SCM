#region Imports

using System;
using System.Collections.Generic;
using System.Text;
using Statistics;

#endregion

namespace MonteCarlo {

    internal class ObsolCalculator {

        #region Fields

        private ConvolutionCalculator cc;
        private ConvProbCalc cpc;
        private Histogram ltFcstHist;
        private int leadTime;
        private int maxLeadTime;
        private int obsTime;
        private double restObs;

        #endregion

        #region Constructor

        internal ObsolCalculator(int maxLeadTime) {
            cc = new ConvolutionCalculator(); //poner parámetros del otro constructor
            this.maxLeadTime = maxLeadTime;
            this.leadTime = -1;
            this.obsTime = -1;
            this.restObs = -1;
        }

        #endregion

        #region internal Methods

        internal void LoadHistogram(Histogram ltFcstHist, int leadTime, int obsTime) {
            if (ltFcstHist.Count <= 1) { return; }
            this.ltFcstHist = ltFcstHist;
            this.leadTime = leadTime;
            this.obsTime = obsTime;
            double obsNLeadTimes = obsTime / Math.Min(leadTime, maxLeadTime);
            cc.LoadHistogram(ltFcstHist, obsNLeadTimes);
            cpc = new ConvProbCalc(cc, (int)obsNLeadTimes);
            restObs = 1 + (obsNLeadTimes - (int)obsNLeadTimes) / obsNLeadTimes;
        }
        
        /** Method:  Get an obsolescence forecast */
        internal double GetObsTimeFcst(double p) {
            if (obsTime == -1 || p < 80) { return 0; }
            if (ltFcstHist.Count <= 1) { return 0; }
            return cpc.GetPercentile(p);
        }


        /** Method:  Get obsolescence expected value (in units)  
        ltFcst -  stock previously calculated */
        internal double GetObsExpectedValue(double ltFcst) {
            if (obsTime == -1) { return 0; }
            int nSteps = 12;
            double expValue = 0.0;
            double step = 1;
            if (ltFcst > nSteps) { step = ltFcst / nSteps; }
            double low = 0;
            double up = step;
            double lowProb = 0;
            double upProb;
            while (up <= ltFcst) {
                upProb = cpc.Probability(up);
                expValue += (upProb - lowProb) * (ltFcst - low);
                low = up;
                up = low + step;
                lowProb = upProb;
            }
            
            if (double.IsNaN(expValue)) { throw new Exception("Cannot calculate obsolescence"); }
            if (expValue < 0) { return 0; } else { return expValue; }
        }


           
       /** Method:  Get obsolescence risk  
        stock -  a previously calculated stock 
        obsExpectedValue -  expected value for obsolescence */
        internal double GetObsRisk(double stock, double obsExpectedValue) {
            if (obsTime == -1 || stock < 1 || ltFcstHist.Freqs.Count == 0) { return 0.0; }
            try { return obsExpectedValue / stock; } 
            catch { return -1; }
        }

        #endregion

    }
}
