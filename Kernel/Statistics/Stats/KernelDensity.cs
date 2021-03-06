#region Imports

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Maths;

#endregion

namespace Statistics {

    /** Method:  Class for density estimation by gaussian density functions composition */
    internal class KernelDensity {

        #region Fields

        private Histogram histogram;
        private RootSearch rs;
        private NormalDistrib ns;
        private double stDev;
        private int maxIterations;
        private Dictionary<int, double> percentiles;
        private bool mini;
        private bool bisection;
        private List<double> zerosDeriv2;
        private double minInt;
        private double maxInt;
    
        #endregion

        #region Constructor

        internal KernelDensity(double stDev, int maxIterations, int maxClasses) {
            this.stDev = stDev;
            this.maxIterations = maxIterations;
            double epsilon = 0.005;
            double epsilonInterval = 0.05;
            histogram = new Histogram(maxClasses);
            percentiles = new Dictionary<int, double>();
            rs = new RootSearch(epsilon, epsilonInterval, maxIterations);
            ns = new NormalDistrib();
            mini = false;
            bisection = false;
            zerosDeriv2 = new List<double>();
        }

        #endregion

        #region Properties, Setters and Getters

        /** Method:  Maximum of iterations */
        internal int MaxIterations {
            get { return maxIterations; }
            set { maxIterations = value; }
        }

        /** Method:  Histogram of the distribution */
        internal Histogram Histogram {
            get { return histogram; }
            set {
                histogram = value;
            }
        }

        /** Method:  Get the frequency of any value */
        internal double GetHistogramFreq(int val) {
            return histogram.GetFreq(val);
        }

        /** Method:  Get percentile for any percentage */
        internal double GetPercentile(double perc) {
            int key = GetKey(perc);
            return percentiles[key];
        }

        /** Method:  Set a percentile for any value */
        internal void SetPercentile(double perc, double val) {
            percentiles[GetKey(perc)] = val;
        }

        /** Method:  if mini mode should be performed */
        internal bool Mini {
            get { return mini; }
            set { mini = value; }
        }

        /** Method:  if bisection method should be performed (else montecarlo) */
        internal bool Bisection
        {
            get { return bisection; }
            set { bisection = value; }
        }
        
        /** Method:  Min value of the distribution */
        internal double Min {
            get { return histogram.Min; }
        }

        /** Method:  Max value of the distribution */
        internal double Max {
            get { return histogram.Max; }
        }

        /** Method:  Standard deviation for every gaussian function */
        internal double StDev {
            get { return stDev; }
            set { stDev = value; }
        }

        #endregion

        #region internal Methods

        #region Load Methods

        /** Method:  Load data from a list that represents a distribution (indexes = values, values = frequencies) */
        internal void LoadDist(List<double> meanFrec) {
            histogram.LoadDist(meanFrec);
        }

        /** Method:  Load data from another histogram */
        internal void LoadHist(Histogram hist) {
            histogram.LoadHist(hist);
        }

        #endregion

        #region Probability Methods

        /** Method:  Calculate probability for any value */
        internal double Probability(double x) {
            double p = 0.0;
            double totWeights = 0.0;
            double freq;
            foreach(double val in histogram.GetValues()) {
                freq = histogram.GetFreq(val);
                p += freq * ns.pNorm(x, val, stDev);
                totWeights += freq;
            }
            if(totWeights == 0) { return 0.0; }
            return p/totWeights;
        }

        private double Quantile(double p) {
            return rs.SteffensenAccOneRoot(Probability, DerivProbability, Deriv2Probability, Min, Max, p);
        }

        /** Method:  Calculate the percentile for any percentage */
        internal double CalculatePercentile(double p) {
            if(histogram.Count == 1) {
                IEnumerator<int> enKeys = histogram.GetKeysEnumerator();
                enKeys.MoveNext();
                return (double)enKeys.Current;
            }

            double x = -1;
            int it = 1;
            double maxSteffensen = 0;
            //double maxSteffensen = 80;
            if(p >= maxSteffensen) {
                x =  rs.MonotoneBisection(Probability, true, 0.0, maxInt, p/100.0, 0.005, ref it, 120);
            } 
            else {
                try {
                    x = rs.SteffensenAccOneRoot(Probability, DerivProbability, Deriv2Probability, 0.0, maxInt, p/100.0, maxIterations);
                } 
                catch(Exception ex) { 
                    Trace.WriteLine("");
                    x = -1; 
                }

                if(x < 0 || Math.Abs(Probability(x) - p/100.0) > 0.01) {
                    x =  rs.MonotoneBisection(Probability, true, 0.0, maxInt, p/100.0, 0.005, ref it, 30);
                }
            }
            if(x < 0 || Math.Abs(Probability(x) - p/100.0) > 0.01) {
                x = -1;
            }
            //Console.WriteLine(x.ToString("0.00") + "\t" + p + "\t" + Probability(x).ToString("0.00%"));
            return x;
        }

