#region Imports

using System;
using System.Collections.Generic;
using System.Text;
using Maths;

#endregion

namespace Statistics {

    /** Method:  Algorithmic repository class for stationarity time series clustering  */
    internal class HomogeneityClustering {

        #region Fields

        private double origMaxDiff;
        private double origMaxStDev;
        private double maxDiff;
        private double maxStDev;
        private double maxStDevDiff;
        private int minCount;
        private HypoTest hypoTest;
        private double significance;
        private TestType testType;
        private int minTestCount;

        #endregion

        #region Constructor

        /** Method: Constructor 
        maxDiff -  maximum difference allowed 
        maxStDev -  maximum standard deviation allowed 
        minCount -  minimum number of elements allowed 
        significance -  significance of homogeneity hypothesis test 
        testType -  type of hypothesis test */
        internal HomogeneityClustering(double maxDiff, double maxStDev, int minCount, double significance, TestType testType)
        {
            this.maxDiff = maxDiff;
            this.maxStDev = maxStDev;
            this.origMaxDiff = maxDiff;
            this.origMaxStDev = maxStDev;
            this.minCount = minCount;
            this.minTestCount = 5;
            this.hypoTest = new HypoTest(significance, minTestCount);
            this.significance = significance;
            this.testType = testType;
        }

        #endregion

        #region Properties

        /** Method:  maximum difference allowed within a cluster  */
        internal double MaxDiff
        {
            get { return maxDiff; }
            set
            {
                maxDiff = value;
                origMaxDiff = maxDiff;
            }
        }

        /** Method:  maximum standard deviation difference allowed within a cluster  */
        internal double MaxStDevDiff
        {
            get { return maxStDevDiff; }
            set
            {
                maxStDevDiff = value;
            }
        }

        /** Method:  original maximum difference allowed within a cluster  */
        internal double OrigMaxDiff
        {
            get { return origMaxDiff; }
            set { origMaxDiff = value; }
        }

        /** Method:  maximum standard deviation allowed within a cluster  */
        internal double MaxStDev
        {
            get { return maxStDev; }
            set
            {
                maxStDev = value;
                origMaxStDev = maxStDev;
            }
        }

        /** Method:  original maximum standard deviation allowed within a cluster  */
        internal double OrigMaxStDev
        {
            get { return origMaxStDev; }
            set { origMaxStDev = value; }
        }

        /** Method:  minimum number of values allowed within a cluster  */
        internal int MinCount
        {
            get { return minCount; }
            set { minCount = value; }
        }

        /** Method:  type of statistical test for joins  */
        internal TestType Test
        {
            get { return testType; }
            set { testType = value; }
        }

        /** Method:  cut-off for trimming  */
        internal double CutOff
        {
            get { return hypoTest.CutOff; }
            set { hypoTest.CutOff = value; }
        }

        #endregion

        #region Setters and Getters

        /** Method:  Calculate mean from a list of clusters
        clusters -  the list of clusters 
         <returns> the calculated mean </returns> */
        internal List<double> GetMeans(List<StatCluster> clusters)
        {
            List<double> means = new List<double>();
            foreach (StatCluster sc in clusters)
            {
                for (int i = sc.FirstIndex; i <= sc.LastIndex; i++) { means.Add(sc.Mean); }
            }
            return means;
        }

        /** Method:  Calculate standard deviation from a list of clusters  */
        internal List<double> GetStDevs(List<StatCluster> clusters)
        {
            List<double> stDevs = new List<double>();
            foreach (StatCluster sc in clusters)
            {
                for (int i = sc.FirstIndex; i <= sc.LastIndex; i++) { stDevs.Add(sc.StDev); }
            }
            return stDevs;
        }

        /** Method:  Limit indexes  */
        internal List<int> GetLimitIndexes(List<StatCluster> clusters)
        {
            List<int> limitIndexes = new List<int>();
            foreach (StatCluster sc in clusters) { limitIndexes.Add(sc.FirstIndex); }
            return limitIndexes;
        }

        #endregion

        #region internal Methods

        /** Method:  Main calculation method  */
        internal List<StatCluster> Clustering(List<double> serie, int firstIndex, int lastIndex)
        {
            if (serie == null) { throw new Exception("Serie is null"); }

            List<StatCluster> clusters = CreateClusters(serie, firstIndex, lastIndex);
            maxDiff = origMaxDiff;
            maxStDev = origMaxStDev;
            while (maxDiff < 100.0)
            {
                if (testType == TestType.Statistics) { JoinNeighbourClustersStatistics(clusters); }
                else { JoinNeighbourClusters(clusters); }
                JoinInnerClusters(clusters);
                CloseBigClusters(clusters);
                maxDiff += origMaxDiff;
                maxStDev += origMaxStDev;
            }
            return clusters;
        }

