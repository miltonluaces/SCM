#region Imports

using System;
using System.Collections.Generic;
using System.Text;
using Statistics;
using Maths;

#endregion


namespace MonteCarlo {

   /** Method:  Wrapper of CIEstimation class, speciphical for demand estimations  */
    internal class SimFcst {

        #region Fields

        private KernelDist stock;
        private ConvProbCalc obsol;
        private Dictionary<int, double> ltFcsts;
        private int stockouts;
        private double backorder;
        private string skuId;
        private int leadTime;
        private int iterations;
        private int maxConvolIterations;
        private int maxLeadTime;
        private List<double> serieTr;
        private RndGenerator rand = new RndGenerator();
        private StatFunctions stat = new StatFunctions();
        private ConvolutionCalculator cc;
        private bool ccHistogramLoaded;
        private double minLot;
        private int replPrd;
        private double annualSimFcst;
        private double replenishmentFcst;
        private int suppliesPerYear;
        private double pValue;
       
        private int maxCombValues;

        private int maxCombTerms;
        private int maxFftTerms;
        private int nThreads;
        private double restObs;

        private double loadFactorThreshold;
        private int replenishmentPeriod;
        

        #endregion

        #region Constructor

       /** Method:  Constructor  
        maxClasses -  maximum number of classes 
        maxValueThreshold -  threshold for maximum value 
        maxIterations -  maximum number of iterations 
        pValue -  p-Value for hypothesis tests 
        nThreads -  number of threads for multithreading (1 for mono) 
        loadFactorThreshold -  threshold for sparse series */
        internal SimFcst(int maxClasses, double maxValueThreshold, int maxIterations, double pValue, int nThreads, double loadFactorThreshold) { 
            this.iterations = 100;
            double weigthValue = 1;
            double s = 1;
            double eps = 0.01;
            double noisePerc = 0.1;
            this.pValue = pValue;
            this.maxCombValues = 20;
            this.maxCombTerms = 4;
            this.maxFftTerms =  15;
            this.maxConvolIterations = 1500;
            this.stock = new KernelDist(iterations, maxConvolIterations, weigthValue, s, eps, noisePerc, maxClasses, maxValueThreshold, maxIterations, pValue, maxCombValues, maxCombTerms, maxFftTerms, nThreads);
            this.stock.MinFreqForOutlierFiltering = MinFreqForOutlierFiltering;
            this.ltFcsts = new Dictionary<int, double>();
            this.serieTr = new List<double>();
            this.MaxLeadTime = 30;
            this.nThreads = nThreads;
            this.cc = new ConvolutionCalculator(0.5, 20, maxConvolIterations, maxCombTerms, maxFftTerms, 0.05, 0.1);
            this.ccHistogramLoaded = false;
            this.rand.Reset();
            this.loadFactorThreshold = loadFactorThreshold;
        }

        #endregion

        #region Properties

       /** Method:  Maximum number of classes */
        internal int MaxClasses {
            get { return stock.MaxClasses; }
            set { stock.MaxClasses = value; }
        }

       /** Method:  Threshold for maximum value allowed */
        internal double MaxValueThreshold {
            get { return stock.MaxValueThreshold; }
            set { 
                stock.MaxValueThreshold = value;
            }
        }

       /** Method:  Maximum number of iterations */
        internal int MaxIterations {
            get { return stock.MaxIterations; }
            set { 
                stock.MaxIterations = value;
            }
        }

       /** Method:  Maximum number of iterations for convolutions */
        internal int MaxConvolIterations {
            get { return maxConvolIterations; }
            set { 
                maxConvolIterations = value;
                this.stock.MaxConvolIterations = maxConvolIterations;
                this.cc.MaxIt = maxConvolIterations;
            }
        }
        
       /** Method:  Max value allowed for leadtime */
        internal int MaxLeadTime {
            get { return maxLeadTime; }
            set { maxLeadTime = value; }
        }

