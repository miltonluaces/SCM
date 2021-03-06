#region Imports

using System;
using System.Collections.Generic;
using System.IO;
using Statistics;
using Maths;

#endregion

namespace MonteCarlo
{

    /** Method:  SimFcst class  */
    internal class SimAlg {

        #region Fields

        //parameters
        private double noise;
        private double noiseStd;
        private int iterations;
        private double varCoeffThreshold;
        private double outlierThreshold;
        private double rawOutlierThres;
        private double maxNorm;
        private bool filterOutliers;
        private int minFreqForOutlierFiltering;
        private double determinismLevel;
        private double deterministicIterations;
        private int deletedPeriods;
        private double minAggrWeight;
        private int maxWeighting;
        private int minWeighting;
        private double maxSamples;
        private double checkFreqStability;
        private bool checkSeasonality;
        private bool seasonality;
        private int minSeasonCount;
        private bool mwAggregation;
        private bool sampleFromOriginalValues = false;
        private double freqVarThreshold;
        private int finalIterations;
        private double pValue = 0.05;

        private double minAutoCorr;
        private double minAmplitude;
        private int minLag;
        private int maxLag;


        //series
        private List<double> meanFrec;

        //samples
        private List<Sample> samples;
        private Sample origSample;
        private Histogram histogram;
        private int count;
        private double biasMean = 0.0;
        private double biasStDev = 0.0;

        //result
        private List<SignalCharact.ResultType> results;
        private int statPeriodCount;

        //functions
        private Functions func;
        private StatFunctions stat;
        private RndGenerator randGen;
        private Polynom poly;
        private NormalDistrib ns;
        private Wavelets ws;
        private SignalCharact sg;

        #endregion

        #region Constructors

        /** Method:  Constructor  
        iterations -  number of iterations 
        weigthValue - weight 
        pValue -  p-value for hypothesis tests */
        internal SimAlg(int iterations, double weigthValue, double pValue) {
            this.noiseStd = 0.1;
            this.iterations = iterations;
            this.samples = new List<Sample>();

            int minLeadTimeObs = 10;
            this.minFreqForOutlierFiltering = 4;
            this.varCoeffThreshold = 0.5;
            this.outlierThreshold = 0.95;
            this.rawOutlierThres = 0.95;
            this.maxNorm = 100.0;
            this.filterOutliers = true;
            this.results = new List<SignalCharact.ResultType>();
            this.histogram = new Histogram();

            this.func = new Functions();
            this.randGen = new RndGenerator();
            this.poly = new Polynom();
            this.stat = new StatFunctions();
            this.ns = new NormalDistrib();
            this.ws = new Wavelets();
            double maxDiff = 5;
            double maxStDev = 10;
            this.determinismLevel = 0.3;
            this.deletedPeriods = 0;
            this.pValue = pValue;
            this.noise = 0.15;
            this.checkSeasonality = false;
            this.seasonality = false;
            this.minSeasonCount = 12;
            this.minAutoCorr = 0.7;
            this.maxLag = 12;
            this.sg = new SignalCharact(minLeadTimeObs, varCoeffThreshold, filterOutliers, minFreqForOutlierFiltering, outlierThreshold, rawOutlierThres, maxNorm, maxDiff, maxStDev, pValue);
        }