        #endregion

        #region Set Percentiles

        /** Method:  Set percentiles in variables */
        internal int CalcPercentiles(bool allPercentiles) {
            percentiles.Clear();
            if(mini) { 
                SetPercentilesLess(allPercentiles);
                return 200;
            }
            else {
                maxInt = GetMaxInt();
                minInt = GetMinInt();
                if(allPercentiles) {
                    for(int i=800;i<=1000;i++) { percentiles.Add(i, 0); }
                    int its = rs.SetAllValues(percentiles, Probability, 80, 100, minInt, maxInt, bisection);
                    return its;
                }
                else { return 1; }
            }
        }

   
        /** Method:  Set percentiles in case that data quantity is below the threshold */
        private void SetPercentilesLess(bool allPercentiles) {
            percentiles.Clear();
            maxInt = GetMaxInt();
            minInt = GetMinInt();
            double x, xAnt, p;
            if(histogram.Count == 1) {
                IEnumerator<int> enKeys = histogram.GetKeysEnumerator();
                enKeys.MoveNext();
                for(int i=800;i<1000;i++) {
                    p = (double)i/10.0;
                    percentiles.Add(GetKey(p), 0.0);
                }
                return;
            }
            xAnt = minInt;
            percentiles.Add(GetKey(80.0), minInt);
            int it = 0;
            for(int i=801;i<999;i++) {
                p = (double)i/10.0;
                try { x = rs.MonotoneBisection(Probability, DerivProbability, xAnt, maxInt, p/100.0, 0.0005, ref it, 30); } 
                catch { x = xAnt; }
                if(x < xAnt) { x = xAnt; }
                percentiles.Add(GetKey(p), x);
                xAnt = x;
            }
            percentiles.Add(GetKey(99.9), maxInt);
            percentiles.Add(GetKey(100.0), maxInt);

        }

        /** Method:  Set percentiles in case that data quantity is above the threshold */
        private void SetPercentilesMore(bool allPercentiles) {
            percentiles.Clear();
            maxInt = GetMaxInt();
            minInt = GetMinInt();
            try {
                zerosDeriv2 = ZerosDeriv2();
            } catch (Exception ex) {
                throw new InvalidOperationException("Cannot calculate second derivative zeros");
            }

            if(!allPercentiles) { return; }

            double xAnt = minInt;
            double x;
            int a = 0;
            int b = 1;
            double aInt, bInt;
            double p;
            percentiles.Add(GetKey(80.0), minInt);
            for(int i=801;i<999;i++) {
                p = (double)i/10.0;
                aInt = zerosDeriv2[a];
                bInt = zerosDeriv2[b];
                while(p/100.0 > Probability(bInt) && b < zerosDeriv2.Count-1) {
                    a = b;
                    b++;
                    aInt = zerosDeriv2[a];
                    bInt = zerosDeriv2[b];
                }
                try {
                    x = rs.SteffensenAccOneRoot(Probability, DerivProbability, Deriv2Probability, aInt, bInt, p/100.0, maxIterations); 
                }
                catch {
                    //x = rs.MonotoneBisection(Probability, DerivProbability, aInt, bInt, p/100.0, 0.1, 100);
                    x = xAnt;
                }
                aInt = x;
                if(x < xAnt) { x = xAnt; }
                if(x > maxInt) { x = maxInt; }
                percentiles.Add(GetKey(p), x);
                xAnt = x;
            }
            percentiles.Add(GetKey(99.9), maxInt);
            percentiles.Add(GetKey(100.0), maxInt);
        }

        /** Method:  Get minimum value of interval */
        internal double GetMinInt() {
            int it = 0;
            minInt = rs.MonotoneBisection(Probability, true, Min, maxInt, 0.8, 0.01, ref it, 100);
            return minInt;
        }

        /** Method:  Get maximum value of interval */
        internal double GetMaxInt() {
            int it = 0;
            double max = Max + Max * 0.1;
            while(Probability(max) < 0.99 && it < 10) {
                max += max * 0.1;
                it++;
            }
            return max;
        }

        /** Method:  Set maximum interval (intermediate calculation) */
        internal void SetMaxInt() {
            maxInt = GetMaxInt();
        }

        #endregion

        #region Derivatives

        #region Weighted Normal Four Derivatives

