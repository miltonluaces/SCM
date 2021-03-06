#region Imports

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Maths;
using Statistics;

#endregion

namespace MonteCarlo {

   /** Method:  Algorithmic class for any confidence interval estimation based in composition of gaussians */
    internal class KernelDist {

        #region Fields

        private SimAlg boot;
        private KernelDensity kDens;
        private RootSearch rs;
        private StatFunctions stat;
        private int maxClasses;
        private double maxValueThreshold;
        private double maxValue;
        private Histogram histogram;
        private bool mini;
        private bool all;
        private double pValue;
        private bool convolutionCalc;
        private bool multipleLeadtimes;
        private ConvolutionCalculator cc;
        private ConvProbCalc cpc;
        private double nLeadTimes;
        private PoissonCalc poissonCalc;
        //private MultiLeadTimeCalculator mltc;

        private Histogram histLdtimes;
        private int maxLdtimes;
     
        #endregion

        #region Constructor

       /** Method:  Constructor  
        iterations -  number of iterations 
        weigthValue -  weight value for bootstrapping 
        s -  standard deviation 
        eps -  epsilon for root search 
        noisePerc -  noise percentage 
        maxClasses -  maximum number of classes 
        maxValueThreshold -  threshold for max value 
        maxIterations -  max number of iterations 
        pValue -  p-value for hypothesis tests 
        maxCombValues -  maximum number of values in combinatory 
        maxCombTerms -  maximum number of terms for combinatory 
        maxFftTerms -  maximum number of terms for fast fourier 
        nThreads -  number of threads */
        internal KernelDist(int iterations, int maxConvolIterations, double weigthValue, double s, double eps, double noisePerc, int maxClasses, double maxValueThreshold, int maxIterations, double pValue, int maxCombValues, int maxCombTerms, int maxFftTerms, int nThreads) {
            this.maxClasses = maxClasses;
            this.maxValueThreshold = maxValueThreshold;
            this.boot = new SimAlg(iterations, weigthValue, pValue);
            this.histogram = new Histogram(maxClasses);
            this.kDens = new KernelDensity(s, maxIterations, maxClasses);
            this.stat = new StatFunctions();
            this.mini = false;
            this.pValue = pValue;
            this.rs = new RootSearch(0.01, 0.01, 100);
            this.cc = new ConvolutionCalculator();
            //this.mltc = new MultiLeadTimeCalculator();
            this.cc.MaxIt = maxConvolIterations;
            this.convolutionCalc = false;
            this.multipleLeadtimes = false;
            int maxIt = 100;
            this.poissonCalc = new PoissonCalc(0.01, maxIt, -1, -1, -1);
            this.poissonCalc.Method = PoissonCalc.MethodType.Hurdle;
            this.maxLdtimes = 5;
        }

        #endregion

        #region Properties

       /** Method:  maximum number of classes allowed */
        internal int MaxClasses {
            get { return maxClasses; }
            set { maxClasses = value; }
        }

       /** Method:  Threshold for the maximum value allowed in the distribution */
        internal double MaxValueThreshold {
            get { return maxValueThreshold; }
            set { maxValueThreshold = value; }
        }

       /** Method:  Maximum number of iterations */
        internal int MaxIterations {
            get { return kDens.MaxIterations; }
            set { kDens.MaxIterations = value; }
        }

       /** Method:  Maximum number of iterations for convolutions */
        internal int MaxConvolIterations {
            get { return cc.MaxIt; }
            set { this.cc.MaxIt = value; }
        }
        
       /** Method:  Time series with original values */
        internal List<double> OrigSerie { 
            get { return boot.OrigSerie; } 
        }
        
       /** Method:  Time series with current values */
        internal List<double> Serie { 
            get { return boot.Serie; } 
        }
        
       /** Method:  Time series with regression values */
        internal List<double> Regres { 
            get { return boot.Regres; } 
        }
        