        /** Method:  Constructor  
         grade -  polynomial grade 
         noiseStd -  percentage of noise 
         iterations -  number of iterations 
         pValue -  p value for hypothesis tests */
        internal SimAlg(int grade, int noiseStd, int iterations, double pValue) {
            this.noiseStd = noiseStd;
            this.iterations = iterations;
            this.samples = new List<Sample>();
            this.histogram = new Histogram();
            int minLeadTimeObs = 10;

            this.minFreqForOutlierFiltering = 4;
            this.varCoeffThreshold = 0.5;
            this.outlierThreshold = 0.95;
            this.rawOutlierThres = 0.95;
            this.maxNorm = 100.0;
            this.filterOutliers = true;

            this.results = new List<SignalCharact.ResultType>();

            this.func = new Functions();
            this.randGen = new RndGenerator();
            this.poly = new Polynom();
            this.stat = new StatFunctions();
            this.ns = new NormalDistrib();
            this.ws = new Wavelets();
            double maxDiff = 5;
            double maxStDev = 10;
            this.determinismLevel = 0.3;
            this.deletedPeriods = 0;
            this.pValue = pValue;
            this.checkSeasonality = false;
            this.seasonality = false;
            this.minAutoCorr = 0.7;
            this.maxLag = 12;
            this.sg = new SignalCharact(minLeadTimeObs, varCoeffThreshold, filterOutliers, minFreqForOutlierFiltering, outlierThreshold, rawOutlierThres, maxNorm, maxDiff, maxStDev, pValue);
        }

        #endregion

        #region Properties

        /** Method:  time series with original values */
        internal List<double> OrigSerie {
            get { return sg.OrigSerie; }
        }

        /** Method:  time series with current values */
        internal List<double> Serie {
            get { return sg.Serie; }
        }

        /** Method:  regression time series */
        internal List<double> Regres {
            get { return sg.Regres; }
        }

        /** Method:  bias time series */
        internal List<double> Bias {
            get { return sg.Bias; }
        }

        /** Method:  filtered time series */
        internal List<double> FilteredSerie {
            get { return sg.FilteredSerie; }
        }

        /** Method:  list that represents frequencies (indexes = values, values = frequencies) */
        internal List<double> MeanFrec {
            get { return meanFrec; }
            set { meanFrec = value; }
        }

        /** Method:  Proportion of minimum of lead time observations according to time series length */
        internal double MinLeadTimeObsProportion {
            get { return sg.MinLeadTimeObsProportion; }
            set { sg.MinLeadTimeObsProportion = value; }
        }

        /** Method:  Minimum of lead time observations */
        internal int MinLeadTimeObs {
            get { return sg.MinCount; }
            set { sg.MinCount = value; }
        }

        /** Method:  Threshold for outlier filtering */
        internal double OutlierThreshold {
            get { return outlierThreshold; }
            set {
                outlierThreshold = value;
                sg.OutlierThreshold = outlierThreshold;
            }
        }

        /** Method:  Threshold for outlier filtering */
        internal double RawOutlierThres {
            get { return rawOutlierThres; }
            set  {
                rawOutlierThres = value;
                sg.RawOutlierThres = rawOutlierThres;
            }
        }

        /** Method:  If outliers should be filtered */
        internal bool FilterOutliers {
            get { return filterOutliers; }
            set { filterOutliers = value; }
        }

        /** Method:  Minimum number of frequencies to perform outlier filtering */
        internal int MinFreqForOutlierFiltering {
            get { return minFreqForOutlierFiltering; }
            set { minFreqForOutlierFiltering = value; }
        }

        /** Method:  Min value */
        internal double Min {
            get { return sg.Min; }
        }

        /** Method:  Max value */
        internal double Max {
            get { return sg.Max; }
        }

        /** Method:  Range value */
        internal double Range {
            get { return (Max - Min); }
        }

        /** Method:  Number of filtered periods */
        internal int FilteredPeriods {
            get { return sg.OutlierIndexes.Count; }
        }

        /** Method:  Number of data */
        internal int Count {
            get { return sg.Serie.Count; }
        }

        /** Method:  Min value of a filtered outlier */
        internal double MinOutlierFiltered {
            get { return sg.MinOutlierFiltered; }
        }

        /** Method:  Max value of a non filtered value */
        internal double MaxValueNotFiltered {
            get { return sg.MaxValueNotFiltered; }
        }

        /** Method:  List of indexes of values identified as outliers */
        internal List<int> OutlierIndexes {
            get { return sg.OutlierIndexes; }
        }

