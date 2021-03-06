#region Imports

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Statistics;
using Maths;

#endregion

namespace MonteCarlo {
    
    /** Method: Class for signal characterization: trend removal, noise filtering, outlier filtering, stationarity, statistics  */
    internal class SignalCharact {

        #region Fields

        //calculation classes
        private Wavelets wav;
        private Polynom poly;
        private StatFunctions stat;
        private Functions func;
        private NormalDistrib ns;
        private Differential drv;
        private HomogeneityClustering hc;
        private List<HomogeneityClustering.StatCluster> clusters;
        private HomogeneityClustering.StatCluster firstCluster;
        private HypoTest hypoTest;
        private double[] weights;
        
        //parameters
        private double varCoeffThreshold;
        private bool filterOutliers;
        private int minFreqForOutlierFiltering;
        private int minCount;
        private double outlierThreshold;
        private double rawOutlierThres;
        private double maxNorm;
        private double minReg;
        private double maxReg;
        private int maxWeighting;
        private int minWeighting;
        private double minWeight;
        private double minLeadTimeObsProportion;
        private double pValue = 0.05;
        private double fc = 0;
        private int minSpan;
        private double loadFactorThreshold;

        //collections
        private List<double> origSerie;
        private List<double> filteredSerie;
        private List<double> serie;
        //private List<double> serieDrv1;
        private List<double> serieDrv2;
        private List<double> regres;
        private List<double> bias;

        private List<double> normSerie;
        private List<double> normRegres;
        
        private List<double> means;
        private List<double> stDevs;

        private SortedDictionary<int, double> freqsDict;
     
        //results
        private double min;
        private double max;
        private double minOutlierFiltered;
        private double maxValueNotFiltered;
        private List<int> outlierIndexes;
        private int firstIndexToFcst;
        private List<ResultType> results;
        private double statPeriodMean;
        private double minNonZeroFreq;
        private double significance;
        private HomogeneityClustering.TestType testType;

        private double minFc;
        private bool isSparse;
        private int lag;
        private bool continuous;
        private int span;

        private double minWall;
        private int nSpans;

        private double rootExpForClusterWeighting;
        private double ratioMvThreshold;
        
        #endregion

        #region Constructor

        /** Method:  Constructor  
        minCount -  minimum quantity of values 
        varCoeffThreshold -  variance coefficent threshold 
        filterOutliers -  if outliers should be filtered 
        minFreqForOutlierFiltering -  minimum frequency to allow outlier filtering 
        outlierThreshold -  Threshold for oultier filtering 
        maxNorm -  Maximum normalized value (i.e. 100) 
        maxDiff -  Maximum mean difference allowed between two clusters 
        maxStDev -  Maximum standard deviation allowed in a cluster (after join) 
        pValue -  pValue por hypothesis tests  */
        internal SignalCharact(int minCount, double varCoeffThreshold, bool filterOutliers, int minFreqForOutlierFiltering, double outlierThreshold, double dailyOutlierThreshold, double maxNorm, double maxDiff, double maxStDev, double pValue) {

            this.minCount = minCount;
            this.varCoeffThreshold = varCoeffThreshold;
            this.filterOutliers = filterOutliers;
            this.minFreqForOutlierFiltering = minFreqForOutlierFiltering;
            this.outlierThreshold = outlierThreshold;
            this.rawOutlierThres = dailyOutlierThreshold;
            this.maxNorm = maxNorm;

            this.freqsDict = new SortedDictionary<int, double>();
            this.results = new List<ResultType>();

            this.poly = new Polynom();
            this.stat = new StatFunctions();
            this.func = new Functions();
            this.wav = new Wavelets();
            this.ns = new NormalDistrib();
            this.drv = new Differential();
            this.hypoTest = new HypoTest(0.05, 5);
            this.testType = HomogeneityClustering.TestType.Statistics;
            //this.testType = HomogeneityClustering.TestType.Wilcoxon;
            this.hc = new HomogeneityClustering(maxDiff, maxStDev, minCount, pValue, testType);
            this.statPeriodMean = -1.0;
            this.clusters = new List<HomogeneityClustering.StatCluster>();
            this.minNonZeroFreq = double.MaxValue;
            this.pValue = pValue;
            this.minSpan = 7;
            this.minFc = 0.8;
            this.minCount = 5;
            this.lag = -1;
            this.span = 1;
            this.minWall = 2;
            this.rootExpForClusterWeighting = 1;
            this.ratioMvThreshold = 0.2;
        }