       /** Method:  Time series with bias values */
        internal List<double> Bias { 
            get { return boot.Bias; } 
        }
        
       /** Method:  Time series with filtered values */
        internal List<double> FilteredSerie { 
            get { return boot.FilteredSerie; } 
        }
        
       /** Method:  List that represents a distribution (indexes = values, values = frequencies) */
        internal List<double> MeanFrec { 
            get { return histogram.GetFreqsList(); }
            set { boot.MeanFrec = value; }
        }

       /** Method:  The histogram of the actual distribution */
        internal Histogram Histogram {
            get { return histogram; }
            set { histogram = value; }
        }

       /** Method:  Threshold for outlier filtering */
        internal double OutlierThreshold {
            get { return boot.OutlierThreshold; }
            set { boot.OutlierThreshold = value; }
        }

       /** Method:  Threshold for daily outlier filtering */
        internal double RawOutlierThres {
            get { return boot.RawOutlierThres; }
            set { boot.RawOutlierThres = value; }
        }

       /** Method:  If outliers should be filtered */
        internal bool FilterOutliers {
            get { return boot.FilterOutliers; }
            set { boot.FilterOutliers = value; }
        }

       /** Method:  If bisection method should be used (else Montecarlo) */
        internal bool Bisection
        {
            get { return kDens.Bisection; }
            set { kDens.Bisection = value; }
        }
        
       /** Method:  Proportion of minimum of lead time observations according to time series length */
        internal double MinLeadTimeObsProportion {
            get { return boot.MinLeadTimeObsProportion; }
            set { boot.MinLeadTimeObsProportion = value; }
        }

       /** Method:  Minimum number of frequencies to perform outlier filtering */
        internal int MinFreqForOutlierFiltering {
            get { return boot.MinFreqForOutlierFiltering; }
            set { boot.MinFreqForOutlierFiltering = value; }
        }

       /** Method:  Minimum value */
        internal double Min { 
            get { return boot.Min; } 
        }

       /** Method:  Maximum value */
        internal double Max { 
            get { return boot.Max; } 
        }

       /** Method:  Range of values */
        internal double Range {
            get { return (Max - Min); } 
        }

       /** Method:  Number of filtered periods */
        internal int FilteredPeriods {
            get { return boot.OutlierIndexes.Count; } 
        }

       /** Method:  Number of values */
        internal int Count { 
            get { return boot.Serie.Count; } 
        }
        
       /** Method:  Min value of a filtered outlier */
        internal double MinOutlierFiltered {
            get { return boot.MinOutlierFiltered; } 
        }

       /** Method:  Min value of a non-filtered value */
        internal double MaxValueNotFiltered { 
            get { return boot.MaxValueNotFiltered; } 
        }

       /** Method:  List of indexes of values identified as outliers */
        internal List<int> OutlierIndexes {
            get { return boot.OutlierIndexes; } 
        }

       /** Method:  Index of first period to forecast */
        internal int FirstIndexToFcst { 
            get { return boot.FirstIndexToFcst; } 
        }

       /** Method:  List of calculation results (errors or warnings) */
        internal List<SignalCharact.ResultType> Results { 
            get { return boot.Results; } 
        }

       /** Method:  Array of wieghts for the clusters */
        internal double[] Weights {
            get { return boot.Weights; } 
        }

       /** Method:  Minimum of lead time observations */
        internal int MinLeadTimeObs {
            get { return boot.MinLeadTimeObs; }
            set { boot.MinLeadTimeObs = value; }
        }

       /** Method:  Maximum mean difference allowed between two clusters */
        internal double MaxDiff {
            get { return boot.MaxDiff; }
            set { boot.MaxDiff = value; }
        }

       /** Method:  Maximum standard deviation difference allowed within a cluster (joined) */
        internal double MaxStDev {
            get { return boot.MaxStDev; }
            set { boot.MaxStDev = value; }
        }