        /** Method:  Index of first period to forecast */
        internal int FirstIndexToFcst {
            get { return sg.FirstIndexToFcst; }
        }

        /** Method:  List of calculation results (errors or warnings) */
        internal List<SignalCharact.ResultType> Results {
            get { return results; }
        }

        /** Method:  Histogram of the actual distribution */
        internal Histogram Histogram {
            get { return histogram; }
        }

        /** Method:  Maximum difference allowed between two clusters */
        internal double MaxDiff {
            get { return sg.MaxDiff; }
            set { sg.MaxDiff = value; }
        }

        /** Method:  Maximum standard deviation allowed in a cluster (after a join) */
        internal double MaxStDev {
            get { return sg.MaxStDev; }
            set { sg.MaxStDev = value; }
        }

        /** Method:  Maximum standard deviation difference allowed in a cluster */
        internal double MaxStDevDiff {
            get { return sg.MaxStDevDiff; }
            set { sg.MaxStDevDiff = value; }
        }

        /** Method:  List of cluster means */
        internal List<double> Means {
            get { return sg.Means; }
        }

        /** Method:  List of cluster standard deviation */
        internal List<double> StDevs {
            get { return sg.StDevs; }
        }

        /** Method:  Array of cluster weights */
        internal double[] Weights {
            get { return sg.Weights; }
        }

        /** Method:  Start of forgetting period (from the present to the past) */
        internal int MaxWeighting {
            get { return maxWeighting; }
            set { maxWeighting = value; }
        }

        /** Method:   End of forgetting period (from the present to the past) */
        internal int MinWeighting {
            get { return minWeighting; }
            set { minWeighting = value; }
        }

        /** Method:  Forgetting proportion beyond the end of forgetting period  (from the present to the past) */
        internal double MinWeight {
            get { return sg.MinWeight; }
            set { sg.MinWeight = value; }
        }

        /** Method:  Level of determinism of the results (100 full deterministic, 0 full random) */
        internal double DeterminismLevel {
            get { return determinismLevel; }
            set { determinismLevel = value; }
        }

        /** Method:  Minimum weight allowed (if less than min, continuous intervals from the past are cleared) */
        internal double MinAggrWeight {
            get { return minAggrWeight; }
            set { minAggrWeight = value; }
        }

        /** Method:  Mean of stationary period (periods ahead "red point") */
        internal double StatPeriodMean {
            get { return sg.StatPeriodMean; }
        }

        /** Method:  Number of deleted periods */
        internal int DeletedPeriods {
            get { return deletedPeriods; }
        }

        /** Method:  Maximum number of samples */
        internal double MaxSamples {
            get { return maxSamples; }
            set { maxSamples = value; }
        }

        /** Method:  Percentage from all histogram columns to check stability of solution */
        internal double CheckFreqStability {
            get { return checkFreqStability; }
            set { checkFreqStability = value; }
        }

        /** Method:  Threshold of frequency variance for stability checking */
        internal double FreqVarThreshold {
            get { return freqVarThreshold; }
            set { freqVarThreshold = value; }
        }

        /** Method:  Final iterations needed for the calculation */
        internal int FinalIterations {
            get { return finalIterations; }
        }

        /** Method:  Number of values in stationary period */
        internal int StatPeriodCount {
            get { return statPeriodCount; }
        }

        /** Method:  load factor of grouped non-zero values on total grouped values */
        internal double LoadFactor {
            get { return sg.LoadFactor; }
        }

        /** Method:  noise level */
        internal double Noise {
            get { return noise; }
            set { noise = value; }
        }

        /** Method:  samples with their histograms */
        internal List<Sample> Samples {
            get { return this.samples; }
        }

        /** Method:  load factor threshold */
        internal double LoadFactorThreshold {
            get { return sg.LoadFactorThreshold; }
            set { sg.LoadFactorThreshold = value; }
        }

        /** Method:  if it is sparse */
        internal bool IsSparse {
            get { return sg.IsSparse; }
            set { sg.IsSparse = value; }
        }