       /** Method:  Time series with original values */
        internal List<double> OrigSerie {
            get { return stock.OrigSerie; }
        }

       /** Method:  Time series with current values for leadtime  distribution  */
        internal List<double> SerieLeadTime {
            get { return stock.Serie; }
        }

       /** Method:  List that represents a distribution, for leadtime  distribution  */
        internal List<double> MeanFrecLeadTime {
            get { return stock.MeanFrec; }
        }

       /** Method:  ID of the Sku */
        internal string SkuId { 
            get { return skuId; } 
        }

       /** Method:  Dictionary with the distribution for leadtime  distribution */
        internal Dictionary<double, double> LeadTimeValueFreqs {
            get {
                Dictionary<double, double> ltValueFrecs = new Dictionary<double, double>();
                foreach(double val in stock.Histogram.GetValues()) {
                    ltValueFrecs.Add(val, stock.Histogram.GetFreq(val));
                }
                return ltValueFrecs;
            }
        }

       /** Method:  Threshold for oultiers filtering for lead time  distribution */
        internal double OutlierThreshold {
            get { return stock.OutlierThreshold; }
            set { stock.OutlierThreshold = value; }
        }

       /** Method:  Threshold for daily oultier filtering for lead time  distribution */
        internal double RawOutlierThres {
            get { return stock.RawOutlierThres; }
            set { stock.RawOutlierThres = value; }
        }

       /** Method:  If outilers should be filtered in case of lead time distribution */
        internal bool FilterOutliers {
            get { return stock.FilterOutliers; }
            set { stock.FilterOutliers = value; }
        }

       /** Method:  Minimum observations in proportion with whole observations */
        internal double MinLeadTimeObsProportion {
            get { return stock.MinLeadTimeObsProportion; }
            set { stock.MinLeadTimeObsProportion = value; }
        }

       /** Method:  Minimum number of frequencies to allow outlier filtering */
        internal int MinFreqForOutlierFiltering {
            get { return stock.MinFreqForOutlierFiltering; }
            set { stock.MinFreqForOutlierFiltering = value; }
        }

       /** Method:  Minimun number of observations for lead time distribution */
        internal int StockMinLeadTimeObs {
            get { return stock.MinLeadTimeObs; }
            set { stock.MinLeadTimeObs = value; }
        }

       /** Method:  Level of determinism of the solution */
        internal double DeterminismLevel {
            get { return stock.DeterminismLevel; }
            set { stock.DeterminismLevel = value; }
        }

       /** Method:  Minimum weight allowed (if less, continuous intervals from the beginning are cleared) */
        internal double MinAggrWeight {
            get { return stock.MinAggrWeight; }
            set { stock.MinAggrWeight = value; }
        }

       /** Method:  Number of deleted periods in case of lead time serie */
        internal int DeletedPeriods { 
            get { return stock.DeletedPeriods; } 
        }

       /** Method:  Number of filtered periods in case of lead time serie  */
        internal int FilteredPeriods {
            get {
                if(stock.OutlierIndexes != null) {
                    return stock.OutlierIndexes.Count;
                } 
                else {
                    return 0;
                }
            }
        }

       /** Method:  Number of periods in lead time serie */
        internal int Count { 
            get { return stock.Serie.Count; } 
        }

       /** Method:  Value of minimum outlier filtered in lead time serie */
        internal double MinOutlierFiltered { 
            get { return stock.MinOutlierFiltered; } 
        }

       /** Method:  Minimum value not filtered in lead time serie  */
        internal double MaxValueNotFiltered { 
            get { return stock.MaxValueNotFiltered; } 
        }

       /** Method:  List of indexes of oultiers in case of lead time serie */
        internal List<int> OutlierIndexes {
            get { return stock.OutlierIndexes; } 
        }

       /** Method:  Index of first period to forecast in lead time serie */
        internal int FirstIndexToFcst { 
            get { return stock.FirstIndexToFcst; } 
        }

