#region Imports

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Maths;
using Statistics;

#endregion

namespace MonteCarlo {

    /** Method:  Class for convolution calculations  */
    internal class Convolution : IConvolution {

        #region Fields

        private Combinatory comb;
        private Histogram originalHistogram;
        private Histogram histogram;
        private StatFunctions stat;
        private NormalDistrib normal;
        private FFT fft;

        private int maxClasses;
        private int maxCombTerms;
        private int maxFftTerms;
        private double mean;
        private double stDev;
        private double n;

        private List<double> probs;
        private List<double> acumProbs;
        private int lag;
        
        private int nThreads;

        private delegate double function(int n, double value, bool acum);

        #endregion

        #region Constructor

        /** Method:  Constructor  
        maxClasses -  maximum of classes allowed for histogram 
        maxCombTerms -  maximum of convolution terms for combinatory calculation 
        maxFftTerms -  maximum of convolution terms for fast fourier calculation 
        nThreads -  number of threads */
        internal Convolution(int maxClasses, int maxCombTerms, int maxFftTerms, int nThreads) {
            this.comb = new Combinatory();
            stat = new StatFunctions();
            normal = new NormalDistrib();
            fft = new FFT();
             
            this.maxClasses = maxClasses;
            this.maxCombTerms = maxCombTerms;
            this.maxFftTerms = maxFftTerms;
            this.nThreads = nThreads;
        }

        #endregion

        #region Properties 

        /** Method:  Maximum number of classes allowed  */
        internal int MaxClasses {
            get { return maxClasses; }
            set { maxClasses = value; }
        }

        /** Method:  Maximum number of convolution terms allowed for combinatory  */
        internal int MaxCombTerms {
            get { return maxCombTerms; }
            set { maxCombTerms = value; }
        }

        /** Method:  Maximum number of convolution terms allowed for fast fourier transformation  */
        internal int MaxFftTerms {
            get { return maxFftTerms; }
            set { maxFftTerms = value; }
        }
        
        /** Method:  Histogram for convolution calculation  */
        internal Histogram Histogram {
            get { return histogram; }
        }

        #endregion

        #region Internal Methods

        /** Method:  Load Data interface implementation (needs to set maxClasses first if needed)  */
        void IConvolution.LoadData(List<double> data, double n) {
            Histogram hist = new Histogram(maxClasses);
            hist.LoadData(data);
            this.n = n;
            LoadHistogram(hist, maxClasses);
        }

        /** Method:  Load histogram for calculation  
        hist -  histogram for calculation 
        n -  number of convolutions */
        void IConvolution.LoadHistogram(double min, double max, double totFreqs, double range, int maxClasses, SDict<int,double> freqs, double n) {
            Histogram hist = new Histogram();
            hist.LoadHist(min, max, totFreqs, range, maxClasses, freqs);
            ((IConvolution)this).LoadData(hist.GetRawData(), n);
        }
        
        /** Method:  Load a Histogram for calculation with restriction of classes  
        maxClasses -  maximum number of classes allowed (-1 for no restriction) */
        internal void LoadHistogram(Histogram histogram, int maxClasses) {
            this.maxClasses = maxClasses;

            //set properties
            this.originalHistogram = histogram;
            this.mean = originalHistogram.Mean;
            this.stDev = originalHistogram.StDev;
            this.maxClasses = maxClasses;
       
            //scale classes
            if(maxClasses > 0 && maxClasses < histogram.Freqs.Count) {
                this.histogram = new Histogram(maxClasses);
                this.histogram.LoadHist(originalHistogram);
            }
            else {
                this.histogram = this.originalHistogram;
            }
        }


        internal double ProbabilityInt(int n, double value, bool acum) {
            return Probability(n, value, acum);
        }

        /** Method:  ProbabilityComb  
        value -  value for probability calculation 
        n -  number of summands 
        acum -  accumulated probability */
        internal double Probability(double n, double value, bool acum) {
            if(histogram.Count == 1) {
                double uniqueVal = histogram.GetValues()[0];
                if(acum) {
                    if(value < uniqueVal) { return 0; }
                    else { return 1; }
                }
                else {
                    if(value == uniqueVal) { return 1; }
                    else { return 0; }
                }
            }

            if(value >= histogram.Max * n) { return 1.0; }
            if(value <= histogram.Min * n) { return 0.0; }
            int nInt = (int)n;
                        
            //short
            if(n <= maxCombTerms) {
                double perm = comb.Permutations(nInt);
                if(nThreads > 1) { return ProbabilityCombAcumMultithreading(histogram, perm, nInt, value); }
                else {
                    if (acum) { return ProbabilityCombAcum(histogram, perm, nInt, value); } 
                    else { return ProbabilityComb(histogram, perm, nInt, value); }
                }
            }

            //intermediate
            else if(n <= maxFftTerms) {
                if (acum) { return ProbabilityFftAcum(nInt, value); } 
                else { return ProbabilityFft(nInt, value); }
            }
            
            //large
            else {
                if(acum) { return ProbabilityNormalAcum(n, value); }
                else { return 0; }
            }
        }