        #endregion

        #region Properties

        /** Method:  Minimum quantity of values allowed  */
        internal int MinCount { 
            get { return minCount; }
            set { 
                minCount = value;
                hc.MinCount = value;
            }
        }
      
        /** Method:  load factor of grouped non-zero values on total grouped values  */
        internal double LoadFactor {
            get { return wav.LoadFactor; }
        }

        /** Method:  Minimum value  */
        internal double Min { 
            get { return min; } 
        }

        /** Method:  Maximum value  */
        internal double Max { 
            get { return max; } 
        }
        
        /** Method:  Range of values  */
        internal double Range {
            get { return max-min; } 
        }

        /** Method:  Min value of a filtered outlier  */
        internal double MinOutlierFiltered { 
            get { return minOutlierFiltered; } 
        }

        /** Method:  Max value of a non-filtered value  */
        internal double MaxValueNotFiltered { 
            get { return maxValueNotFiltered; } 
        }

        /** Method:  Filtered time series  */
        internal List<double> FilteredSerie { 
            get { return filteredSerie; } 
        }

        ///** Method:  First derivative time series  */
        //internal List<double> SerieDrv1 { 
            //get { return serieDrv1; } 
        //}

        /** Method:  Second derivative time series  */
        internal List<double> SerieDrv2 { 
            get { return serieDrv2; } 
        }

        /** Method:  Index of first period to forecast  */
        internal int FirstIndexToFcst { 
            get { return firstIndexToFcst; }
        }

        /** Method:  Original time series  */
        internal List<double> OrigSerie { 
            get { return origSerie; }
            set { origSerie = value; }
        }
        
        /** Method:  Current time series  */
        internal List<double> Serie { 
            get { return serie; }
            set { serie = value; }
        }

        /** Method:  Regression time series  */
        internal List<double> Regres { 
            get { return regres; } 
        }

        /** Method:  Bias time series  */
        internal List<double> Bias { 
            get { return bias; } 
        }

        /** Method:  Indexes of filtered outliers  */
        internal List<int> OutlierIndexes {
            get { return outlierIndexes; } 
        }

        /** Method:  List of results (warinings or errors)  */
        internal List<ResultType> Results { 
            get { return results; } 
        }

        /** Method:  Data frequencies in a dictionary  */
        internal SortedDictionary<int, double> FreqsDict {
            get { return freqsDict; } 
        }

        /** Method:  Threshold for outlier filtering  */
        internal double OutlierThreshold { 
            get { return outlierThreshold; } 
            set {outlierThreshold = value; }
        }

        /** Method:  Threshold for daily outlier filtering  */
        internal double RawOutlierThres {
            get { return rawOutlierThres; }
            set { rawOutlierThres = value; }
        }
        
        /** Method:  Maximum mean difference allowed between two clusters  */
        internal double MaxDiff {
            get { return hc.MaxDiff; }
            set { hc.MaxDiff = value; }
        }

        /** Method:  Maximum standard deviation difference allowed between two clusters  */
        internal double MaxStDevDiff {
            get { return hc.MaxStDevDiff; }
            set { hc.MaxStDevDiff = value; }
        }

        /** Method:  Maximum difference allowed within a cluster (after join)  */
        internal double MaxStDev {
            get { return hc.MaxStDev; }
            set { hc.MaxStDev = value; }
        }

        /** Method:  List of cluster means  */
        internal List<double> Means { 
            get { return means; } 
        }

        /** Method:  List of cluster standard deviations  */
        internal List<double> StDevs {
            get { return stDevs; } 
        }

        /** Method:  Array of weights for clusters  */
        internal double[] Weights { 
            get { return weights; }
            set { weights = value; }
        }

        /** Method:  Mean of stationary period  */
        internal double StatPeriodMean { 
            get { return statPeriodMean; } 
        }

        /** Method:  Minimum leadtime observations in proportion with total observations  */
        internal double MinLeadTimeObsProportion {
            get { return minLeadTimeObsProportion; }
            set { minLeadTimeObsProportion = value; }
        }
        
        /** Method:  List of clusters  */
        internal List<HomogeneityClustering.StatCluster> Clusters {
            get { return clusters; } 
        }

        /** Method:  Start of forgetting period (from the present to the past)  */
        internal int MaxWeighting
        {
            get { return maxWeighting; }
            set { maxWeighting = value; }
        }

        /** Method:  End of forgetting period (from the present to the past)   */
        internal int MinWeighting
        {
            get { return minWeighting; }
            set { minWeighting = value; }
        }