       /** Method:  Maximum standard deviation difference allowed between two clusters (joined) */
        internal double MaxStDevDiff {
            get { return boot.MaxStDevDiff; }
            set { boot.MaxStDevDiff = value; }
        }

       /** Method:  List of cluster means */
        internal List<double> Means {
            get { return boot.Means; } 
        }
        
       /** Method:  List of cluster standard deviations */
        internal List<double> StDevs {
            get { return boot.StDevs; } 
        }

       /** Method:  Number of deleted periods */
        internal int DeletedPeriods { 
            get { return boot.DeletedPeriods; } 
        }

       /** Method:  If mini resampling should be applied */
        internal bool Mini { 
            get { return mini; }
            set { mini = value; }
        }

       /** Method:  if is an sparse time series */
        internal bool IsSparse {
            get { return boot.IsSparse; }
            set { boot.IsSparse = value; }
        }
        
       /** Method:  Start of forgetting period (from the present to the past) */
        internal int maxWeighting {
            get { return boot.MaxWeighting; }
            set { boot.MaxWeighting = value; }
        }

       /** Method:   End of forgetting period (from the present to the past) */
        internal int minWeighting {
            get { return boot.MinWeighting; }
            set { boot.MinWeighting = value; }
        }

       /** Method:  Forgetting proportion beyond the end of forgetting period  (from the present to the past) */
        internal double MinWeight {
            get { return boot.MinWeight; }
            set { boot.MinWeight = value; }
        }

       /** Method:  Start of forgetting period (from the present to the past) ForSparse*/
        internal int maxWeightingForSparse {
            get { return poissonCalc.MaxWeighting; }
            set { poissonCalc.MaxWeighting = value;  }
        }

       /** Method:   End of forgetting period (from the present to the past) ForSparse */
        internal int minWeightingForSparse {
            get { return poissonCalc.MinWeighting; }
            set { poissonCalc.MinWeighting = value; }
        }

       /** Method:  Forgetting proportion beyond the end of forgetting period  (from the present to the past) ForSparse*/
        internal double MinWeightForSparse {
            get { return poissonCalc.MinWeight; }
            set { poissonCalc.MinWeight = value;  }
        }
        
       /** Method:  Mean of stationary period */
        internal double StatPeriodMean { 
            get { return boot.StatPeriodMean; } 
        }

       /** Method:  Level of determinism of the solution */
        internal double DeterminismLevel {
            get { return boot.DeterminismLevel; }
            set { boot.DeterminismLevel = value; }
        }

       /** Method:  Minimum weight allowed (if less than min, continuous intervals from the past are cleared) */
        internal double MinAggrWeight {
            get { return boot.MinAggrWeight; }
            set { boot.MinAggrWeight = value; }
        }

       /** Method:  Maximum number of samples */
        internal double MaxSamples {
            get { return boot.MaxSamples; }
            set { boot.MaxSamples = value; }
        }

       /** Method:  Percentage from all histogram columns to check stability of solution */
        internal double CheckFreqStability {
            get { return boot.CheckFreqStability; }
            set { boot.CheckFreqStability = value; }
        }

       /** Method:  Threshold of frequency variance for stability checking */
        internal double FreqVarThreshold {
            get { return boot.FreqVarThreshold; }
            set { boot.FreqVarThreshold = value; }
        }

       /** Method:  Final iterations needed for the calculation */
        internal int FinalIterations { 
            get { return boot.FinalIterations; } 
        }

       /** Method:  Number of values in stationary period */
        internal int StatPeriodCount { 
            get { return boot.StatPeriodCount; } 
        }

       /** Method:  load factor of grouped non-zero values on total grouped values */
        internal double LoadFactor {
            get { return boot.LoadFactor; }
        }

       /** Method:  if all percentiles should be calculated */
        internal bool AllPercentiles {
            get { return all; }
            set { all = value; }
        }

       /** Method:  noise level */
        internal double Noise {
            get { return boot.Noise; }
            set { boot.Noise = value; }
        }