        /** Method:  Quantile  
        n -  number of summands of the convolution 
        p -  probability */
        internal double Quantile(double n, double p) {
            double quantile = -1;
            int nInt = (int)n;
            
            if (p == 0.5) {
                quantile =  histogram.Mean * n;
            }
            else {
                //short
                if (nInt <= maxCombTerms) {
                    if (p < 0.5) { quantile = MonotoneBisectionRec(ProbabilityInt, histogram.Min * nInt, histogram.Mean * nInt, p, true, 0.01, 1, 100, histogram, nInt); } 
                    else if (p > 0.5) { quantile = MonotoneBisectionRec(ProbabilityInt, histogram.Mean * nInt, histogram.Max * nInt, p, true, 0.01, 1, 100, histogram, nInt); }
                }
                //intermediate
                else if (n <= maxFftTerms) { quantile = QuantileFft(nInt, p); }
                //large
                else { quantile = QuantileNormal(n, p/100); }
            }
            return Math.Round(quantile);
        }

        double IConvolution.ProbabilityAcum(double x) {
            return Probability(this.n, x, true);
        }

        double IConvolution.Quantile(double p) {
            return Quantile(this.n, p);
        }


        /** Method:  if calculation is valid  */
        bool IConvolution.IsValid() {
            return true;
        }
        
        #endregion

        #region Private Methods

        #region Calculation methods

        #region Combinatory (short)

        private double ProbabilityComb(Histogram hist, double perm, int n, double value) {
            List<int> keys = hist.GetKeys();
            int key = hist.GetKey(value);
            List<int[]> sumsVars = comb.SumsVariations(keys, n, key, false);

            double convProb = 0;
            double prob;
            foreach(int[] sumsVar in sumsVars) {
                prob = ProbabilityOneComb(hist, perm, sumsVar);
                convProb += prob;
            }
            return convProb;
        }

        private double ProbabilityCombAcum(Histogram hist, double perm, int n, double value) {
            List<int> keys = hist.GetKeys();
            int key = hist.GetKey(value);
            
            Dictionary<int, List<int[]>> sumsVarsDict = comb.SumsVariationsUntil(keys, n, key, false);

            double convProb = 0;
            double prob;
            foreach(List<int[]> sumsVars in sumsVarsDict.Values) {
                prob = ProbabilityManyCombs(hist, perm, sumsVars);
                convProb += prob;
            }
            return convProb;
        }
        
        private double ProbabilityOneComb(Histogram hist, double perm, int[] sumsVar) {
            double prob = 1;
            int rep = 1;
            double permRep = 1;
            for(int i=0;i<sumsVar.Length;i++) {
                if(i>0 && sumsVar[i] == sumsVar[i-1]) { rep++; }
                if(rep > 1 && (i == sumsVar.Length-1 || sumsVar[i] != sumsVar[i-1])) { permRep *= comb.Permutations(rep); rep = 1; }
                prob *= hist.ProbabilityByKey(sumsVar[i]);
            }
            return prob * (perm/permRep);
        }

        private double ProbabilityManyCombs(Histogram hist, double perm, List<int[]> sumsVars) {
            double oneProb = 0;
            double manyProbs = 0;
            foreach(int[] sumsVar in sumsVars) {
                oneProb = ProbabilityOneComb(hist, perm, sumsVar);
                manyProbs += oneProb;
            }
            return manyProbs;
        }
        
        #endregion

        #region Fast Fourier (intermediate)

        private double ProbabilityFft(int n, double value) {
            if(probs == null) { LinearConvolve(histogram, n); }
            //int key = (int)(value / histogram.Width- histogram.Min) + lag;
            int key = (int)Math.Ceiling(((value/ histogram.Width- histogram.Min) + histogram.Mean));
            if(key < 0 || key > probs.Count - 1) { return 0; }
            return probs[key];
        }

        private double ProbabilityFftAcum(int n, double value) {
            if(probs == null) { LinearConvolve(histogram, n); }
            //int key = (int)(value / histogram.Width - histogram.Min) + lag;
            int key = (int)Math.Ceiling(((value / histogram.Width - histogram.Min + histogram.Mean)));
            if(key < 0 || key > probs.Count - 1) { return 0; }
            return acumProbs[key];
        }