        /** Method:  Forgetting proportion beyond end of stationary period (from the present to the past)  */
        internal double MinWeight {
            get { return minWeight; }
            set { minWeight = value; }
        }

        /** Method:  Number of elements of first cluster  */
        internal int FirstClusterCount { 
            get { return firstCluster.Count; } 
        }

        /** Method:  Type of statistical test for joins  */
        internal HomogeneityClustering.TestType Test {
            get { return testType; }
            set {
                testType = value;
                hc.Test = value;
            }
        }

        /** Method:  Minimum Load Factor  */
        internal double MinFc {
            get { return minFc; }
            set { minFc = value; }
        }

        /** Method:  Cut-off for trimming  */
        internal double CutOff {
            get { return hc.CutOff; }
            set { hc.CutOff = value; }
        }

        /** Method:  load factor threshold  */
        internal double LoadFactorThreshold {
            get { return loadFactorThreshold; }
            set { loadFactorThreshold = value; }
        }

        /** Method:  if it is sparse  */
        internal bool IsSparse {
            get { return isSparse; }
            set { isSparse = value; }
        }

        /** Method:  lag for seasonality  */
        internal int Lag {
            get { return lag; }
            set { lag = value; }
        }

        /** Method:  Exponent for non linear weighting of clusters (1-25)  */
        internal double RootExpForClusterWeighting {
            get { return rootExpForClusterWeighting; }
            set { rootExpForClusterWeighting = value; }
        }

        /** Method:  First index of stationary period  */
        internal int StatPeriodFirstIndex {
            get { return firstIndexToFcst; }
            set { firstIndexToFcst = value; }
        }

        /** Method:  Ratio mean/variance for sparse time series (threshold)  */
        internal double RatioMvThreshold {
            get { return ratioMvThreshold; }
            set { ratioMvThreshold = value; }
        }
        
        #endregion

        #region Internal Methods

        /** Method:  Main calculation method  
        minCount -  minimum number of cases */
        internal void Characterization(int minCount) {
            SetRegressionAndBias();
            FilterOutliers();
            NormalizeCollections();
            CalculateWeights();
            HomogeneityClusteringTest(firstIndexToFcst);
            SetClusterWeights();
        }

        /** Method:  Calculate load factor for sparse calculations
        origSerie -  original serie (not grouped) */
        internal void CalculateLoadFactor(List<double> origSerie) {
            int nonZerosCount = 0;
            foreach (double val in origSerie) { if (val > 0) { nonZerosCount++; } }
            fc = (double)nonZerosCount / (double)origSerie.Count;
        }
        
        /** Method:  Calculate index of first homogeneous period  */
        internal int CalculateHomogeneousInitialIndex(double alpha, double highAlpha) {
            int maxLag = 12;
            int initialIndex = 0;
            if(clusters.Count < 2) { return initialIndex; }

            double pValue1 = -1;
            double pValue2 = -1;
            for(int i=clusters.Count-1;i>0;i--) {
                initialIndex = clusters[i].FirstIndex;
                if(clusters[i-1].Count > 1 && clusters[i].Count > 1 && !(hc.AreHomogeneous(this.origSerie, clusters[i].FirstIndex, ref pValue1) || hc.AreHomogeneous(origSerie, clusters[i-1].FirstIndex, clusters[i-1].LastIndex, clusters[i].FirstIndex, clusters[i].LastIndex, ref pValue2)) && origSerie.Count - initialIndex >= minCount) { break; }
            }
            if (initialIndex == 0 || pValue1 < highAlpha || pValue2 < highAlpha) { return initialIndex; }

            List<double> acf = stat.ACF(serie, maxLag);
            int p = hypoTest.TestMovingAverage(acf, alpha);
            if (p != -1) { return 0; }            
            
            return initialIndex;
        }

        internal void JoinSmallClusters(int minSize, double pValueThreshold) {  
            for(int i=0;i<clusters.Count;i++) {
                if(clusters[i].Count > minSize) { continue; }
                double pVal1 = 0;
                double pVal2 = 0;
                if(i>0) { pVal1 = hc.TestHomogeneity(clusters[i-1], clusters[i]); }
                if(i<clusters.Count-1) { pVal2 =  hc.TestHomogeneity(clusters[i], clusters[i+1]); }
                if(pVal1 >= pValueThreshold && pVal1 > pVal2 ){ clusters[i].Join(clusters[i-1]); }
                else if(pVal2 >= pValueThreshold && pVal2 > pVal1 ){ clusters[i].Join(clusters[i+1]); }
            }
        }

