#region Imports

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Statistics;
using Maths;

#endregion

namespace MonteCarlo {
    
    /** Class:  Class for multiple convolution calculations  */
    internal class ConvolutionCalculator {

        #region Fields

        private IConvolution convCalc;

        private double lambdaInc;
        private int maxClasses;
        private int maxIt;
        private int maxComb;
        private int minNorm;
        private double minMaxLik;
        private double maxCombError;
        private List<double> data;
        private MethodType method;
        private Histogram hist;
        private double n;
        
        #endregion

        #region Constructors

        /** Method: Constructor */
        internal ConvolutionCalculator() {
            this.lambdaInc = 0.5;
            this.maxClasses = 100;
            this.maxIt = 100;
            this.maxComb = 4;
            this.minNorm = 15;
            this.minMaxLik = 0.05;
            this.maxCombError = 0.1;
            this.method = MethodType.None;
            this.hist = new Histogram();
            this.n = 1;
        }

        /** Method: Constuctor with parameters  
        lambdaInc -  increments in lambda value for Normalized calculation 
        maxClasses -  maximum of classes for combinatory calculation 
        maxIt -  maximum of iterations for montecarlo calculation 
        maxComb -  maximum convolutions for combinatory calculation 
        minNorm -  minimum convolutions for normal calculation 
        minMaxLik -  minimum of maximum likelihood value for validate normalizing calculation 
        maxCombError -  maximum relative error for combinatory method application */
        internal ConvolutionCalculator(double lambdaInc, int maxClasses, int maxIt, int maxComb, int minNorm, double minMaxLik, double maxCombError) {
            this.lambdaInc = lambdaInc;
            this.maxClasses = maxClasses;
            this.maxIt = maxIt;
            this.maxComb = maxComb;
            this.minNorm = minNorm;
            this.minMaxLik = minMaxLik;
            this.maxCombError = maxCombError;
            this.method = MethodType.None;
        }

        #endregion

        #region Properties

        /** Histogram for calculation  */
        internal Histogram Histogram {
            get { return hist; }
        }

        /** Method:  maximum of classes for combinatory calculation  */
        internal int MaxClasses {
            get { return maxClasses; } 
        }

        /** Method:  maximum of iterations for montecarlo calculation  */
        internal int MaxIt {
            get { return maxIt;  }
            set { maxIt = value; }
        }

        /** Method:  maximum convolutions for combinatory calculation  */
        internal int MaxComb {
            get { return maxComb;  }
        }

        /** Method:  minimum convolutions for normal calculation  */
        internal int MinNorm {
            get { return minNorm;  }
        }

        /** Method:  maximum relative error for combinatory calculation  */
        internal double MaxCombError {
            get { return maxCombError; }
        }
        
        /** Method:  data for calculation  */
        internal List<double> Data {
            get { return data; }
        }

        /** Method:  method used for calculation  */
        internal MethodType Method {
            get { return method; }
            set { method = value; }
        }

        #endregion

        #region Internal Methods

        /** Method:  Load data for calculation 
        data -  data from time series 
        n -  number of convolutions  
        method can be set before loading data to force a particular calculation method */
        internal void LoadData(List<double> data, double n) {

            this.data = data;
            this.n = n;            
            this.method = SelectMethod(n); 
    
            switch (method) { 
                case MethodType.Combinatory: convCalc = new CombinatoryConv(maxClasses, maxCombError); break;
                case MethodType.Montecarlo:  convCalc = new MontecarloConv(maxIt); break;
                case MethodType.Normal:      convCalc = new NormalConv(); break;
            }

            if (!convCalc.IsValid()) {
                convCalc = new MontecarloConv(maxIt);
                method = MethodType.Montecarlo;
                convCalc.LoadData(data, n);
            }
        }

        /** Method:  Load histogram for calculation 
        hist -  the histogram 
        n -  number of convolutions */
        internal void LoadHistogram(Histogram hist, double n) {
            this.hist = hist;
            this.n = n;
            if (data == null || data.Count == 0) {  data = hist.GetRawData(); }

            if (this.method == MethodType.None) { this.method = SelectMethod(n); }
            SelectMethod(method, n);
        }

        /** Select the proper method for a certain number n of convolutions */
        internal void SelectMethod(MethodType method, double n) {
            int montecarloIts = (int)(maxIt / (double)n);
         
            switch (method) {
                case MethodType.Combinatory:
                    convCalc = new CombinatoryConv(maxClasses, maxCombError);
                    convCalc.LoadHistogram(hist.Min, hist.Max, hist.TotFreqs, hist.Range, hist.MaxClasses, hist.Freqs, n);
                    break;
                case MethodType.Montecarlo:
                    convCalc = new MontecarloConv(montecarloIts);
                    convCalc.LoadHistogram(hist.Min, hist.Max, hist.TotFreqs, hist.Range, hist.MaxClasses, hist.Freqs, n);
                    break;
                case MethodType.Normal:
                    convCalc = new NormalConv();
                    convCalc.LoadHistogram(hist.Min, hist.Max, hist.TotFreqs, hist.Range, hist.MaxClasses, hist.Freqs, n);
                    break;
            }
        }
        
        /** Method:  Acumulated probability for a certain value  x */
        internal double ProbabilityAcum(double x) {
            if (data == null || data.Count == 0) { return -1; }
            return convCalc.ProbabilityAcum(x);
        }

        /** Method:  Quantile for a certain probability p */
        internal double Quantile(double p) {
            if (data == null || data.Count == 0) { return -1; }
            if (p == 0.5) { return hist.Mean * n; }
            return convCalc.Quantile(p);
        }

        /** Method:  if calculation is valid  */
        internal bool IsIsValid() {
            return true;
        }

        
        #endregion

        #region Private Methods

        internal MethodType SelectMethod(double n) {

            double nCombs = Math.Pow(hist.NonZeroFreqs,n);

            MethodType method;

            //short : combinatory or montecarlo
            if (nCombs <= maxIt) {
                method = MethodType.Combinatory;
            }

            //long: normal approximation
            else {
                method = MethodType.Normal;
            }
            SelectMethod(method, n);
            return method;
        }
        
        #endregion

        #region Enums

        /** Method:  Type of method used for calculation  */
        internal enum MethodType {
            None, Combinatory, Montecarlo, Normal, FastFourier  };

        #endregion

    }
}