        /** Method:  Method for performing recursive clustering upon clusters  */
        internal List<StatCluster> RecursiveClustering(List<StatCluster> clusters)
        {
            List<StatCluster> recursiveClusters = new List<StatCluster>();
            recursiveClusters.Add(clusters[clusters.Count - 1]);
            for (int i = 0; i < clusters.Count - 2; i++)
            {
                recursiveClusters.Add(new StatCluster(clusters[clusters.Count - 2 - i], recursiveClusters[i]));
            }
            return recursiveClusters;
        }

        /** Method:  Test of homogeneity of samples  */
        internal List<double> TestHomogeneity(List<double> serie)
        {
            testType = TestType.Wilcoxon;
            List<double> homog = new List<double>();
            StatCluster ini = new StatCluster();
            StatCluster end = new StatCluster();
            end.Values.AddRange(serie);
            for (int i = 0; i < minTestCount; i++)
            {
                ini.Values.Add(serie[i]);
                end.Values.RemoveAt(0);
                homog.Add(0.0);
            }
            for (int i = minTestCount; i < serie.Count - minTestCount; i++)
            {
                homog.Add(TestHomogeneity(ini, end));
                ini.Values.Add(serie[i]);
                end.Values.RemoveAt(0);
            }
            for (int i = 0; i < minTestCount; i++) { homog.Add(0.0); }
            int changeModelIndex = -1;
            for (int i = homog.Count - minTestCount; i >= minTestCount; i--) { if (i > 0.5) changeModelIndex = i; break; }
            return homog;
        }

        /** Method:  Test of homogeneity based in clusters  */
        internal int TestFirstHomogeneousCluster(List<StatCluster> clusters)
        {
            StatCluster ini;
            StatCluster end;
            for (int c = 0; c < clusters.Count - 1; c++)
            {
                ini = new StatCluster();
                end = new StatCluster();
                ini.Values.Add(clusters[c].Mean);
                for (int i = c + 1; i < clusters.Count; i++) { end.Values.Add(clusters[i].Mean); }
                if (ini.FirstIndex > minTestCount && AreHomogeneous(ini, end)) { return ini.FirstIndex; }
            }
            return clusters[clusters.Count - 1].FirstIndex;
        }

        #endregion

        #region Private Methods

        #region Calculate Methods

        private List<StatCluster> CreateClusters(List<double> serie, int firstIndex, int lastIndex)
        {
            List<StatCluster> clusters = new List<StatCluster>();
            StatCluster sC;
            for (int j = firstIndex; j <= lastIndex; j++)
            {
                sC = new StatCluster(j, serie[j]);
                clusters.Add(sC);
            }
            return clusters;
        }

        private void JoinNeighbourClusters(List<StatCluster> clusters)
        {
            int i;
            List<int> joined = new List<int>();
            while (true)
            {
                i = 1;
                joined.Clear();
                while (i < clusters.Count - 1)
                {
                    if (clusters[i - 1].Close || clusters[i].Close || clusters[i + 1].Close)
                    {
                        i = i + 1;
                    }
                    else
                    {
                        if (AreHomogeneous(clusters[i - 1], clusters[i]))
                        {
                            joined.Add(i);
                            clusters[i - 1].Join(clusters[i]);
                            i = i + 2;
                        }
                        else if (AreHomogeneous(clusters[i], clusters[i + 1]))
                        {
                            joined.Add(i + 1);
                            clusters[i].Join(clusters[i + 1]);
                            i = i + 3;
                        }
                        else
                        {
                            i = i + 1;
                        }
                    }
                }
                if (joined.Count == 0) { break; }
                for (int j = joined.Count - 1; j >= 0; j--) { clusters.RemoveAt(joined[j]); }
            }
        }

        private void JoinInnerClusters(List<StatCluster> clusters)
        {
            int i;
            List<int> joined = new List<int>();
            while (true)
            {
                i = 1;
                joined.Clear();
                while (i < clusters.Count - 1)
                {
                    if (!clusters[i - 1].Close && !clusters[i].Close && !clusters[i + 1].Close && clusters[i].Count < minCount && clusters[i - 1].Count + clusters[i + 1].Count > minCount && GetDiff(clusters[i - 1], clusters[i + 1]) < maxDiff && clusters[i].StDev < clusters[i].StDev)
                    {
                        clusters[i - 1].Join(clusters[i]);
                        clusters[i - 1].Join(clusters[i + 1]);
                        joined.Add(i);
                        joined.Add(i + 1);
                        i = i + 3;
                    }
                    else
                    {
                        i = i + 1;
                    }
                }
                if (joined.Count == 0) { break; }
                for (int j = joined.Count - 1; j >= 0; j--) { clusters.RemoveAt(joined[j]); }
            }
        }