       /** Method:  List of results of the calculation */ 
        internal List<SignalCharact.ResultType> StockResults { 
            get { return stock.Results; } 
        }

       /** Method:  Lead time */
        internal int LeadTime { 
            get { return leadTime; }
            set { leadTime = value; }
        }

       /** Method:  Filtered lead time serie */
        internal List<double> StockFilteredSerie { 
            get { return stock.FilteredSerie; } 
        }

       /** Method:  Maximum difference allowed between two clusters (joined) */
        internal double MaxDiff {
            get { return stock.MaxDiff; }
            set { stock.MaxDiff = value; }
        }

       /** Method:  Maximum standard deviation difference allowed within a cluster (joined) */
        internal double MaxStDev {
            get { return stock.MaxStDev; }
            set { stock.MaxStDev = value; }
        }

       /** Method:  Maximum standard deviation difference allowed between two clusters (joined) */
        internal double MaxStDevDiff {
            get { return stock.MaxStDevDiff; }
            set { stock.MaxStDevDiff = value; }
        }

       /** Method:  List of cluster means for lead time series */
        internal List<double> Means { get { return stock.Means; } }

       /** Method:  List of cluster standard deviations for lead time series */
        internal List<double> StDevs { 
            get { return stock.StDevs; } 
        }

       /** Method:  Array of weight for lead time series clusters */
        internal double[] Weights { 
            get { return stock.Weights; } 
        }

       /** Method:  Start of forgetting period (from the present to the past) */
        internal int maxWeighting {
            get { return stock.maxWeighting; }
            set { stock.maxWeighting = value; }
        }

       /** Method:   End of forgetting period (from the present to the past) */
        internal int minWeighting {
            get { return stock.minWeighting; }
            set { stock.minWeighting = value; }
        }

       /** Method:  Forgetting proportion beyond the end of forgetting period  (from the present to the past) */
        internal double minWeight {
            get { return stock.MinWeight; }
            set { stock.MinWeight = value; }
        }

       /** Method:  Start of forgetting period (from the present to the past) ForSparse*/
        internal int maxWeightingForSparse {
            get { return stock.maxWeightingForSparse; }
            set { stock.maxWeightingForSparse = value; }
        }

       /** Method:   End of forgetting period (from the present to the past) ForSparse*/
        internal int minWeightingForSparse {
            get { return stock.minWeightingForSparse; }
            set { stock.minWeightingForSparse = value; }
        }

       /** Method:  Forgetting proportion beyond the end of forgetting period  (from the present to the past) ForSparse*/
        internal double minWeighForSparse {
            get { return stock.MinWeightForSparse; }
            set { stock.MinWeightForSparse = value; }
        }
        
       /** Method:  Maximum number of samples */
        internal double MaxSamples {
            get { return stock.MaxSamples; }
            set { stock.MaxSamples = value; }
        }

       /** Method:  Percentage from all histogram columns to check stability of solution */
        internal double CheckFreqStability {
            get { return stock.CheckFreqStability; }
            set { stock.CheckFreqStability = value; }
        }

       /** Method:  Threshold of frequency variance for stability checking */
        internal double FreqVarThreshold {
            get { return stock.FreqVarThreshold; }
            set { stock.FreqVarThreshold = value; }
        }

       /** Method:  Mean of stationary period for lead time series */
        internal double StatPeriodMean { 
            get { return stock.StatPeriodMean; } 
        }

       /** Method:  Number of final iterations for lead time distribution */
        internal int FinalIterations { 
            get { return stock.FinalIterations; } 
        }

       /** Method:  Number of values in stationary period in case of lead time series */
        internal int StatPeriodCount { 
            get { return stock.StatPeriodCount; } 
        }

       /** Method:  Annual Sim Forecast */
        internal double AnnualSimFcst {
            get { return annualSimFcst; }
        }

       /** Method:  Replenishment forecast */
        internal double ReplenishmentFcst {
            get { return replenishmentFcst; }
        }
        
       /** Method:  number of stockouts */
        internal int Stockouts {
            get { return stockouts; }
        }
        
