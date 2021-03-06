#region Imports

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Statistics;
using Maths;

#endregion


namespace MonteCarlo {

    /** Method:  Class for convolution calculation with combinatory */
    internal class CombinatoryConv : IConvolution
    {

        #region Fields

        private Combinatory comb;
        private int maxClasses = -1;
        private Histogram originalHistogram;
        private Histogram histogram;
        private double mean;
        private double stDev;
        private double n;
        private bool acum;
        private double maxError;
        private delegate double function(int n, double value, bool acum);

        #endregion

        #region Constructor

        internal CombinatoryConv(int maxClasses, double maxError)
        {
            this.maxClasses = maxClasses;
            this.comb = new Combinatory();
            this.acum = true;
            this.maxError = maxError;
        }

        #endregion

        #region Internal Methods

        /** Method:  Load data for calculation 
        data - data from time series
        n -  number (real) of convolutions */
        void IConvolution.LoadData(List<double> data, double n)   {
            Histogram hist = new Histogram(maxClasses);
            hist.LoadData(data);
            this.n = n;
            ((IConvolution)this).LoadHistogram(hist.Min, hist.Max, hist.TotFreqs, hist.Range, hist.MaxClasses, hist.Freqs, n);
        }

        /** Method:  Load a Histogram for calculation with restriction of classes 
        n -  number (real) of convolutions */
        void IConvolution.LoadHistogram(double min, double max, double totFreqs, double range, int maxClasses, SDict<int, double> freqs, double n)  {

            //set properties
            Histogram histogram = new Histogram();
            histogram.LoadHist(min, max, totFreqs, range, maxClasses, freqs);
            this.originalHistogram = histogram;
            this.mean = originalHistogram.Mean;
            this.stDev = originalHistogram.StDev;
            this.n = n;

            //scale classes
            if (maxClasses > 0 && maxClasses < histogram.Freqs.Count)
            {
                this.histogram = new Histogram(maxClasses);
                this.histogram.LoadHist(originalHistogram);
            }
            else
            {
                this.histogram = this.originalHistogram;
            }
        }

        /** Method:  Acumulated probability for a certain value */
        double IConvolution.ProbabilityAcum(double x)   {
            if (histogram.Count == 1)
            {
                double uniqueVal = histogram.GetValues()[0];
                if (acum)
                {
                    if (x < uniqueVal) { return 0; } else { return 1; }
                }
                else
                {
                    if (x == uniqueVal) { return 1; } else { return 0; }
                }
            }

            if (x >= histogram.Max * n) { return 1.0; }
            if (x <= histogram.Min * n) { return 0.0; }
            int nInt = (int)n;

            double perm = comb.Permutations(nInt);
            if (acum) { return ProbabilityAcum(histogram, perm, nInt, x); }
            else { return Probability(histogram, perm, nInt, x); }


        }

        /** Method:  Quantile for a certain probability */
        double IConvolution.Quantile(double p)   {
            double quantile = -1;
            int nInt = (int)n;
            double min = histogram.Min * nInt;
            if (min < 0) { min = 0; }
            quantile = MonotoneBisectionRec(Probability, min, histogram.Max * nInt, p, true, 0.01, 1, 100, histogram, nInt);
            //if (p == 0.5) {
            //    quantile =  histogram.Mean * n;
            //}
            //else {
            //    if (p < 0.5) { quantile = MonotoneBisectionRec(Probability, histogram.Min * nInt, histogram.Mean * nInt, p, true, 0.01, 1, 100, histogram, nInt); } 
            //    else if (p > 0.5) { quantile = MonotoneBisectionRec(Probability, histogram.Mean * nInt, histogram.Max * nInt, p, true, 0.01, 1, 100, histogram, nInt); }
            //}
            return Math.Round(quantile);
        }

        internal double Probability(int n, double x, bool acum)    {
            return ((IConvolution)this).ProbabilityAcum(x);
        }