        private void JoinNeighbourClustersStatistics(List<StatCluster> clusters)
        {
            int i;
            List<int> joined = new List<int>();
            double diff1, diff2, stDev1, stDev2, stDevDiff1, stDevDiff2, countProp1, countProp2;
            while (true)
            {
                i = 1;
                joined.Clear();
                while (i < clusters.Count - 1)
                {
                    if (clusters[i - 1].Close || clusters[i].Close || clusters[i + 1].Close)
                    {
                        i = i + 1;
                    }
                    else
                    {
                        diff1 = GetDiff(clusters[i - 1], clusters[i]);
                        diff2 = GetDiff(clusters[i], clusters[i + 1]);
                        stDev1 = GetStDev(clusters[i - 1], clusters[i]);
                        stDev2 = GetStDev(clusters[i], clusters[i + 1]);
                        stDevDiff1 = Math.Abs(clusters[i - 1].StDev - clusters[i].StDev);
                        stDevDiff2 = Math.Abs(clusters[i].StDev - clusters[i + 1].StDev);
                        countProp1 = (double)clusters[i - 1].Count / (double)clusters[i].Count;
                        if (countProp1 > 1.0) { countProp1 = 1.0 / countProp1; }
                        countProp2 = (double)clusters[i].Count / (double)clusters[i + 1].Count;
                        if (countProp2 > 1.0) { countProp2 = 1.0 / countProp2; }

                        if (diff1 < diff2 && diff1 <= maxDiff && stDev1 < maxStDev && stDevDiff1 < maxStDevDiff * countProp1)
                        {
                            joined.Add(i);
                            clusters[i - 1].Join(clusters[i]);
                            i = i + 2;
                        }
                        else if (diff1 >= diff2 && diff2 <= maxDiff && stDev2 < maxStDev && stDevDiff2 < maxStDevDiff * countProp2)
                        {
                            joined.Add(i + 1);
                            clusters[i].Join(clusters[i + 1]);
                            i = i + 3;
                        }
                        else
                        {
                            i = i + 1;
                        }
                    }
                }
                if (joined.Count == 0) { break; }
                for (int j = joined.Count - 1; j >= 0; j--) { clusters.RemoveAt(joined[j]); }
            }
        }

        private void CloseBigClusters(List<StatCluster> clusters)
        {
            foreach (StatCluster sc in clusters)
            {
                if (!sc.Close && sc.Count >= minCount) { sc.Close = true; }
            }
        }

        #endregion

        #region Auxiliar Methods

        /** Method:  Checks if two clusters are homogeneous  */
        internal bool AreHomogeneous(StatCluster sc1, StatCluster sc2)
        {
            double pValue = TestHomogeneity(sc1, sc2);
            return pValue >= significance;
        }

        /** Method:  Checks if two clusters are homogeneous  */
        internal bool AreHomogeneous(StatCluster sc1, StatCluster sc2, ref double pValue)
        {
            pValue = TestHomogeneity(sc1, sc2);
            return pValue >= significance;
        }

        /** Method:  Tests if two subsets divided by the index are homogeneous  
        serie -  time series 
        index -  index of first value of final subset */
        internal bool AreHomogeneous(List<double> serie, int index)
        {
            double pValue = -1;
            return AreHomogeneous(serie, index, ref pValue);
        }

        /** Method:  Tests if two subsets divided by the index are homogeneous  
        serie -  time series 
        index -  index of first value of final subset */
        internal bool AreHomogeneous(List<double> serie, int index, ref double pValue)
        {
            if (index <= 0 || index >= serie.Count) { throw new Exception("Index must be positive and smaller than series count"); }

            StatCluster sc1 = new StatCluster();
            for (int i = 0; i < index; i++) { sc1.AddValueAndSum(serie[i]); }
            StatCluster sc2 = new StatCluster();
            for (int i = index; i < serie.Count; i++) { sc2.AddValueAndSum(serie[i]); }
            return AreHomogeneous(sc1, sc2, ref pValue);
        }