        internal double TestHomogeneity(HomogeneityClustering.StatCluster sc1, HomogeneityClustering.StatCluster sc2) {
            return hc.TestHomogeneity(sc1, sc2);
        }

        #endregion

        #region Private Methods

        #region Load Data

        internal void LoadData(List<double> origSerie, int firstIndex, int span, int nSpans, int minCount, bool continuous) {
            this.continuous = continuous;
            this.nSpans = nSpans;
            this.span =+ span;
            if (minCount != -1) { this.minCount = minCount; }
            if (minCount < 3) { minCount = (int)Math.Min(origSerie.Count, 3); }
            if (continuous) {
                maxWeighting = maxWeighting * span;
                minWeighting = minWeighting * span;
                this.minCount = minCount * span;
                this.minWall = minWall * span; 
            }
            this.results.Clear();
            this.origSerie = origSerie;
            FilterDailyOutliers(origSerie, firstIndex);

            clusters.Clear();
            wav.Group(this.origSerie, 0, span, continuous); 
            this.serie = wav.Data;

            //is sparse? 
            //double lf = GetLoadFactor(serie);
            double lf = GetWeightedLoadFactor(serie);
            if (this.LoadFactorThreshold == 0 || (lf > 0.05 && (lf >= this.LoadFactorThreshold || clusters.Count > 1))) { isSparse = false; } 
            else {
                double m = stat.Mean(serie);
                double v = stat.Variance(serie);
                if (v > 0 && m / v > ratioMvThreshold) { isSparse = false; }
                else { isSparse = true; }
            }
            SetMinMaxAndFreqs(serie);

            if (isSparse) { return; }
            
            int maxSpan = (int)((double)origSerie.Count / 4.00);  //max span to warrant 4 grouped data
            if (span > maxSpan && maxSpan >= 7) { span = maxSpan; }
        }

        private List<double> Group(List<double> origSerie, int span, int firstIndex) {
            List<double> serie = new List<double>();
            double groupedValue = 0;

            for (int i = firstIndex; i < origSerie.Count; i++) {
                groupedValue += origSerie[i];
                if ((i + 1) % span == 0) {
                    serie.Add(groupedValue);
                    groupedValue = 0;
                }
            }
            if (origSerie.Count % span != 0) { serie.Add(groupedValue); }
            return serie;
        }

        #endregion

        #region Regression and Bias

        /** Method:  Set wavelet regression and calculate bias  */
        internal void SetRegressionAndBias() {
            if (freqsDict.Count == 1 || serie.Count <= 2) { 
                return; 
            }
            else if(serie.Count <= 4) {
                regres = new List<double>();
                regres.AddRange(serie);
            }
            else {
                wav.CalcFreqDescomposition();
                regres = wav.GetFreqDescomposition(wav.Coeffs.Count-2);
            }
            bias = func.Substract(serie, regres, false, false);
        }

        #endregion

        #region Outlier Filtering

        #region Grouped filtering

        private void FilterOutliers() {
            if(serie.Count <= 2 || !filterOutliers || max-min <= 0 || freqsDict.Values.Count < minFreqForOutlierFiltering) {
                maxValueNotFiltered = max;
                this.filteredSerie = serie;
                return; 
            }

            outlierIndexes = new List<int>();
            double meanBias = stat.Mean(bias);
            double stDevBias = stat.StDev(bias);
            if(stDevBias == 0) { return; }
            double maxValue, p;
            int maxIndex = -1;
            bool[] usedIndexes = new bool[serie.Count];
            minOutlierFiltered = double.MaxValue;
            maxValueNotFiltered = double.MinValue;

            while(outlierIndexes.Count < serie.Count) {
                maxValue = double.MinValue;
                for(int i=0;i<serie.Count;i++) {
                    if(!usedIndexes[i] && serie[i] > maxValue) { maxValue = serie[i]; maxIndex = i; }
                }
                usedIndexes[maxIndex] = true;

                p = ns.pNorm(bias[maxIndex], meanBias, stDevBias);

                if(serie[maxIndex] > minNonZeroFreq &&  p > outlierThreshold) {
                    if(maxValue < minOutlierFiltered) { minOutlierFiltered = maxValue; }
                    outlierIndexes.Add(maxIndex);
                } 
                else {
                    maxValueNotFiltered = maxValue;
                    outlierIndexes.Sort();
                    break;
                }
            }

            if(outlierIndexes.Count > 0) {
                results.Add(ResultType.OutliersFiltered);
                for(int i=0;i<serie.Count;i++) {
                    if(regres[i] > maxValueNotFiltered) {
                        regres[i] = maxValueNotFiltered;
                        bias[i] = 0.0;
                    }
                    if(serie[i] > maxValueNotFiltered) {
                        serie[i] = regres[i];
                        bias[i] = 0.0;
                    }
                }
                max = maxValueNotFiltered;
            } 
            else {
                maxValueNotFiltered = max;
            }
            this.filteredSerie = serie;
  }