        /** Method: DerivProbability
        La derivada primera de la función de probabilidad acumulada en x, es la función de densidad en 
        Porque la función de probabilidad acumulada en x es la integral de la función de densidad hasta x
        f' = Σ d(x)
        x Valor en el cual se desea obtener la derivada de la función de probabilidad acumulada
        value of the derivative for x */
        internal double DerivProbability(double x) {
            double dens = 0.0;
            double freq;
            foreach(double val in histogram.GetValues()) {
                freq = histogram.GetFreq(val);
                dens += freq * ns.pNormDeriv(x, (double)val, stDev);
            }
            return dens;
        }

        /** Method: 
        Derivada segunda de la función de probabilidad acumulada en x.
        f'' = Σ d'(x)  donde d'(x) = 
        x - Valor en el cual se desea obtener la derivada segunda de la función de probabilidad acumulada */
        internal double Deriv2Probability(double x) {
            double dens = 0.0;
            double freq;
            foreach(int val in histogram.GetValues()) {
                freq = histogram.GetFreq(val);
                dens += freq * ns.pNormDeriv2(x, (double)val, stDev);
            }
            return dens;
        }

        /** Method: 
          tercera de la función de probabilidad acumulada en x.</para>
        f''' = Σ d''(x)  donde d'(x) = 
        x -Valor en el cual se desea obtener la derivada tercera de la función de probabilidad acumulada */
        internal double Deriv3Probability(double x) {
            double dens = 0.0;
            double freq;
            foreach(int val in histogram.GetValues()) {
                freq = histogram.GetFreq(val);
                dens += freq * ns.pNormDeriv3(x, (double)val, stDev);
            }
            return dens;
        }

        /** Method: 
        Derivada cuarta de la función de probabilidad acumulada en x.
        f'''' = Σ d'''(x)  donde d'(x) =
        x - Valor en el cual se desea obtener la derivada cuarta de la función de probabilidad acumulada  */
        internal double Deriv4Probability(double x) {
            double dens = 0.0;
            double freq;
            foreach(int val in histogram.GetValues()) {
                freq = histogram.GetFreq(val);
                dens += freq * ns.pNormDeriv4(x, (double)val, stDev);
            }
            return dens;
        }

        #endregion

        #region Zeros in Second Derivative

        #region General

        /** Method:  Obtains a list of zeros in the second derivative of the function */
        internal List<double> ZerosDeriv2() {
            if(mini) { return ZerosDeriv2Less(); } else { return ZerosDeriv2More(); }
        }

        #endregion

        #region Less than 3 Frequencies

        private List<double> ZerosDeriv2Less() {
            List<double> zerosWDeriv2 = new List<double>();
            zerosWDeriv2.Add(minInt);
            List<double> values = histogram.GetValues();
            double val1, val2, valInter;
            val1 = (double)values[0];
            val2 = (double)values[1];

            if(Deriv2Probability(val1) == 0 && val1 > minInt) { zerosWDeriv2.Add(val1); }
            if(val2 == 0) { return zerosWDeriv2; }
            valInter = GetIntermediateZero(histogram.GetFreq(val1), histogram.GetFreq(val2), val1, val2);
            if(Deriv2Probability(valInter) == 0 && val1 > minInt) { zerosWDeriv2.Add(valInter); }
            if(Deriv2Probability(val2) == 0 && val2 > minInt) { zerosWDeriv2.Add(val2); }
            return zerosWDeriv2;
        }

        private double GetIntermediateZero(double w1, double w2, double m1, double m2) {
            return (2 * Math.Log(w1/w2) - Math.Pow(m1, 2) + Math.Pow(m2, 2)) / (2 *(m2 - m1));
        }

        #endregion

        #region More or equal 3 Frequencies

        private List<double> ZerosDeriv2More() {
            zerosDeriv2.Clear();
            zerosDeriv2.Add(minInt);
            double interZero = -1;
            double a, b, d2a, d2b;

            List<double> values = histogram.GetValues();
            for(int i=0;i<values.Count-1;i++) {
                a = values[i];
                b = values[i+1];

                d2a = Deriv2Probability(a);
                d2b = Deriv2Probability(b);

                if(d2a * d2b > -0.01) { a = b; continue; }
                interZero = rs.SteffensenAccOneRoot(Deriv2Probability, Deriv3Probability, Deriv4Probability, a, b, 0.0);
                if(interZero > minInt) { zerosDeriv2.Add(interZero); }
            }
            zerosDeriv2.Add(maxInt);
            return zerosDeriv2;
        }

        #endregion


        #endregion

        #endregion

        #endregion

        #region Private Methods

        private int GetKey(double val) {
            return Convert.ToInt32(Math.Round(val, 1)* 10);
        }

        /** Method:  Conversion key to value  */
        /// <param name="key"> the key </param>
        /// <returns> the corresponding value </returns>
        internal double GetValue(int key) {
            return (double)key / 10.0;
        }

        private void AddPercentile(double perc, double val) {
            percentiles[GetKey(perc)] = val;
        }


        #endregion

    }
}