        /** Method:  if calculation is valid */
        bool IConvolution.IsValid()  {
            double quantMean = ((IConvolution)this).Quantile(0.5);
            return (Math.Abs(quantMean - mean * n) / (mean * n) <= maxError);
        }

        #endregion

        #region Private Methods

        private double Probability(Histogram hist, double perm, int n, double value)
        {
            List<int> keys = hist.GetKeys();
            int key = hist.GetConvKey(value, n);
            List<int[]> sumsVars = comb.SumsVariations(keys, n, key, false);

            double convProb = 0;
            double prob;
            foreach (int[] sumsVar in sumsVars)
            {

                prob = ProbabilityOneComb(hist, perm, sumsVar);
                convProb += prob;
            }
            return convProb;
        }

        private double ProbabilityAcum(Histogram hist, double perm, int n, double value)
        {
            List<int> keys = hist.GetKeys();
            int key = hist.GetConvKey(value, n);

            Dictionary<int, List<int[]>> sumsVarsDict = comb.SumsVariationsUntil(keys, n, key, false);

            double convProb = 0;
            double prob;
            foreach (List<int[]> sumsVars in sumsVarsDict.Values)
            {
                prob = ProbabilityManyCombs(hist, perm, sumsVars);
                convProb += prob;
            }
            return convProb;
        }

        private double ProbabilityOneComb(Histogram hist, double perm, int[] sumsVar)
        {
            double prob = 1;
            int rep = 1;
            double permRep = 1;
            for (int i = 0; i < sumsVar.Length; i++)
            {
                if (i > 0 && sumsVar[i] == sumsVar[i - 1]) { rep++; }
                if (rep > 1 && (i == sumsVar.Length - 1 || sumsVar[i] != sumsVar[i - 1])) { permRep *= comb.Permutations(rep); rep = 1; }
                prob *= hist.ProbabilityByKey(sumsVar[i]);
            }
            return prob * (perm / permRep);
        }

        private double ProbabilityManyCombs(Histogram hist, double perm, List<int[]> sumsVars)
        {
            double oneProb = 0;
            double manyProbs = 0;
            foreach (int[] sumsVar in sumsVars)
            {
                oneProb = ProbabilityOneComb(hist, perm, sumsVar);
                manyProbs += oneProb;
            }
            return manyProbs;
        }

        private double QuantileSec(double min, double max, double acumMin, int n, double p)
        {
            double acum = acumMin;
            double q = 0;
            double acumAnt = double.MinValue;
            for (int i = (int)min + 1; i <= (int)max; i++)
            {
                q = i;
                acumAnt = acum;
                acum += Probability(n, q, false);
                if (acum > p) { break; }
            }
            if (Math.Abs(acumAnt - p) < Math.Abs(acum - p)) { return q - 1; } else { return q; }
        }

        //TODO: Refactoring (this is almost a clone of MonotoneBisectionRec of NumCalc (cannot use the other due to circular reference)
        private double MonotoneBisectionRec(function f, double min, double max, double val, bool ascendant, double eps, int it, int maxIt, Histogram hist, int n)
        {
            it++;
            double inter = min + (max - min) / 2.0;
            if (it > maxIt || inter == min || inter == max)
            {
                return inter;
            }
            double fInter = f(n, inter, true);
            double diff = val - fInter;
            if (Math.Abs(diff) < eps) { return inter; }
            if (diff > 0 && diff < 0.05) { return QuantileSec(inter, max, fInter, n, val); }
            if (ascendant)
            {
                if (val < fInter) { return MonotoneBisectionRec(f, min, inter, val, ascendant, eps, it, maxIt, hist, n); }
                if (val > fInter) { return MonotoneBisectionRec(f, inter, max, val, ascendant, eps, it, maxIt, hist, n); }
            }
            else
            {
                if (val > fInter) { return MonotoneBisectionRec(f, min, inter, val, ascendant, eps, it, maxIt, hist, n); }
                if (val < fInter) { return MonotoneBisectionRec(f, inter, max, val, ascendant, eps, it, maxIt, hist, n); }
            }
            return inter;
        }


        #endregion

    }
}