        /** Method:  If seasonality algorithms have been applied */
        internal bool Seasonality {
            get { return seasonality; }
        }

        /** Method:  Minimum autocorrelation for seasonality */
        internal double MinAutoCorr {
            get { return minAutoCorr; }
            set { minAutoCorr = value; }
        }

        /** Method:  Minimum amplitude for seasonality */
        internal double MinAmplitude {
            get { return minAmplitude; }
            set { minAmplitude = value; }
        }

        /** Method:  Minimum lag for checking seasonality */
        internal int MinLag {
            get { return minLag; }
            set { minLag = value; }
        }

        /** Method:  Maximum lag for checking seasonality */
        internal int MaxLag {
            get { return maxLag; }
            set { maxLag = value; }
        }

        /** Method:  Maximum lag for checking seasonality */
        internal int Lag {
            get { return sg.Lag; }
        }

        /** Method:  If continuous grouping will be applied */
        internal bool MwAggregation {
            get { return mwAggregation; }
            set { mwAggregation = value; }
        }

        /** Method:  Exponent for non linear weighting of clusters (1-25) */
        internal double RootExpForClusterWeighting {
            get { return sg.RootExpForClusterWeighting; }
            set { sg.RootExpForClusterWeighting = value; }
        }

        /** Method:  First index of stationary period */
        internal int StatPeriodFirstIndex {
            get { return sg.StatPeriodFirstIndex; }
            set { sg.StatPeriodFirstIndex = value; }
        }

        /** Method:  Ratio mean/variance for sparse time series (threshold) */
        internal double RatioMvThreshold {
            get { return sg.RatioMvThreshold; }
            set { sg.RatioMvThreshold = value; }
        }

        /** Method:  Minimum grouped data for seasonality */
        internal int MinSeasonCount {
            get { return minSeasonCount; }
            set { minSeasonCount = value; }
        }

        #endregion

        #region Internal Methods

        /** Method:  Main calculation method  
        origSerie -  time series with original values 
        firstIndex -  index of first period 
        leadTime -  lead time */
        internal void Calculate(List<double> origSerie, int firstIndex, int leadTime, int nLeadTimes)
        {
            results.Clear();
            this.seasonality = false;
            sg.MaxWeighting = (int)(maxWeighting / leadTime);
            sg.MinWeighting = (int)(minWeighting / leadTime);
            sg.CalculateLoadFactor(origSerie);
            sg.IsSparse = false;
            sg.LoadData(origSerie, firstIndex, leadTime, nLeadTimes, sg.MinCount, this.mwAggregation);
            if (IsSparse) { results.Add(SignalCharact.ResultType.Sparse); return; }

            #region mini (obsolete)

            /*
            if (mini) {
                double[] weights = new double[sg.Serie.Count];
                for (int j = 0; j < sg.Serie.Count; j++) { weights[j] = 1.00; }
                func.ApplyForgettingFunction(weights, sg.MaxWeighting, sg.ForgetEndPeriods, sg.minWeigh);
                GenerateSamplesMini(sg.Serie, weights, determinismLevel);
                return;
            }
            else {
            */

            #endregion
            if (maxLag < 2 || sg.Serie.Count < minSeasonCount)  {
                this.seasonality = false;
            }
            else  {
                double cutoff = 0.05;
                this.seasonality = false;
                //sg.Lag = stat.CalcBestSeasonalLag(sg.Serie, cutoff, minAutoCorr, minAmplitude, minLag, maxLag);
                //if (sg.Lag < 2) { this.seasonality = false; }
                //else { this.seasonality = true; }
            }

            sg.Characterization(-1);
            if (Serie.Count <= 2 || Min == Max)
            {
                histogram = new Histogram();
                histogram.LoadDist(sg.FreqsDict, Min, Max);
                finalIterations = 1;
                return;
            }

            for (int i = 0; i < sg.Weights.Length; i++)
            {
                if (sg.Weights[i] > MinWeight) { deletedPeriods = i; break; }
            }
            GenerateSamples(deletedPeriods, sg.Weights, determinismLevel);
        }