       /** Method:  List of calculated backorder */
        internal double Backorder {
            get { return backorder; }
        }

       /** Method:  Minimum lot */
        internal double MinLot {
            get { return minLot; }
        }

       /** Method:  Replenishment period */
        internal int ReplPrd {
            get { return replPrd; }
            set { replPrd = value; }
        }

       /** Method:  load factor of grouped non-zero values on total grouped values */
        internal double LoadFactor {
            get { return stock.LoadFactor; }
        }

       /** Method:  p-value for hypothesis tests */
        internal double PValue {
            get { return pValue; }
        }

       /** Method:  Mean of leadtime forecast distribution */
        internal double LeadTimeFcstMean {
            get { return stock.Histogram.Mean; }
        }

       /** Method:  Forecast distribution */
        internal Histogram Histogram {
            get { return stock.Histogram; }
        }

       /** Method:  if all percentiles should be calculated */
        internal bool AllPercentiles
        {
            get { return stock.AllPercentiles; }
            set { stock.AllPercentiles = value; }
        }

       /** Method:  If bisection method should be used (else Montecarlo) */
        internal bool Bisection
        {
            get { return stock.Bisection; }
            set { stock.Bisection = value; }
        }

       /** Method:  noise level */
        internal double StockNoise {
            get { return stock.Noise; }
            set { stock.Noise = value; }
        }

       /** Method:  method used for calculation */
        internal ConvolutionCalculator.MethodType Method {
            get { return stock.Method; }
        }

       /** Method:  calculator */
        internal ConvolutionCalculator CC {
            get { return this.cc; }
            set { this.cc = value; }
        }

       /** Method:  if time series is sparse */
        internal bool IsSparse {
            get { return stock.IsSparse; }
        }

       /** Method:  ratio of percentile 90-99-9 range upon all */
        internal double Percentile90Ratio {
            get {
                if (Histogram == null || Histogram.Count == 0) { return -1; }
                double perc90 = this.GetLeadTimeFcst(90);
                double perc99 = this.GetLeadTimeFcst(99);
                return (perc99 - perc90)/perc99;
            }
        }

       /** Method:  If histogram has been loaded for calculation */
        internal bool CcHistogramLoaded {
            get { return ccHistogramLoaded; }
            set { ccHistogramLoaded = value; }
        }

       /** Method:  load factor threshold */
        internal double LoadFactorThreshold {
            get { return stock.LoadFactorThreshold; }
            set { stock.LoadFactorThreshold = value; }
        }

       /** Method:  first Non Zero for poisson */
        internal double FirstNonZero {
            get { return stock.FirstNonZero; }
            set { stock.FirstNonZero = value; }
        }

       /** Method:  If seasonality algorithms have been applied */
        internal bool Seasonality {
            get { return stock.Seasonality; }
        }
        
       /** Method:  Minimum autocorrelation for seasonality */
        internal double MinAutoCorr {
            get { return stock.MinAutoCorr; }
            set { stock.MinAutoCorr = value; }
        }

       /** Method:  Minimum amplitude for seasonality */
        internal double MinAmplitude {
            get { return stock.MinAmplitude; }
            set { stock.MinAmplitude = value; }
        }
   
       /** Method:  Minimum lag for checking seasonality */
        internal int MinLag {
            get { return stock.MinLag; }
            set { stock.MinLag = value; }
        }
        
       /** Method:  Maximum lag for checking seasonality */
        internal int MaxLag {
            get { return stock.MaxLag; }
            set { stock.MaxLag = value; }
        }

       /** Method:  Maximum lag for checking seasonality */
        internal int Lag {
            get { return stock.Lag; }
        }

       /** Method:  If mwAggregation grouping will be applied */
        internal bool MwAggregation {
            get { return stock.MwAggregation; }
            set { stock.MwAggregation = value; }
        }

