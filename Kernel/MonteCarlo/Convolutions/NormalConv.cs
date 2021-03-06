#region Imports

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Statistics;
using Maths;

#endregion

namespace MonteCarlo
{

    /** Method:  Class for normal aproximation of convolutions */
    internal class NormalConv : IConvolution
    {

        #region Fields

        private List<double> data;
        private double nConv;
        private double convMean;
        private double convStDev;
        private StatFunctions stat;
        private NormalDistrib nd;

        #endregion

        #region Constructor

        /** Method:  Constructor */
        internal NormalConv()
        {
            data = new List<double>();
            stat = new StatFunctions();
            nd = new NormalDistrib();
        }

        #endregion

        #region Properties

        /** Method:  Convolution mean */
        internal double ConvMean
        {
            get { return convMean; }
        }

        /** Method:  Convolution standard deviation */
        internal double ConvStDev
        {
            get { return convStDev; }
        }

        #endregion


        #region ConvolutionCalc interface implementation

        /** Method:  Load data for calculation */
        void IConvolution.LoadData(List<double> data, double nConv)  {
            this.data = data;
            this.nConv = nConv;
            this.convMean = stat.Mean(data) * nConv;
            this.convStDev = stat.StDev(data) * Math.Sqrt(nConv);
        }

        /** Method:  Load histogram for calculation */
        void IConvolution.LoadHistogram(double min, double max, double totFreqs, double range, int maxClasses, SDict<int, double> freqs, double nConv)  {
            Histogram hist = new Histogram();
            hist.LoadHist(min, max, totFreqs, range, maxClasses, freqs);
            this.nConv = nConv;
            this.convMean = hist.Mean * nConv;
            this.convStDev = hist.StDev * Math.Sqrt(nConv);
        }

        /** Method:  Acumulated probability for a certain value */
        double IConvolution.ProbabilityAcum(double x)   {
            if (convStDev == 0)
            {
                if (x < convMean) { return 0; }
                else { return 1; }
            }
            return nd.pNorm(x, convMean, convStDev);
        }

        /** Method:  Quantile for a certain probability */
        double IConvolution.Quantile(double p) {
            if (convStDev == 0) { return convMean; }
            if (p < 0 || p > 1) { throw new Exception("probability must be between 0 and 1"); }
            return nd.qNorm(p, convMean, convStDev);
        }

        /** Method:  if calculation is valid */
        /// <returns> true if it is valid, false if not </returns>
        bool IConvolution.IsValid()  {
            return true;
        }

        #endregion
    }
}