        #endregion

        #region Private Methods

        #region Sampling

        private void GenerateSamples(int firstPeriod, double[] weights, double determinismLevel)   {
            if (Serie.Count <= 2) { throw new Exception("Count cannot be less than 2"); }

            samples.Clear();
            randGen.Reset();
            List<double> values;
            Sample sample;
            noiseStd = -noise * determinismLevel;

            //set biasMean and stDevMean
            List<double> clustInterval = new List<double>();
            double biasMean, biasStDev;
            if (sg.Clusters.Count == 0)  {
                biasMean = stat.Mean(Bias);
                biasStDev = stat.StDev(Bias);
                statPeriodCount = Serie.Count;
            }
            else   {
                statPeriodCount = sg.FirstClusterCount;
                foreach (HomogeneityClustering.StatCluster cluster in sg.Clusters) {
                    clustInterval.Clear();
                    for (int i = cluster.FirstIndex; i <= cluster.LastIndex; i++)
                    {
                        clustInterval.Add(Bias[i]);
                    }
                    cluster.BiasMean = stat.Mean(clustInterval);
                    cluster.BiasStDev = stat.StDev(clustInterval);
                }
            }

            sampleFromOriginalValues = true;

            //origSample
            values = GetSampleValues();
            if (deletedPeriods > 0)  {
                double[] weightsNew = new double[values.Count - deletedPeriods];
                for (int i = 0; i < weightsNew.Length; i++)
                {
                    weightsNew[i] = weights[deletedPeriods + i];
                }
                weights = weightsNew;
                values.RemoveRange(0, deletedPeriods);
            }
            //deterministicIterations = (int)(iterations * determinismLevel / 100.0);
            deterministicIterations = 100;

            histogram.Clear();
            for (int i = 0; i < deterministicIterations; i++)  {
                values = GetSampleValues();
                values.RemoveRange(0, deletedPeriods);
                origSample = new Sample(values, weights, false);
                histogram.LoadDist(origSample.Histogram.Freqs, origSample.Histogram.Min, origSample.Histogram.Max);
            }
            /*
            //basic iterations
            for (int i = 0; i < iterations - deterministicIterations; i++)
            {
                sample = new Sample(values, weights, true);
                samples.Add(sample);
                histogram.LoadDist(sample.Histogram.Freqs, sample.Histogram.Min, sample.Histogram.Max);
            }
            double[] freqsAnt = null;
            double[] freqsNew;
            if (maxSamples <= iterations)
            {
                finalIterations = iterations;
                return;
            }

            //additional iterations
            double mse;
            int it = iterations;
            bool stop = false;
            while (it <= maxSamples && !stop)
            {
                if (it % 10 == 0)
                {
                    freqsAnt = histogram.GetArrayNormFreqs();
                }
                sample = new Sample(values, weights, true);
                samples.Add(sample);
                histogram.LoadDist(sample.Histogram.Freqs, sample.Histogram.Min, sample.Histogram.Max);
                if (it % 10 == 0)
                {
                    stop = true;
                    freqsNew = histogram.GetArrayNormFreqs();
                    mse = stat.MSError(freqsNew, freqsAnt, checkFreqStability);
                    if (mse > freqVarThreshold) { stop = false; }
                }
                it++;
            }
            */
            histogram.Max = histogram.GetValues()[histogram.Count - 1] * 1.1;
            //finalIterations = it;
            finalIterations = iterations;
            count = histogram.Count;


        }