        private double QuantileFft(int n, double p) {
            if(p > 1) { throw new Exception ("Error Probabilities must be between 0 and 1"); }
            if(probs == null) { LinearConvolve(histogram, n); }

            for(int i=0;i<acumProbs.Count;i++) {
                if(acumProbs[i] >= p) {
                    if(i > 0 && Math.Abs(acumProbs[i] - p) >  Math.Abs(acumProbs[i+1] - p)) { return (int)Math.Round(i+1 - histogram.Mean); }
                    else { return (int)Math.Round(i - histogram.Mean); }
                }
            }
            return -1;
        }
        
        private void LinearConvolve(Histogram hist, int n) {
            List<double> completeValues = new List<double>();
            for(int i=0;i<maxClasses;i++) {
                if(hist.GetValue(i) > hist.Max) { break; }
                if(hist.Freqs.ContainsKey(i)) { completeValues.Add(hist.Freqs[i]); } 
                else { completeValues.Add(0.0); }
            }
            PadRight(completeValues);

            ComplexNum[] comp = new ComplexNum[completeValues.Count];
            for(int i=0;i<completeValues.Count;i++) { comp[i] = new ComplexNum(completeValues[i], 0); }
            ComplexNum[] complexRes = fft.LinearConvolve(comp, n);
            List<double> realRes = new List<double>();
            double tot = 0;
            double val;
            for(int i=0;i<complexRes.Length;i++) {
                val = complexRes[i].Real;
                if(val > 0) { 
                    realRes.Add(val);
                    tot += val;
                }
                else {
                    realRes.Add(0); 
                }

            }
            probs = new List<double>();
            acumProbs = new List<double>();
            double prob;
            double acum = 0;
            for(int i=0;i<complexRes.Length;i++) { 
                prob = realRes[i]/tot;
                probs.Add(realRes[i]/tot);
                acum += prob;
                acumProbs.Add(acum);
            }
            lag = (int)Math.Round(hist.Mean);
        }

        private void CalculateAcum() {
            acumProbs = new List<double>();
            double acum = 0;
            for(int i=0;i<probs.Count;i++) {
                acum += probs[i];
                acumProbs[i] = acum;
            }
        }

        #endregion

        #region Normal (long)

        private double ProbabilityNormalAcum(double n, double value) {
            double convMean = n * mean;
            double convStDev = Math.Sqrt(n) * stDev;
            if(convStDev == 0) {
                if(value < convMean) { return 0; }
                else { return 1; }
            }
            return normal.pNorm(value, convMean, convStDev);
        }

        private double QuantileNormal(double n, double p) {
            double convMean = n * mean;
            double convStDev = Math.Sqrt(n) * stDev;
            if(convStDev == 0) { return convMean; }
            if(p < 0 || p > 1) { throw new Exception("Probability p must be between 0 and 1"); }
            return normal.qNorm(p, convMean, convStDev);
        }

        #endregion

        #endregion

        #region Auxiliar Methods

        private double QuantileSec(double min, double max, double acumMin, int n, double p) {
            double acum = acumMin;
            double q = 0;
            double acumAnt = double.MinValue;
            for(int i = (int)min+1;i<= (int)max;i++) {
                q = i;
                acumAnt = acum;
                acum += Probability(n, q, false);
                if(acum > p) { break; }
            }
            if(Math.Abs(acumAnt - p) < Math.Abs(acum - p)) { return q - 1; }
            else { return q; }
        }

        //TODO: Refactoring (this is almost a clone of MonotoneBisectionRec of NumCalc (cannot use the other due to circular reference)
        private double MonotoneBisectionRec(function f, double min, double max, double val, bool ascendant, double eps, int it, int maxIt, Histogram hist, int n) {
            it++;
            double inter = min + (max - min) / 2.0;
            if(it > maxIt || inter == min || inter == max) {
                return inter;
            }
            double fInter = f(n, inter, true);
            double diff = val - fInter;
            if(Math.Abs(diff) < eps) { return inter; }
            if(diff > 0 && diff < 0.05) { return QuantileSec(inter, max, fInter, n, val); }
            if(ascendant) {
                if(val < fInter) { return MonotoneBisectionRec(f, min, inter, val, ascendant, eps, it, maxIt, hist, n); }
                if(val > fInter) { return MonotoneBisectionRec(f, inter, max, val, ascendant, eps, it, maxIt, hist, n); }
            }
            else {
                if(val > fInter) { return MonotoneBisectionRec(f, min, inter, val, ascendant, eps, it, maxIt, hist, n); }
                if(val < fInter) { return MonotoneBisectionRec(f, inter, max, val, ascendant, eps, it, maxIt, hist, n); }
            }
            return inter;
        }