       /** Method:  Exponent for non linear weighting of clusters (1-25) */
        internal double RootExpForClusterWeighting {
            get { return stock.RootExpForClusterWeighting; }
            set { stock.RootExpForClusterWeighting = value; }
        }

       /** Method:  First index of stationary period */
        internal int StatPeriodFirstIndex {
            get { return stock.StatPeriodFirstIndex; }
            set { stock.StatPeriodFirstIndex = value; }
        }

       /** Method:  Ratio mean/variance for sparse time series (threshold) */
        internal double RatioMvThreshold {
            get { return stock.RatioMvThreshold; }
            set { stock.RatioMvThreshold = value; }
        }

       /** Method:  Histogram of leadtimes (multiple leadtimes) */
        internal Histogram HistogramLdtimes {
            get { return stock.HistogramLdtimes; }
            set { stock.HistogramLdtimes = value; }
        }

       /** Method:  If it has been calculated with multiple leadtimes */
        internal bool MultipleLeadtimes {
            get { return stock.MultipleLeadtimes; }
        }

        #endregion

        #region internal Methods

        #region Load Data

        /** Method:  Load data for calculation  
        skuId -  the id of the sku 
        serie -  the time series 
        leadTime -  lead time 
        obsTime -  obsolescence time 
        firstIndex -  index of first period for calculation 
        allPercentiles -  if all percentiles should be calculated 
        minLot -  minimum lot allowed 
        fromFirstNonZero -  if the serie should be calculated from the first non zero (remove zeros) 
        replPrd -  replenishment period */
        internal void LoadData(string skuId, List<double> serie, int leadTime, int firstIndex, bool allPercentiles, double minLot, int replPrd, bool fromFirstNonZero) {
            if (serie == null || serie.Count == 1) { return; }
            while(fromFirstNonZero && serie.Count > 1) {
                if(serie[0] != 0) { break; }
                serie.RemoveAt(0);
                firstIndex--;
            }
            if (firstIndex < 0) { firstIndex = 0; }
            this.skuId = skuId;
            this.leadTime = leadTime;
            this.minLot = minLot;
            this.replPrd = replPrd;

            double nLeadTimes = (double)leadTime / (double)maxLeadTime;

            if (leadTime <= maxLeadTime) { stock.LoadData(serie, leadTime, 1, firstIndex, allPercentiles); } 
            else { stock.LoadData(serie, maxLeadTime, (int)Math.Round(nLeadTimes), firstIndex, allPercentiles); }
        }

        internal void LoadData(string skuId, List<double> serie, Histogram ldtimes, int firstIndex, bool allPercentiles, double minLot, int replPrd, bool fromFirstNonZero) {
            while (fromFirstNonZero) {
                if (serie[0] != 0 || serie.Count == 1) { break; }
                serie.RemoveAt(0);
                firstIndex--;
            }
            if (firstIndex < 0) { firstIndex = 0; }
            this.skuId = skuId;
            this.leadTime = -1;
            this.minLot = minLot;
            this.replPrd = replPrd;
            stock.LoadData(serie, ldtimes, firstIndex, allPercentiles);
        }
        
        #endregion

        #region LeadTimeFcst

       /** Method:  Get a lead time forecast */
        internal double GetLeadTimeFcst(double p) {
            if (Histogram.Count == 1) { return Histogram.GetValues()[0]; }
            if(p != 50.0 && p < 80.0) { return 0.0; }
            if (p == 100.0) { return stock.GetPercentile(99.9); }
            return stock.GetPercentile(p);  
        }

       /** Method:  Calculate Percentile (forced for outlier demand) */
        internal double CalculatePercentile(double p) {
            if (Histogram.Count == 1) { return Histogram.GetValues()[0]; }
            if (p == 100.0) { return stock.GetPercentile(99.9); }
            return stock.CalculatePercentile(p);
        }

        internal double Probability(double x) {
            return stock.GetProbability(x);
        }
        
        #endregion
        
        #region Statistics