        /** Method:  Tests if two subsets divided by the index are homogeneous  
        serie -  time series 
        iniIndex1 -  index of first value of initial subset 
        endIndex1 -  index of last value of initial subset 
        iniIndex2 -  index of first value of final subset 
        endIndex2 -  index of last value of final subset */
        internal bool AreHomogeneous(List<double> serie, int iniIndex1, int endIndex1, int iniIndex2, int endIndex2)
        {
            double pValue = -1;
            return AreHomogeneous(serie, iniIndex1, endIndex1, iniIndex2, endIndex2, ref pValue);
        }

        /** Method:  Tests if two subsets divided by the index are homogeneous  
        serie -  time series 
        iniIndex1 -  index of first value of initial subset 
        endIndex1 -  index of last value of initial subset 
        iniIndex2 -  index of first value of final subset 
        endIndex2 -  index of last value of final subset 
        pValue -  return value of p value calcualted */
        internal bool AreHomogeneous(List<double> serie, int iniIndex1, int endIndex1, int iniIndex2, int endIndex2, ref double pValue)
        {
            if (iniIndex1 < 0 || iniIndex1 >= serie.Count || endIndex1 <= iniIndex1 || endIndex1 >= serie.Count || iniIndex2 <= iniIndex1 || iniIndex2 >= serie.Count || endIndex2 <= iniIndex2 || endIndex2 >= serie.Count)
            {
                throw new Exception("Bad index");
            }

            StatCluster sc1 = new StatCluster();
            for (int i = iniIndex1; i < endIndex1; i++) { sc1.AddValueAndSum(serie[i]); }
            StatCluster sc2 = new StatCluster();
            for (int i = iniIndex2; i < endIndex2; i++) { sc2.AddValueAndSum(serie[i]); }
            return AreHomogeneous(sc1, sc2, ref pValue);
        }


        internal double TestHomogeneity(StatCluster sc1, StatCluster sc2)
        {
            if (sc1.Mean == sc2.Mean && sc1.StDev == sc2.StDev) { return 1.0; }

            double pValue = -1;
            double pValueMean = -1;
            double pValueVar = -1;
            switch (testType)
            {
                case TestType.ParametricMean:
                    hypoTest.ParametricHomogeneity(sc1.Values, sc2.Values, ref pValueMean, ref pValueVar);
                    pValue = pValueMean;
                    break;
                case TestType.ParametricVar:
                    hypoTest.ParametricHomogeneity(sc1.Values, sc2.Values, ref pValueMean, ref pValueVar);
                    pValue = pValueVar;
                    break;
                case TestType.ParametricMeanVar:
                    hypoTest.ParametricHomogeneity(sc1.Values, sc2.Values, ref pValueMean, ref pValueVar);
                    pValue = Math.Max(pValueMean, pValueVar);
                    break;
                case TestType.MannWithney:
                    pValue = hypoTest.MannWhitney(sc1.Values, sc2.Values, true);
                    break;
                case TestType.Wilcoxon:
                    pValue = hypoTest.WaldWolfowitz(sc1.Values, sc2.Values, true);
                    break;
            }
            return pValue;
        }

        private double GetDiff(StatCluster sc1, StatCluster sc2)
        {
            return Math.Abs(sc1.Mean - sc2.Mean);
        }

        private double GetStDev(StatCluster sc1, StatCluster sc2)
        {
            double sum = sc1.Sum + sc2.Sum;
            double sumSq = sc1.SumSq + sc2.SumSq;
            double sqSum = sum * sum;
            int n = sc1.Count + sc2.Count;
            double var = (sumSq - (sqSum / n)) / (n - 1);
            //double var = (n * sumSq - sum*sum) / (n * (n - 1));
            if (var < 1.0) { return 0.0; }
            return Math.Sqrt(var);
        }

        #endregion

        #endregion

        #region Inner Class StatCluster

        /** Method:  Speciphical cluster class for Stationary Clustering  */
        internal class StatCluster
        {

            #region Fields

            private int firstIndex;
            private int lastIndex;
            private int count;
            private double mean;
            private double biasMean;
            private double stDevMean;
            private double sum;
            private double sumSq;
            private bool close;
            private double weight;
            private double minStDev;
            private List<double> values;

            #endregion

            #region Constructors

            /** Method:  Constructor  */
            internal StatCluster()
            {
                values = new List<double>();
                sum = 0;
                sumSq = 0;
                weight = 1;
            }

            /** Method:  Constructor  
            index -  index of cluster 
            val -  value */
            internal StatCluster(int index, double val)
            {
                firstIndex = index;
                lastIndex = index;
                count = 1;
                mean = val;
                sum = val;
                sumSq = val * val;
                weight = 0.0;
                minStDev = 0.5;
                values = new List<double>();
                values.Add(val);
            }

