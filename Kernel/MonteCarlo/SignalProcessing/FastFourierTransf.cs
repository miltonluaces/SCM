#region Imports

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Statistics;
using System.Threading;
using Maths;

#endregion

namespace MonteCarlo {
    
    /** Class: Fast Fourier Transformed for convolutions */ 
    internal class FastFourierConvolution : IConvolution {

        #region Fields

        private List<double> probs;
        private List<double> acumProbs;
        private FFT fft;
        private Histogram histogram;
        private double n;
        private int maxClasses;
        private int lag;

        #endregion

        #region Constructor

        /** Method:  Constructor  
        maxClasses -  maximum number of classes */
        internal FastFourierConvolution(int maxClasses) {
            this.fft = new FFT();
            this.maxClasses = maxClasses;
        }

        #endregion

        #region Internal Methods

        /** Method:  Load data for calculation  
        data -  data from time series 
        n -  number of convolutions */
        void IConvolution.LoadData(List<double> data, double n) {
            histogram.Clear();
            histogram.LoadData(data);
            this.n = n;
        }

        /** Method:  Load histogram for calculation  
        hist -  histogram for calculation 
        n -  number of convolutions */
        void IConvolution.LoadHistogram(double min, double max, double totFreqs, double range, int maxClasses, SDict<int, double> freqs, double n) {
            Histogram hist = new Histogram();
            hist.LoadHist(min, max, totFreqs, range, maxClasses, freqs);
            this.n = n;
        }

        /** Method:  Acumulated probability for a certain value  */
        double IConvolution.ProbabilityAcum(double x) {
            if (probs == null) { LinearConvolve(histogram, (int)n); }
            int key = (int)Math.Ceiling(((x / histogram.Width - histogram.Min + histogram.Mean)));
            if (key < 0 || key > probs.Count - 1) { return 0; }
            return acumProbs[key];
        }

        /** Method:  Quantile for a certain probability  */
        double IConvolution.Quantile(double p) {
            if (p > 1) { throw new Exception("Error Probabilities must be between 0 and 1"); }
            if (probs == null) { LinearConvolve(histogram, (int)n); }

            for (int i = 0; i < acumProbs.Count; i++) {
                if (acumProbs[i] >= p) {
                    if (i > 0 && Math.Abs(acumProbs[i] - p) > Math.Abs(acumProbs[i + 1] - p)) { return (int)Math.Round(i + 1 - histogram.Mean); } else { return (int)Math.Round(i - histogram.Mean); }
                }
            }
            return -1;
        }

        /** Method:  if calculation is valid  */
        bool IConvolution.IsValid() {
            return true;
        }

        #endregion

        #region Private Methods

        private double ProbabilityFft(int n, double value) {
            if (probs == null) { LinearConvolve(histogram, n); }
            //int key = (int)(value / histogram.Width- histogram.Min) + lag;
            int key = (int)Math.Ceiling(((value / histogram.Width - histogram.Min) + histogram.Mean));
            if (key < 0 || key > probs.Count - 1) { return 0; }
            return probs[key];
        }

        
        private void LinearConvolve(Histogram hist, int n) {
            List<double> completeValues = new List<double>();
            for (int i = 0; i < maxClasses; i++) {
                if (hist.GetValue(i) > hist.Max) { break; }
                if (hist.Freqs.ContainsKey(i)) { completeValues.Add(hist.Freqs[i]); } else { completeValues.Add(0.0); }
            }
            PadRight(completeValues);

            ComplexNum[] comp = new ComplexNum[completeValues.Count];
            for (int i = 0; i < completeValues.Count; i++) { comp[i] = new ComplexNum(completeValues[i], 0); }
            ComplexNum[] complexRes = fft.LinearConvolve(comp, n);
            List<double> realRes = new List<double>();
            double tot = 0;
            double val;
            for (int i = 0; i < complexRes.Length; i++) {
                val = complexRes[i].Real;
                if (val > 0) {
                    realRes.Add(val);
                    tot += val;
                } else {
                    realRes.Add(0);
                }

            }
            probs = new List<double>();
            acumProbs = new List<double>();
            double prob;
            double acum = 0;
            for (int i = 0; i < complexRes.Length; i++) {
                prob = realRes[i] / tot;
                probs.Add(realRes[i] / tot);
                acum += prob;
                acumProbs.Add(acum);
            }
            lag = (int)Math.Round(hist.Mean);
        }

        private void PadRight(List<double> values) {
            int count = values.Count;
            int quadPow = 2;
            while (count > quadPow) { quadPow *= 2; }
            if (quadPow == count) { return; }
            int addCount = quadPow - count;
            for (int i = 0; i < addCount; i++) { values.Add(0); }
        }

        #endregion
    }
}