        #endregion

        #region Raw filtering

        private void FilterDailyOutliers(List<double> origSerie, int firstIndex) {
            int minDailyCount = (int)(1 / (1 - rawOutlierThres));
            double maxFc = 0.5;
            if (origSerie.Count > minDailyCount && fc > (1 - rawOutlierThres)) {
                if (fc > maxFc) { this.origSerie = FilterDailyOutliersAll(origSerie, firstIndex); }
                else {this.origSerie = FilterDailyOutliersZerosAndNonZeros(origSerie, firstIndex); }
            }
            else { 
                if(firstIndex > 0) {
                    this.origSerie = new List<double>();
                    for (int i = firstIndex; i < origSerie.Count;i++ ) {
                        this.origSerie.Add(origSerie[i]);
                    }
                }
                else {
                    this.origSerie = origSerie; 
                }
            }
        }
        
        private List<double> FilterDailyOutliersAll(List<double> dailySerie, int firstIndex) {
            if(dailySerie.Count < minCount) { return dailySerie; }
            double alpha = 1 - this.rawOutlierThres;
            double mean = stat.TrimMeanNonZero(dailySerie, alpha);
            OutlierDetection outlierDetection = new OutlierDetection();
            List<double> data = new List<double>();
            List<double> model = new List<double>();
            for(int i=firstIndex;i<dailySerie.Count;i++) {
                data.Add(dailySerie[i]);
                model.Add(mean); 
            }
            double lim = outlierDetection.StudDelResLimit(dailySerie, model, this.rawOutlierThres);
            List<double> filteredSerie = new List<double>();
            //for(int i=0;i<firstIndex;i++) { filteredSerie.Add(dailySerie[i]); }
            List<double> studDelRes = outlierDetection.StudDelRes(data, model);
            for(int i=firstIndex;i<dailySerie.Count;i++) {
                if(studDelRes[i-firstIndex] < lim || dailySerie[i] <= mean) { filteredSerie.Add(dailySerie[i]); }
            }
            return filteredSerie;
        }

        private List<double> FilterDailyOutliersZerosAndNonZeros(List<double> dailySerie, int firstIndex) {
            if(dailySerie.Count < minCount) { return dailySerie; }
            double alpha = 1 - this.rawOutlierThres;
            double mean = stat.TrimMeanNonZero(dailySerie, alpha);
            OutlierDetection outlierDetection = new OutlierDetection();
            List<double> data = new List<double>();
            List<double> dataNonZeros = new List<double>();
            List<double> model = new List<double>();
            List<double> modelNonZeros = new List<double>();
            for(int i=firstIndex;i<dailySerie.Count;i++) {
                data.Add(dailySerie[i]);
                if(dailySerie[i] > 0) { 
                    dataNonZeros.Add(dailySerie[i]);
                    modelNonZeros.Add(mean);
                }
                model.Add(mean);
            }
            double lim = outlierDetection.StudDelResLimit(dailySerie, model, this.rawOutlierThres);
            double limNonZeros = outlierDetection.StudDelResLimit(dataNonZeros, modelNonZeros, this.rawOutlierThres);
            List<double> filteredSerie = new List<double>();
            //for(int i=0;i<firstIndex;i++) { filteredSerie.Add(dailySerie[i]); }
            List<double> studDelRes = outlierDetection.StudDelRes(data, model);
            for(int i=firstIndex;i<dailySerie.Count;i++) {
                if(studDelRes[i-firstIndex] < lim || dailySerie[i] <= mean) { filteredSerie.Add(dailySerie[i]); }
                if(studDelRes[i-firstIndex] >= lim && studDelRes[i-firstIndex] < limNonZeros) { filteredSerie.Add(dailySerie[i]); }
            }
            return filteredSerie;
        }

        #endregion

        #endregion

        #region Normalization

