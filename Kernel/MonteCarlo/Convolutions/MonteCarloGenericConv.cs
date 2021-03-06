#region Imports

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Statistics;
using Maths;

#endregion

namespace MonteCarlo {

    /** Class: Class for any type of MonteCarlo convolutions */
    internal class MontecarloGenericConv {

        #region Fields 
        
        private List<Distribution> distributions;
        private KernelDensity de;
        private NormalDistrib nd;
        private RndGenerator rg;
        private int maxIt;
        private int normMin;

        private double mean;
        private double stDev;

        #endregion

        #region Constructor

        internal MontecarloGenericConv(int maxIt, int normMin) {
            distributions = new List<Distribution>();
            de = new KernelDensity(1, 100, 100);
            nd = new NormalDistrib();
            rg = new RndGenerator();
            this.maxIt = maxIt;
            this.normMin = normMin;

            this.mean = -1;
            this.stDev = -1;
        }

        #endregion

        #region Properties

        internal List<Distribution> Distributions {
            get { return distributions; }
        }

        internal Histogram Histogram {
            get { return de.Histogram; }
        }

        #endregion

        #region internal Methods

        #region Load Methods

        internal void AddData(List<double> data, double nLeadTimes, double factor) { 
            Histogram hist = new Histogram();
            hist.LoadData(data);
            AddHistogram(hist, nLeadTimes, factor);
        }

        internal void AddHistogram(Histogram histogram, double nLeadTimes, double factor) {
            Distribution dist = new Distribution(histogram, nLeadTimes, factor);
            distributions.Add(dist);
        }

        internal void LoadConvolution() {
            if (distributions.Count < normMin) { LoadMontConvolution(); } 
            else { LoadNormConvolution(); }
        }
        
        private void LoadMontConvolution() {
            List<double> convData = new List<double>();
            Histogram convHist = new Histogram();
          
            double sum;
            for (int i = 0; i < maxIt; i++) {
                sum = 0;
                foreach (Distribution dist in distributions) { 
                    dist.RawData = dist.Histogram.GetRawData();  
                }
                foreach (Distribution dist in distributions) {
                    int nLeadTimesInt = (int)dist.NLeadTimes;
                    double nLeadTimesRes = dist.NLeadTimes - nLeadTimesInt;
                    int[] indexes = new int[(int)dist.NLeadTimes + 1];
                    int indexRes = -1;

                    for (int j = 1; j <= nLeadTimesInt; j++) {
                        indexes[j - 1] = rg.NextInt(0, dist.RawData.Count - 1);
                        sum += dist.RawData[indexes[j - 1]];
                    }
                    if (nLeadTimesRes > 0) {
                        indexRes = rg.NextInt(0, dist.RawData.Count - 1);
                        sum = sum + dist.RawData[indexRes] * nLeadTimesRes;
                    }
                    convData.Add(sum * dist.Factor);
                }
            }

            convHist.LoadData(convData);
            de = new KernelDensity(1, 100, 100);
            de.LoadHist(convHist);
            de.SetMaxInt();
        }

        private void LoadNormConvolution() {
            double totCount = 0;
            double sum = 0;
            
            foreach (Distribution dist in distributions) {
                int nLeadTimesInt = (int)dist.NLeadTimes;
                double nLeadTimesRes = dist.NLeadTimes - nLeadTimesInt;
                
                for (int i = 0; i < nLeadTimesInt; i++) {
                    sum += dist.Histogram.Mean * dist.Histogram.Count;
                    totCount++;
                }
                if (nLeadTimesRes > 0) {
                    sum += dist.Histogram.Mean * dist.Histogram.Count * nLeadTimesRes;
                    totCount++;
                }
              
            }
            mean = sum / (double)totCount;

            foreach (Distribution dist in distributions) {
                stDev += dist.Histogram.StDev * ((double)dist.Histogram.Count / (double)totCount);
            }
        }
        
        #endregion

        #region Calculations

        internal double ProbabilityAcum(double x) {
            if (distributions.Count > normMin) { return nd.pNorm(x, mean, stDev); } 
            else { return de.Probability(x); }
        }

        internal double Quantile(double p) {
            if (p < 0 || p > 1) { throw new Exception("Error Probability must be between 0 and 1"); }
            if (distributions.Count == 0) { return 0; }
            if (distributions.Count > normMin) { return nd.qNorm(p, mean, stDev); } 
            else { return de.CalculatePercentile(p * 100.0); }
        }

        #endregion
        
        #endregion

        #region Class Distribution

        internal class Distribution {
            
            private List<double> values;
            private List<double> freqs;
            private Histogram histogram;
            private double nLeadTimes;
            private double factor;
            private List<double> rawData;

            internal Distribution(Histogram histogram, double nLeadTimes, double factor) {
                this.histogram = histogram;
                this.nLeadTimes = nLeadTimes;
                this.values = histogram.GetValues();
                this.freqs = histogram.GetFreqs();
                this.factor = factor;
                this.rawData = new List<double>();
            }

            internal List<double> Values { get { return values; } }
            internal List<double> Freqs { get { return freqs; } }
            internal Histogram Histogram { get { return histogram; } }
            internal double Factor { get { return factor; } }
            internal double NLeadTimes { get { return nLeadTimes; } }
            internal double Min { get { return factor; } }
            internal double Max { get { return factor; } }
            internal List<double> RawData { 
                get { return rawData; }
                set { rawData = value; }
            }
        }

        #endregion

    }
}