        private void PadRight(List<double> values) {
            int count = values.Count;
            int quadPow = 2;
            while(count > quadPow) { quadPow *= 2; }
            if(quadPow == count) { return; }
            int addCount = quadPow - count;
            for(int i=0;i<addCount;i++) { values.Add(0); }
        }

        #endregion

        #region Obsolete Methods

        [Obsolete]
        private double ProbabilityCombinations(Histogram hist, int n, double value) {
            double perm = comb.Permutations(n);

            List<int> keys = hist.GetKeys();
            List<int[]> sumsVars = comb.SumsCombinations(keys, n, (int)value, false);
            double convProb = 0;
            double prob;
            foreach(int[] sumsVar in sumsVars) {
                prob = 1;
                for(int i=0;i<sumsVar.Length;i++) {
                    prob *= hist.ProbabilityByKey(sumsVar[i]);
                }
                convProb += prob;
            }
            return convProb;
        }

        [Obsolete]
        private double SimpleConvolutionProbability(Histogram hist, double value) {
            int z = (int)value;
            List<int> keys = hist.GetKeys();
            double minValue = keys[0];
            double maxValue = keys[keys.Count-1];
            double convProb = 0;
            double Px, Py;
            for(int k=0;k<keys.Count;k++) {
                Px = hist.ProbabilityByKey(keys[k]);
                Py = hist.ProbabilityByKey(z - keys[k]);
                convProb += Px * Py;
            }
            return convProb;
        }

        [Obsolete]
        private double ProbabilityCombValues(Histogram hist, List<int> values, double perm, int n, double value) {
            List<int[]> sumsVars = comb.SumsVariations(values, n, (int)value, false);

            double convProb = 0;
            double prob;
            foreach(int[] sumsVar in sumsVars) {
                prob = ProbabilityOneComb(hist, perm, sumsVar);
                convProb += prob;
            }
            return convProb;
        }

        [Obsolete]
        private double ProbabilityCombAcumValues(Histogram hist, List<int> values, double perm, int n, double value) {
            Dictionary<int, List<int[]>> sumsVarsDict = comb.SumsVariationsUntil(values, n, (int)value, false);

            double convProb = 0;
            double prob;
            foreach(List<int[]> sumsVars in sumsVarsDict.Values) {
                prob = ProbabilityManyCombs(hist, perm, sumsVars);
                convProb += prob;
            }
            return convProb;
        }
        
        #endregion

        #endregion

        #region Multithreading

        #region Main method

        private double ProbabilityCombAcumMultithreading(Histogram hist, double perm, int n, double value) {
            List<int> keys = hist.GetKeys();
            int key = hist.GetKey(value);
      
            Dictionary<int, List<int[]>> sumsVarsDict = comb.SumsVariationsUntil(keys, n, key, false);

            Thread thread;
            MultiCalculator mc;
            List<Thread> threads = new List<Thread>();
            List<MultiCalculator> mcs = new List<MultiCalculator>();
            foreach(List<int[]> sumsVars in sumsVarsDict.Values) {
                mc = new MultiCalculator(this, sumsVars, hist, perm);
                mcs.Add(mc);
                thread = new Thread(new ThreadStart(mc.Calculate), nThreads);
                threads.Add(thread);
                thread.Start();
            }
            foreach(Thread th in threads) { th.Join(); }

            double convProb = 0;
            foreach(MultiCalculator mC in mcs) { convProb += mC.Result; }
            return convProb;
        }

        #endregion

        #region Class MultiCalculator

        /** Method:  Inner class for multithreading calculation  */
        internal class MultiCalculator {

            #region Fields

            private Convolution conv;
            private double result;
            private List<int[]> sumsVars;
            private Histogram hist;
            private double perm;

            #endregion

            #region Constructor

            /** Method:  Constructor  
            conv -  this class, convolution 
            sumsVars -  sums of variations 
            hist -  histogram of the distribution 
            perm -  calculated permutation */
            internal MultiCalculator(Convolution conv, List<int[]> sumsVars, Histogram hist, double perm) {
                this.conv = conv;
                this.sumsVars = sumsVars;
                this.hist = hist;
                this.perm = perm;
            }

            #endregion

            #region Properties

            /** Method:  Result of calculation  */
            internal double Result {
                get { return result; }
            }

            #endregion

            #region internal Method

            /** Method:  Main calculation method  */
            internal void Calculate() {
                result = conv.ProbabilityManyCombs(hist, perm, sumsVars);
            }

            #endregion

            #region Private Methods

            #endregion

        }

        #endregion

        #endregion
    }
}