       /** Method:  method used for calculation */
        internal ConvolutionCalculator.MethodType Method {
            get { return cc.Method; }
        }

       /** Method:  load factor threshold */
        internal double LoadFactorThreshold {
            get { return boot.LoadFactorThreshold; }
            set { boot.LoadFactorThreshold = value; }
        }

       /** Method:  first Non Zero for poisson */
        internal double FirstNonZero {
            get { 
                if(IsSparse) { return poissonCalc.FirstNonZero; }
                return -1;
            }
            set {
                if (IsSparse) {
                    poissonCalc.FirstNonZero = value;
                }
            }
     
        }

       /** Method:  If seasonality algorithms have been applied */
        internal bool Seasonality {
            get { return boot.Seasonality; }
        }
        
       /** Method:  Minimum autocorrelation for seasonality */
        internal double MinAutoCorr {
            get { return boot.MinAutoCorr; }
            set { boot.MinAutoCorr = value; }
        }

       /** Method:  Minimum amplitude for seasonality */
        internal double MinAmplitude {
            get { return boot.MinAmplitude; }
            set { boot.MinAmplitude = value; }
        }
        
       /** Method:  Minimum lag for checking seasonality */
        internal int MinLag {
            get { return boot.MinLag; }
            set { boot.MinLag = value; }
        }
        
       /** Method:  Maximum lag for checking seasonality */
        internal int MaxLag {
            get { return boot.MaxLag; }
            set { boot.MaxLag = value; }
        }

       /** Method:  Maximum lag for checking seasonality */
        internal int Lag {
            get { return boot.Lag; }
        }

       /** Method:  If mwAggregation  will be applied */
        internal bool MwAggregation {
            get { return boot.MwAggregation; }
            set { boot.MwAggregation = value; }
        }

       /** Method:  Exponent for non linear weighting of clusters (1-25) */
        internal double RootExpForClusterWeighting {
            get { return boot.RootExpForClusterWeighting; }
            set { boot.RootExpForClusterWeighting = value; }
        }

       /** Method:  First index of stationary period */
        internal int StatPeriodFirstIndex {
            get { return boot.StatPeriodFirstIndex; }
            set { boot.StatPeriodFirstIndex = value; }
        }

       /** Method:  Ratio mean/variance for sparse time series (threshold) */
        internal double RatioMvThreshold {
            get { return boot.RatioMvThreshold; }
            set { boot.RatioMvThreshold = value; }
        }

       /** Method:  Histogram of leadtimes (multiple leadtimes) */
        internal Histogram HistogramLdtimes {
            get { return histLdtimes; }
            set { histLdtimes = value; }
        }
        
       /** Method:  Maximum number of leadtimes in distribution (multiple leadtimes) */
        internal int MaxLdtimes {
            get { return maxLdtimes; }
            set { maxLdtimes = value; }
        }

       /** Method:  If it has been calculated with multiple leadtimes */
        internal bool MultipleLeadtimes {
            get { return multipleLeadtimes; }
        }
        
        #endregion

        #region internal Methods

        #region Load Data