        private void GenerateSamplesMini(List<double> values, double[] weights, double determinismLevel)
        {
            if (Serie.Count <= 2)
            {
                return;
            }

            samples.Clear();
            randGen.Reset();
            Sample sample;
            double k = 0.15;
            noiseStd = -k * determinismLevel / 100.0 + k;

            //origSample
            deterministicIterations = (int)(iterations * determinismLevel / 100.0);
            histogram.Clear();
            for (int i = 0; i < deterministicIterations; i++)
            {
                origSample = new Sample(values, weights, false);
                histogram.LoadDist(origSample.Histogram.Freqs, origSample.Histogram.Min, origSample.Histogram.Max);
            }

            //basic iterations
            for (int i = 0; i < iterations - deterministicIterations; i++)
            {
                sample = new Sample(values, weights, true);
                samples.Add(sample);
                histogram.LoadDist(sample.Histogram.Freqs, sample.Histogram.Min, sample.Histogram.Max);
            }
            double[] freqsAnt = null;
            double[] freqsNew;
            if (maxSamples <= iterations)
            {
                finalIterations = iterations;
                return;
            }

            //additional iterations
            double mse;
            int it = iterations;
            bool stop = false;
            while (it <= maxSamples && !stop)
            {
                if (it % 10 == 0) { freqsAnt = histogram.GetArrayNormFreqs(); }
                sample = new Sample(values, weights, true);
                samples.Add(sample);
                histogram.LoadDist(sample.Histogram.Freqs, sample.Histogram.Min, sample.Histogram.Max);
                if (it % 10 == 0)
                {
                    stop = true;
                    freqsNew = histogram.GetArrayNormFreqs();
                    mse = stat.MSError(freqsNew, freqsAnt, checkFreqStability);
                    if (mse > freqVarThreshold) { stop = false; }
                }
                it++;
            }
            finalIterations = it;
            count = histogram.Count;
        }

        internal List<double> GetSampleValues()  {
            if (sampleFromOriginalValues) {
                List<double> origSample = new List<double>(Serie);
                return origSample; 
            }
            List<double> values = new List<double>();
            double value = 0;
            double bias, noise;
            if (Bias.Count <= 2) { return Serie; }

            if (sg.Clusters == null || sg.Clusters.Count == 0)   {
                for (int i = 0; i < Serie.Count; i++)
                {
                    bias = randGen.NextNormal(biasMean, biasStDev);
                    noise = bias * randGen.NextNormal() * noiseStd;
                    value = Regres[i] + bias + noise;
                    values.Add(value);
                }
            }
            else  {
                foreach (HomogeneityClustering.StatCluster cluster in sg.Clusters)  {
                    for (int i = cluster.FirstIndex; i <= cluster.LastIndex; i++)  {
                        bias = randGen.NextNormal(cluster.BiasMean, cluster.BiasStDev);
                        noise = bias * randGen.NextNormal() * noiseStd;
                        value = Regres[i] + bias + noise;
                        if (value < 0) { value = 0; }
                        values.Add(value);
                    }
                }
            }
            return values;
        }

        #endregion

        #region Histogram

        private void SetFrequencies()
        {

            //Set samples frequencies
            int maxCount = 0;
            foreach (Sample sample in this.samples)
            {
                if (sample.Frecuencias.Count > maxCount) { maxCount = sample.Frecuencias.Count; }
            }

            //Initiallize meanFrec
            meanFrec = new List<double>();
            for (int i = 0; i < maxCount; i++) { meanFrec.Add(0.00); }

            //Calculate mean fequencies
            foreach (Sample sample in this.samples)
            {
                for (int i = 0; i < sample.Valores.Count; i++)
                {
                    meanFrec[(int)sample.GetValor(i)] = (double)meanFrec[(int)sample.GetValor(i)] + sample.GetFrecuencia(i);
                }
            }

            count = 0;
            for (int i = 0; i < meanFrec.Count; i++)
            {
                meanFrec[i] = (double)meanFrec[i] / (double)this.samples.Count;
                if (meanFrec[i] != 0) { count++; }
            }
            meanFrec.Add(0.00);
        }

        #endregion

        #endregion

    }
}