        private void NormalizeCollections() {
            if(freqsDict.Count == 1 || serie.Count <= 2) {
                normRegres = serie;
                return; 
            }
            //serie
            double range = max - min;
            normSerie = new List<double>();
            if(range == 0) { normSerie = serie; } 
            else {
                for(int i=0;i<serie.Count;i++) { normSerie.Add(((serie[i] - min)/range) * maxNorm); }
            }
            
            //regres
            minReg = double.MaxValue;
            maxReg = double.MinValue;
            foreach(double value in serie) {
                if(value < minReg) { minReg = value; }
                if(value > maxReg) { maxReg = value; }
            }

            double rangeReg = maxReg - minReg;
            if(range == 0) { 
                normRegres = regres;
            } 
            else {
                normRegres = new List<double>();
                for(int i=0;i<regres.Count;i++) { normRegres.Add(Normalize(regres[i], minReg, maxReg, maxNorm)); }
            }
        }

        private double Normalize(double val, double min, double max, double maxNorm) { 
            return (val - min)/(max-min) * maxNorm; 
        }

        private double UnNormalize(double val, double min, double max, double maxNorm) {
            return (val/maxNorm) *(max-min) + min;
        }

        
        #endregion

        #region Stationarity Methods
      
        #region Stationarity Clustering Test

        private void HomogeneityClusteringTest(int statPeriodIndex) {
            if (serie.Count <= minCount || statPeriodIndex >= normRegres.Count - 1) {
                HomogeneityClustering.StatCluster cluster = new HomogeneityClustering.StatCluster();
                SetMinMaxAndFreqs(serie);
                return;
            }
            filteredSerie = new List<double>();
            filteredSerie.AddRange(serie);
            if (statPeriodIndex > minCount) {
                clusters = hc.Clustering(normRegres, 0, statPeriodIndex-1);
                List<HomogeneityClustering.StatCluster> newClusters = hc.Clustering(normRegres, statPeriodIndex, serie.Count - 1);
                clusters.AddRange(newClusters);
            } 
            else {
                clusters = hc.Clustering(normRegres, 0, serie.Count-1);
            }
            means = hc.GetMeans(clusters);
            stDevs = hc.GetStDevs(clusters);

            int i = clusters.Count-1;
            firstCluster = new HomogeneityClustering.StatCluster(clusters[i]);
            
            List<HomogeneityClustering.StatCluster> walls = new List<HomogeneityClustering.StatCluster>();

            if(testType == HomogeneityClustering.TestType.Statistics) {
                while(serie.Count - firstCluster.FirstIndex < minCount && i >= 0) { firstCluster.Join(clusters[--i]); }
                while(i>0) {

                    if(clusters[i-1].Mean > firstCluster.Mean && clusters[i-1].Count <= minWall) {
                        walls.Add(clusters[i-1]);
                        i--;
                        continue;
                    }

                    else if(clusters[i-1].Mean <= firstCluster.Mean && clusters[i-1].Count <= minCount) {
                        if(firstCluster.Mean - clusters[i-1].Mean <= hc.OrigMaxDiff) {
                            firstCluster.Join(clusters[i-1]);
                        }
                        i--;
                        continue;

                    }
                    else {
                        double maxThresholdForAdvance = hc.OrigMaxDiff;

                        if(Math.Abs(clusters[i-1].Mean - firstCluster.Mean) < maxThresholdForAdvance * weights[firstCluster.FirstIndex] && 
                                   (firstCluster.Mean == 0 || clusters[i-1].Mean / firstCluster.Mean < 2)) {
                            firstCluster.Join(clusters[i-1]);
                            i--;
                        }
                        else {
                            break;
                        }
                    }
                }
            }
            else {
                while(i > 0 && (serie.Count - firstCluster.FirstIndex < minCount || hc.AreHomogeneous(clusters[i-1], firstCluster))) {
                    if(clusters[i-1].Mean > firstCluster.Mean && clusters[i-1].Count <= 2) { walls.Add(clusters[i-1]); }
                    firstCluster.Join(clusters[i-1]);
                    i--;
                }
            }

            if (firstCluster.FirstIndex > firstIndexToFcst) { 
                firstIndexToFcst = firstCluster.FirstIndex; 

            }
            if(serie.Count - firstIndexToFcst < minCount) { firstIndexToFcst = serie.Count - minCount - 1; }

            //filtrado walls
            outlierThreshold = 1; //TODO: revisar
            if(outlierIndexes == null) { outlierIndexes = new List<int>(); }
            List<double> trunkMeans = new List<double>();
            for(int j=firstIndexToFcst;j<means.Count;j++) { trunkMeans.Add(means[j]); }
            double meanTrunk = stat.Mean(trunkMeans);
            double stDevTrunk = stat.StDev(trunkMeans);
            if(stDevTrunk == 0) { return; }
            double p;
            bool clustFiltered = false;
            for(int j=0;j<walls.Count;j++) {
                p = ns.pNorm(walls[j].Mean, meanTrunk, stDevTrunk);
                if(p > outlierThreshold) {
                    if(walls[j].Mean < minOutlierFiltered) { 
                        minOutlierFiltered = walls[j].Mean;
                        clustFiltered = true;
                    }
                    for(int k=walls[j].FirstIndex;k<=walls[j].LastIndex;k++) { 
                        outlierIndexes.Add(k);
                        double fixedValue =  UnNormalize(firstCluster.Mean, minReg, maxReg, maxNorm);
                        serie[k] = fixedValue;
                        regres[k] = fixedValue;
                        bias[k] = 0;
                        means[k] = fixedValue;
                    }
                } 
            }
            if(outlierIndexes.Count > 1) {
                outlierIndexes.Sort();
            }
            if(clustFiltered) { results.Add(ResultType.ClusteringFiltered); }
        
            //serie.RemoveRange(0, firstIndexToFcst);
            //regres.RemoveRange(0, firstIndexToFcst);
            //bias.RemoveRange(0, firstIndexToFcst);
            SetMinMaxAndFreqs(serie);
            statPeriodMean = UnNormalize(firstCluster.Mean, min, max, maxNorm);
        }