        internal ConvolutionCalculator.MethodType SelectMethod(int nLeadTimes) {
            return stock.SelectMethod(nLeadTimes);
        }
        
       /** Method:  Calculate lead time forecast for rollin year */
        internal double CalculateAnnualSimFcst(double p) {
            if(this.leadTime == 0) { return 0; }
            annualSimFcst = -1;
            int nLeadTimes = 365 / Math.Min(leadTime, maxLeadTime);
            ConvolutionCalculator.MethodType method = SelectMethod(nLeadTimes);
            this.cc.LoadHistogram(this.Histogram, nLeadTimes);
            cc.Method = method;
            LoadHistogram(cc, nLeadTimes);
            annualSimFcst = cc.Quantile(p);
            return AnnualSimFcst;
        }
        
       /** Method:  Calculate stock outs  
        serviceLevel -  service level 
        annualFcst -  rolling annual forecast */
        internal int CalculateStockouts(double serviceLevel, double annualFcst) {
            if (this.leadTime == 0) { return 0; }
            if(annualFcst < 1) { return 0; }
            if(replPrd == 0) { throw new Exception("Repleinsment period must be greater than zero"); }

            //supplies per year and stockouts
            suppliesPerYear = 365 / (replPrd-leadTime);
            int nLots = (int)(annualFcst / minLot);
            if(nLots < suppliesPerYear) { suppliesPerYear = nLots; }
            this.stockouts = (int)(Math.Round((1.0 - serviceLevel/100.0) * suppliesPerYear));
            return stockouts;
        }

       /** Method:  Calculate non served demand within a year  
        stock -  previously calculated stock 
        serviceLevel -  service level 
        stockouts -  number of stockouts 
        confidenceLevel -  confidence level of calculation 
        Important: Annual forecast must be previously calculated */
        internal double CalculateBackorder(double serviceLevel, double stock, int stockouts, double confidenceLevel) {
            if (cc.Histogram == null || cc.Histogram.Count == 0) {
                int nLeadTimes = 365 / Math.Min(leadTime, maxLeadTime);
                LoadHistogram(cc, nLeadTimes); 
            }
            //cc.SelectMethod(stockouts); 
            if (stockouts >= cc.MinNorm) { cc.Method = ConvolutionCalculator.MethodType.Normal; } 
            else { cc.Method = ConvolutionCalculator.MethodType.Montecarlo; }
            if (this.leadTime == 0) { return 0; }
            if (stockouts < 1) { return 0; }
            LoadHistogram(cc, stockouts);
            this.backorder = cc.Quantile(confidenceLevel);
            return backorder;
        }

       /** Method:  SuppliesPerYear */
        internal int SuppliesPerYear {
            get { return suppliesPerYear; }
            set { suppliesPerYear = value; }
        }

       /** Method:  Replenishment Forecast */
        internal double CalculateReplenishmentFcst(double nLeadTimes, double p) {
            if (this.leadTime == 0) { return 0; }
            cc.Method = ConvolutionCalculator.MethodType.None;
            if (p <= 0) { replenishmentFcst = 0; }
            this.cc.LoadHistogram(this.Histogram, nLeadTimes); 
            ConvProbCalc cpc = new ConvProbCalc(this.cc, nLeadTimes);
            replenishmentFcst = cpc.CalculatePercentile(p);
            return replenishmentFcst;
        }

        private void LoadHistogram(ConvolutionCalculator cc, double n) {
            //if(ccHistogramLoaded) { return; }
            cc.LoadHistogram(stock.Histogram, n);
            ccHistogramLoaded = true;
        }
        
        #endregion

        #region Monitoring

       /** Method:  Calculates confidence intervals for percentiles 
        percentile -  percentile calculated 
        p -  percentile for the percentile 
        lwr -  lower ci limit 
        upr -  upper ci limit */
        internal void CalculateCI(double percentile, double p, ref double lwr, ref double upr) {
            stock.CalculateCI(percentile, p, ref lwr, ref upr);
        }

        #endregion

        #endregion

    }
}