        /** Method:  Load data 
        serie -  the serie to load 
        leadTime -  lead time 
         nLeadTimes -  n leadTimes si corresponde (convoluciones para long lead time), sino es 1 
       firstIndex -  index of first period for calculation 
        allPercentiles -  if all percentiles should be calculated  */
        internal void LoadData(List<double> serie, int leadTime, int nLeadTimes, int firstIndex, bool allPercentiles) {
            if (serie.Count == 0) { return; }

            multipleLeadtimes = false;
            convolutionCalc = (nLeadTimes >= 2);
            
            this.nLeadTimes = nLeadTimes;
            this.all = allPercentiles;
            this.histogram.Clear();
            if(serie.Count == 0) { return; }
            boot.OutlierThreshold = this.OutlierThreshold;
            boot.Calculate(serie, firstIndex, leadTime, nLeadTimes);

            this.maxValue = boot.Max * (1.0 + maxValueThreshold / 100.0);

            List<double> serieZero = new List<double>();
            serieZero.Add(0.0);
          
            kDens.Mini = false;
            if(boot.Serie.Count == 0) {
                kDens.Mini = true;
                histogram.LoadData(serieZero);
            }
            if(boot.Serie.Count <= 2) {
                kDens.Mini = true;
                histogram.LoadData(boot.Serie);
            } 
            else {
                if (IsSparse) { LoadSparse(); }
                else { histogram.LoadDist(boot.Histogram.Freqs, boot.Histogram.Min, boot.Histogram.Max); }
            }
    
            CalcPercentiles(nLeadTimes);

            //check reliability of an all zero curve 
            if (Math.Round(GetPercentile(99)) == 0) {
                LoadSparse();
                CalcPercentiles(nLeadTimes);
            }
        }

        private void LoadSparse() {
            List<double> serieZero = new List<double>();
            serieZero.Add(0.0);
            
            poissonCalc.LoadData(boot.Serie);
            histogram = poissonCalc.GetHistogram();
            if (histogram.Count <= 1) {
                histogram = new Histogram();
                histogram.LoadData(serieZero);
            }
            this.IsSparse = true;
            this.Results.Add(SignalCharact.ResultType.AsSparse);
        }

       
       /** Method:  Load data for only one leadtime (with no convolutions)  
        serie -  the serie to load 
        leadTime -  lead time 
        firstIndex -  index of first period for calculation 
        allPercentiles -  if all percentiles should be calculated  */
        internal void LoadData(List<double> serie, int leadTime, int firstIndex, bool allPercentiles) {
            multipleLeadtimes = false;
            LoadData(serie, leadTime, 1, firstIndex, allPercentiles);
        }

        internal double CalcRangeForDensity() {
            double tot = kDens.Histogram.TotFreqs;
            double sum = 0.0;
            foreach(double val in kDens.Histogram.GetValues()) {
                sum += kDens.Histogram.GetFreq(val);
                if(sum/tot > 0.95) {
                    if(val == 0) { return 1.0; }
                    return val; 
                }
            }
            return 0.0;
        }

        internal ConvolutionCalculator.MethodType SelectMethod(int nLeadTimes) {
            this.nLeadTimes = nLeadTimes;
            cc.Method = cc.SelectMethod(nLeadTimes);
            return cc.Method;
        }

       /** Method:  Load data for distribution of leadtimes  
        serie - 
        leadTimes - 
        firstIndex - 
        allPercentiles -  */
        internal void LoadData(List<double> serie, Histogram leadTimes, int firstIndex, bool all) {
            multipleLeadtimes = true;

            histLdtimes = leadTimes;
            int maxLeadTime = (int)histLdtimes.Max;
            LoadData(serie, maxLdtimes, 1,  firstIndex, false);
            //mltc.LoadData(histogram, maxLeadTime, histLdtimes, allPercentiles);
            this.multipleLeadtimes = true;
        }

        #region Calculate Percentiles

        private void CalcPercentiles(int nLdTimes) {
            if (histogram.Min == histogram.Max) { return; }
            kDens.Histogram = histogram;
            double s = CalcRangeForDensity() / 10.0;
            kDens.StDev = (s < 1.0) ? s : 1.0;

            if (nLdTimes < 2) {
                if (all) { kDens.CalcPercentiles(all); } 
                else { kDens.SetMaxInt(); }
            } 
            else {
                cc.LoadHistogram(histogram, nLeadTimes);
                cpc = new ConvProbCalc(cc, nLeadTimes);
                cpc.Bisection = this.Bisection;
                if (all) { cpc.CalcPercentiles(all); }
            }
        }


        #endregion
        
        #endregion
     
        #region Calculate