            /** Method:  Constructor  
            sc -  cluster (copy constructor) */
            internal StatCluster(StatCluster sc)
            {
                firstIndex = sc.FirstIndex;
                lastIndex = sc.LastIndex;
                count = sc.Count;
                sum = sc.Sum;
                sumSq = sc.sumSq;
                weight = sc.Weight;
                minStDev = 0.5;
                values = new List<double>();
                values.AddRange(sc.Values);
                sum = 0;
                sumSq = 0;
            }

            /** Method:  Constructor (join)  */
            internal StatCluster(StatCluster sc1, StatCluster sc2)
            {
                firstIndex = Math.Min(sc1.FirstIndex, sc2.FirstIndex);
                lastIndex = Math.Max(sc1.LastIndex, sc2.LastIndex);
                count = sc1.Count + sc2.Count;
                sum = sc1.Sum + sc2.Sum;
                sumSq = sc1.sumSq + sc2.SumSq;
                weight = 0.0;
                minStDev = 0.5;
                values = new List<double>();
                values.AddRange(sc1.Values);
                values.AddRange(sc2.Values);
            }

            #endregion

            #region Properties

            /** Method:  first index in the cluster  */
            internal int FirstIndex
            {
                get { return firstIndex; }
            }

            /** Method:  last index in the cluster  */
            internal int LastIndex
            {
                get { return lastIndex; }
            }

            /** Method:  number of elements  */
            internal int Count
            {
                get { return count; }
            }

            /** Method:  mean of the cluster  */
            internal double Mean
            {
                get { return sum / (double)count; }
            }

            /** Method:  standard deviation of the cluster  */
            internal double StDev
            {
                get
                {
                    if (count == 1) { return minStDev; }
                    else
                    {
                        double num = count * sumSq - sum * sum;
                        if (num < 0.01) { return minStDev; }
                        return Math.Sqrt(num / (count * (count - 1)));
                    }
                }
            }

            /** Method:  mean of the data bias  */
            internal double BiasMean
            {
                get { return biasMean; }
                set { biasMean = value; }
            }

            /** Method:  standard deviation of the data bias  */
            internal double BiasStDev
            {
                get { return stDevMean; }
                set { stDevMean = value; }
            }

            /** Method:  precalculated sum of data (for standard deviation quick calculation)  */
            internal double Sum
            {
                get { return sum; }
            }

            /** Method:  precalculated squared sum of data (for standard deviation quick calculation)   */
            internal double SumSq
            {
                get { return sumSq; }
            }

            /** Method:  if the cluster is closed  */
            internal bool Close
            {
                get { return close; }
                set { close = value; }
            }

            /** Method:  cluster weight to be applied  */
            internal double Weight
            {
                get { return weight; }
                set { weight = value; }
            }

            /** Method:  list of cluster values  */
            internal List<double> Values
            {
                get { return values; }
                set { values = value; }
            }

            #endregion

            #region internal Methods

            /** Method:  Join this cluster with another  */
            internal void Join(StatCluster sc)
            {
                firstIndex = Math.Min(firstIndex, sc.FirstIndex);
                lastIndex = Math.Max(lastIndex, sc.LastIndex);
                count = count + sc.Count;
                sum = 0;
                sumSq = 0;
                foreach (double val in sc.values)
                {
                    sum += val;
                    sumSq += val * val;

                }
                foreach (double val in this.values)
                {
                    sum += val;
                    sumSq += val * val;
                }
                weight = weight + sc.weight;
                values.AddRange(sc.Values);
            }

            /** Method:  Add one value to the cluster  */
            internal void AddValue(double val)
            {
                this.values.Add(val);
                this.count = this.count + 1;
            }

            /** Method:  Add one value to the cluster and increment sums  */
            internal void AddValueAndSum(double val)
            {
                this.values.Add(val);
                this.sum = this.sum + val;
                this.sumSq = this.sumSq + val * val;
                this.count = this.count + 1;
            }

            #endregion

        }

        #endregion

        #region Enum TestType

        /** Method:  Type of hypothesis testing for homogeneity  */
        internal enum TestType
        {
            /** Method:  Join by statistics  */
            Statistics,
            /** Method:  Parametric test for mean  */
            ParametricMean,
            /** Method:  Parametric test for var  */
            ParametricVar,
            /** Method:  Parametric test for both mean and var  */
            ParametricMeanVar,
            /** Method:  Mann-Withney non parametric test  */
            MannWithney,
            /** Method:  Wilcoxon non parametric test  */
            Wilcoxon
        }

        #endregion
    }
}