        private void SetMinMaxAndFreqs(List<double> serie) {
            min = double.MaxValue;
            max = double.MinValue;
            freqsDict.Clear();
            foreach(double value in serie) {
                if(value < min) { min = value; }
                if(value > max) { max = value; }
                AddFreq(value);
            }
        }
        
        #endregion 

        #region Weighting Clustering 

        private void SetClusterWeights() {
            if(serie.Count < minCount || clusters == null || clusters.Count == 0) { return; }
            double minPVal = double.MaxValue;
            int minPValIndex = -1;
            int minPValIndex2 = -1;
            double pVal;
            for(int i=0;i<clusters.Count;i++) {
                if( pValue > 0) {
                    pVal = hypoTest.MannWhitney(serie, clusters[i].FirstIndex, true);
                    if(pVal < minPVal) {
                        minPVal = pVal;
                        minPValIndex = i;
                    }
                    if(pVal == minPVal) {
                        minPValIndex2 = i;
                    }
                }
                if(clusters[i].FirstIndex >= firstIndexToFcst) { clusters[i].Weight = 1.0; }
                else { clusters[i].Weight = CalcWeigth(clusters[i]);  }

                for (int j = clusters[i].FirstIndex; j <= clusters[i].LastIndex; j++) { weights[j] *= clusters[i].Weight; }
            }
        }

        private double CalcWeigth(HomogeneityClustering.StatCluster sc) {
            //normal intersection : 
            //double w1 = firstCluster.Count;
            //double w2 = sc.Count;
            double w1 = ((double)firstCluster.Count/(double)(firstCluster.Count + sc.Count)) * maxNorm;
            double w2 = ((double)sc.Count / (double)(firstCluster.Count + sc.Count)) * maxNorm; 
            double m1 = firstCluster.Mean;
            double m2 = sc.Mean;
            double s1 = firstCluster.StDev;
            double s2 = sc.StDev;

            double weight = ns.GetNormalIntersection(m1, s1, w1, m2, s2, w2, true);
            //double weight = hypoTest.MannWhitney(firstCluster.Values, sc.Values, true);
            
            if (rootExpForClusterWeighting > 1) { weight = Math.Pow(weight, (1.0 / rootExpForClusterWeighting)); }
            return weight;
        }

        #endregion

        #endregion

        #region Weighting Methods

        private void CalculateWeights() {
            if (lag < nSpans * 2) { weights = func.CalcTempWeights(serie.Count, maxWeighting, minWeighting, minWeight);  } 
            else { weights = CalculateSeasonalWeights(serie, lag);  }
        }

        private double[] CalculateSeasonalWeights(List<double> serie, int lag) {
            double[] wghV = func.CalcTempWeights(lag-nSpans, 0, (lag-nSpans)/2, minWeight);
            for (int i = 0; i < lag / 2; i++) {
                wghV[i] = wghV[wghV.Length - i-1];
            }

            double[] wgh1 = new double[nSpans]; 
            for (int i = 0; i < nSpans; i++) {
                wgh1[i] = 1.00;
            }
          
            List<double> weightList = new List<double>();
            while (weightList.Count < serie.Count) {
                weightList.AddRange(wgh1);
                weightList.AddRange(wghV);
            }
            while (weightList.Count > serie.Count) { weightList.RemoveAt(0); }
            weights = weightList.ToArray();
            return weights;
        }
  