       /** Method:  Get a calculated probability 
        x -  the value for the calculation  */
        internal double GetProbability(double x) {
            //if (Sparse) { return poissonCalc.Probability(x,true); }
            if (convolutionCalc) { return cpc.Probability(x); }
            return kDens.Probability(x);
        }

       /** Method:  Get a probability of an interval 
        lower -  lower limit of the interval 
        upper -  upper limit of the interval */
        internal double GetProbability(double lower, double upper) {
            double lowProb = -1;
            double upProb = -1;
            //if (Sparse) { 
            //    lowProb = poissonCalc.Probability(lower, true);
            //    upProb = poissonCalc.Probability(upper, true);
            //}
            //else 
            if(convolutionCalc) {
                lowProb = cpc.Probability(lower);
                upProb = cpc.Probability(upper);
            }
            else {
                lowProb = kDens.Probability(lower);
                upProb = kDens.Probability(upper);
            }
            return upProb - lowProb;
        }

       /** Method:  Get a percentile */
        internal double GetPercentile(double p) {
            //if (multipleLeadtimes) { return mltc.GetPercentile(p); }
            if(histogram.Min == histogram.Max) { return histogram.Min; }
            if (p == 50) { return histogram.Mean * nLeadTimes; }
            if(p==100) { return maxValue * nLeadTimes; }
            
            double perc = -1.0;
            //if (Sparse) { return poissonCalc.Quantile(p / 100.0); }
            if (convolutionCalc) {
                if(all) { perc = cpc.GetPercentile(p); }
                else { perc = cpc.CalculatePercentile(p); }
            }
            else {
                if(all) { perc = kDens.GetPercentile(p); }
                else { perc = CalculatePercentile(p); }
            }
            if(perc > maxValue * nLeadTimes) { return maxValue * nLeadTimes; }
            return perc;
        }

            
       /** Method:  Calculate a percentile directly (not previously calculated) */
        internal double CalculatePercentile(double p) {
            if(p==100) { return maxValue * nLeadTimes; }
            double quant = -1;
            try {
                if(convolutionCalc) { quant = cpc.CalculatePercentile(p); }
                else { quant = kDens.CalculatePercentile(p); }
            } 
            catch { return -1; }
            if(quant > maxValue * nLeadTimes) { return maxValue * nLeadTimes; }
            return quant;
        }

        
        #endregion

        #region Statistics

       /** Method:  Calculation error  */
        internal double StdError() {
            return stat.StdError(this.Serie);
        }

       /** Method:  Calculates confidence intervals for percentiles  
        percentile -  percentile calculated 
        p -  percentile for the percentile 
        lwr -  lower ci limit 
        upr -  upper ci limit  */
        internal void CalculateCI(double percentile, double p, ref double lwr, ref double upr) {
            p = p / 100.0;
            KernelDensity kDens = new KernelDensity(1, this.MaxIterations, maxClasses);
            double s = CalcRangeForDensity() / 10.0;
            kDens.StDev = (s < 1.0) ? s : 1.0;
            List<double> percs = new List<double>();
            double perc;
            List<double> values;
            List<Sample> samples = new List<Sample>();
            Sample sample;
            for (int i = 0; i < 100; i++) {
                values = boot.GetSampleValues();
                sample = new Sample(values, boot.Weights, true);
                samples.Add(sample);
            }
            foreach (Sample samp in samples) {
                if (samp.Histogram.Min == samp.Histogram.Max) { continue; }
                kDens.Histogram = samp.Histogram;
                kDens.CalcPercentiles(false);
                perc = kDens.CalculatePercentile(percentile);
                if (perc >= 0) { percs.Add(perc); }
            }
            double mean = stat.Mean(percs);
            double sd = stat.StDev(percs);
            NormalDistrib nd = new NormalDistrib();
            upr = nd.qNorm(p, mean, sd);
            lwr = mean - (upr - mean);
        }

        
        #endregion

        #endregion

    }
}
