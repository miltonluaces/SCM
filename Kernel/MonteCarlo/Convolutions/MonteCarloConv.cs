#region Imports

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Statistics;
using Maths;


#endregion

namespace MonteCarlo {

    /** Class:  Class for convolution calculation based on montecarlo aproximation  */
    internal class MontecarloConv : IConvolution {

        #region Fields

        private RndGenerator rg;
        private double n;
        private List<double> convData;
        Histogram hist;
        private KernelDensity de;
        private int maxIt;

        #endregion

        #region Constructor

        /** Method: Constructor  */
        internal MontecarloConv(int maxIt) {
            rg = new RndGenerator();
            convData = new List<double>();
            hist = new Histogram(100);
            de = new KernelDensity(1, 100, 100);
            this.maxIt = 2000; //TODO: eliminate parameter	
        }

        #endregion

        #region ConvolutionCalc interface implementation

        /** Method: Load data for calculation  
        data -  data from time series 
        n -  number (real) of convolutions */
        void IConvolution.LoadData(List<double> data, double n) {
            if (data.Count == 0) {
                 throw new Exception("Error. No data");
             }
            double res = n - (int)n;
            this.n = n;
            convData.Clear();
            hist.Clear();
            de = new KernelDensity(1, 100, 100);
            int []indexes = new int[(int)n+1];
            int indexRes = -1;
            double sum;
            for (int i = 0; i < maxIt/2; i++) {
                sum = 0;
                for (int j = 1; j <= n; j++) {
                    indexes[j-1] = rg.NextInt(0, data.Count-1);
                    sum += data[indexes[j-1]];
                }
                if (res > 0) {
                    indexRes = rg.NextInt(0, data.Count - 1);
                    sum = sum + data[indexRes] * res;
                }
                convData.Add(sum);

                sum = 0;
                for (int j = 1; j <= n; j++) {
                    sum += data[data.Count-1-indexes[j-1]];
                }
                if (res > 0) {
                    sum = sum + data[data.Count-1-indexRes] * res;
                }
                convData.Add(sum);
            }
            hist.LoadData(convData);
            de.LoadHist(hist);
            de.SetMaxInt();
        }

        /** Method:  Load histogram for calculate  
        hist -  histogram for calculation 
        n -  number of convolutions */
        void IConvolution.LoadHistogram(double min, double max, double totFreqs, double range, int maxClasses, SDict<int, double> freqs, double n)  {
            Histogram hist = new Histogram();
            hist.LoadHist(min, max, totFreqs, range, maxClasses, freqs);
            ((IConvolution)this).LoadData(hist.GetRawData(), n);
        }
        
        /** Method:  Acumulated probability for a certain value  x */
        double IConvolution.ProbabilityAcum(double x) {
            return de.Probability(x);
        }

        /** Method:  Quantile for a certain probability  p */
        double IConvolution.Quantile(double p) {
            if (p < 0 || p > 1) { throw new Exception("Error Probability must be between 0 and 1"); }
            return de.CalculatePercentile(p * 100.0);
        }

        /** Method:  if calculation is valid  */
        bool IConvolution.IsValid() {
            return true;
        }

        #endregion

    }
}