        #endregion

        #region Auxiliar Methods

        private double GetLoadFactor(List<double> serie) {
            int tot = 0;
            foreach (double val in serie) {
                if (val > 0) { tot++; }
            }

            double lfactor = (double)tot / ((double)serie.Count);
            return lfactor;
        }

        private double GetWeightedLoadFactor(List<double> serie) {
            int forgetInitIndex = serie.Count - Math.Min(serie.Count, maxWeighting);
            int forgetEndIndex = serie.Count - Math.Min(serie.Count, minWeighting);
            
            double n = (double)serie.Count;
            
            double w1 = (serie.Count - forgetInitIndex) * 1;
            double w2 = (forgetInitIndex - forgetEndIndex) * ((1- minWeight)/2);
            double w3 = forgetEndIndex * minWeight;

            double totW = w1 + w2 + w3;
            w1 /= totW;
            w2 /= totW;
            w3 /= totW;
            double lf1  = 0;
            double lf2 = 0;
            double lf3 = 0;
            double tot1 = 0;
            for(int i=forgetInitIndex;i<serie.Count;i++) {
                if (serie[i] > 0) { tot1++; }
            }
            if (serie.Count - forgetInitIndex > 0) { lf1 = tot1 / (double)(serie.Count - forgetInitIndex); }
            
            double tot2 = 0;
            for (int i = forgetEndIndex; i < forgetInitIndex; i++) {
                if (serie[i] > 0) { tot2++; }
            }
            if (forgetInitIndex - forgetEndIndex > 0) { lf2 = tot2 / (double)(forgetInitIndex - forgetEndIndex); }
            
            double tot3 = 0;
            for (int i = 0; i < forgetEndIndex; i++) {
                if (serie[i] > 0) { tot3++; }
            }
            if (forgetEndIndex > 0) { lf3 = tot3 / (double)(forgetEndIndex); }
            
            
             double lfactor = lf1 * w1 + lf2 * w2 + lf3 * w3;
             return lfactor;
        }
        
        private List<double> GetResiduals(List<double> serie) {
            Result res = poly.LSRegression(serie);
            List<double> coeffs = new List<double>();
            coeffs.Add(res.c0);
            coeffs.Add(res.c1);
            List<double> x = new List<double>();
            for(int i=0;i<serie.Count;i++) { x.Add((double)i); }
            List<double> lsReg = poly.GetRegValues(x, coeffs);
            List<double> resids = func.Substract(serie, lsReg, true, false);
            return resids;
        }

        private void AddFreq(double val) {
            if(val > 0 && val < minNonZeroFreq) { minNonZeroFreq = val; } 
            int key = (int)Math.Ceiling(val);
            if(!freqsDict.ContainsKey(key)) { freqsDict.Add(key, 1.0); } 
            else { freqsDict[key] = freqsDict[key] + 1.0; }
        }

        private void SubstractFreq(double val) {
            int key = (int)Math.Ceiling(val);
            if(freqsDict.ContainsKey(key)) { freqsDict[key] = freqsDict[key] - 1.0; }
            if(freqsDict[key] == 0) { freqsDict.Remove(key); }
        }

        private List<int> GetFiniteJumpCandidates(List<double> serie) {
            return poly.ZerosNegPosIndexes(serieDrv2);
        }

        private void AddInitialValues(List<double> serie, List<double> initialValues, int ini, int fin) {
            for(int i=fin;i>=ini;i--) { serie.Insert(0, initialValues[i]); }
        }

        #endregion

        #endregion

        #region Enums

        /** Method:  Result of calculation  */
        internal enum ResultType {

            /** Method:  Some outliers have been filtered (at least one)  */
            OutliersFiltered,

            /** Method:  Total periods is below the minimum allowed  */
            LessThanMinPeriods,

            /** Method:  The time series is stationary  */
            Stationary,

            /** Method:  Some periods have been filtered by clustering processes  */
            ClusteringFiltered,

            /** Method:  Sparse and only one cluster as a result of clustering process  */
            Sparse,

            /** Method:  Treated as sparse  */
            AsSparse
        };

        #endregion

    }
}
