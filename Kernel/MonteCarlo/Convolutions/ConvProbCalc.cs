#region Imports

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Statistics;
using Maths;

#endregion

namespace MonteCarlo {

    /** Class:  Class for convolution probability calculations  */
    internal class ConvProbCalc {

        #region Fields

        private double nLeadTimes;
        private Dictionary<int, double> percentiles;
        private RootSearch rs;
        private double minInt;
        private double maxInt;
        private bool bisection;
        private ConvolutionCalculator cc;
        private double min;
        private double max;


        #endregion

        #region Constructor

        internal ConvProbCalc(ConvolutionCalculator cc, double nLeadTimes)
        {
            this.cc = cc;
            this.nLeadTimes = nLeadTimes;
            this.rs = new RootSearch(0.1, 0.01, 100);
            this.bisection = false;

            min = double.MaxValue;
            max = double.MinValue;
            foreach (double d in cc.Data)
            {
                if (d < min) { min = d; }
                if (d > max) { max = d; }
            }
            if (min == max)
            {
                this.maxInt = min;
                this.minInt = min;
                return;
            }
            this.maxInt = GetMaxInt();
            this.minInt = GetMinInt();
        }

        #endregion

        #region Properties


        /** Method:  Min value of interval  */
        internal double MinInt
        {
            get { return minInt; }
        }

        /** Method:  Max value of interval  */
        internal double MaxInt
        {
            get { return maxInt; }
        }

        internal bool Bisection
        {
            get { return bisection; }
            set { bisection = value; }
        }

        /** Method:  method used for calculation  */
        internal ConvolutionCalculator.MethodType Method
        {
            get { return cc.Method; }
        }

        #endregion

        #region Internal Methods

        /** Method:  Set percentile values of distribution  
        allPercentiles -  if all percentiles should be calculated */
        internal int CalcPercentiles(bool allPercentiles)
        {
            percentiles = new Dictionary<int, double>();
            for (int i = 800; i <= 1000; i++) { percentiles.Add(i, 0); }
            if (nLeadTimes >= (cc.MinNorm))
            {
                for (int i = 800; i <= 999; i++) { percentiles[i] = cc.Quantile(i / 1000.0); }
                percentiles[1000] = percentiles[999];
                return 200;
            }
            else
            {
                return rs.SetAllValues(percentiles, Probability, 80, 100, minInt, maxInt, bisection);
            }
        }

        /** Method:  Calculate probability of a particular value  */
        internal double Probability(double value)
        {
            return cc.ProbabilityAcum(value);
        }


        /** Method:  Get a pre-calculated percentile of a particular probability  */
        /// <returns> the percentile </returns>
        internal double GetPercentile(double p)
        {
            if (percentiles == null || percentiles.Count == 0) { return CalculatePercentile(p); }
            int key = GetKey(p);
            if (!percentiles.ContainsKey(key)) { throw new Exception(string.Format("Cannot find value " + p)); }
            return percentiles[key];
        }

        /** Method:  Calculate percentile of a particular probability  */
        /// <returns> the percentile </returns>
        internal double CalculatePercentile(double p)
        {
            return cc.Quantile(p / 100.0);
        }

        #endregion

        #region Private Methods

        private int GetKey(double val)
        {
            return Convert.ToInt32(Math.Round(val, 1) * 10);
        }

        private double GetMinInt()
        {
            int it = 0;
            minInt = rs.MonotoneBisection(Probability, true, min * nLeadTimes, maxInt, 0.8, 0.01, ref it, 100);
            return minInt;
        }

        private double GetMaxInt()
        {

            double convMin = min * nLeadTimes;
            int it = 0;
            double convMax = max * 1.1 * nLeadTimes;
            while (Probability(convMax) < 0.999 && it < 100)
            {
                convMin = convMax;
                if (Probability(convMax) == 0 && convMax > max * nLeadTimes) { return convMax; }
                convMax *= 1.1;
                it++;
            }
            while (Probability(convMax) > 0.99)
            {
                convMax *= 0.95;
            }

            maxInt = convMax * 1.05;
            return maxInt;
        }

        #endregion
    }
}
