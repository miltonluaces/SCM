#region Imports

using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using Maths;

#endregion

namespace Statistics  {

    internal class Histogram  {

        #region Fields

        private SDict<int, double> freqs;
        private SDict<int, double> accumFreqs;
        private List<int> accumFreqKeys;
        private List<double> accumFreqValues;
        private double totalFreqs;
        private int maxClasses;
        private double min;
        private double max;
        private double mean;
        private double stDev;
        private bool normalized;
        private int loads;
        private Random rand;
        private double width;
        private int maxKey;

        private double total;
        private double error;
        private bool statsCalculated;

        private Dictionary<double, double> cacheProbabilities;

        #endregion

        #region Constructors

        internal Histogram()  {
            this.maxClasses = int.MaxValue;
            freqs = new SDict<int, double>();
            accumFreqs = new SDict<int, double>();
            accumFreqKeys = new List<int>();
            accumFreqValues = new List<double>();
            cacheProbabilities = new Dictionary<double, double>();
            totalFreqs = -1.0;
            width = -1;
            mean = -1;
            Clear();
            rand = new Random(Environment.TickCount);
            total = 0;
            error = 0;
            statsCalculated = false;
        }

        internal Histogram(int maxClasses) : this()  {
            this.maxClasses = maxClasses;
        }


        #endregion

        #region Properties

        internal int MaxClasses  {
            get { return maxClasses; }
        }

        internal double Min  {
            get { return min; }
        }

        internal double Max  {
            set { max = value; }
            get { return max; }
        }

        internal double Range   {
            get { return max - min; }
        }

        internal double Width   {
            get  {
                if (width == -1) { width = (Range + 1) / maxKey; }
                return width;
            }
        }

        internal bool Normalized
        {
            get { return normalized; }
        }

        internal int Count
        {
            get { return freqs.Count; }
        }

        internal int Loads
        {
            get { return loads; }
        }

        internal SDict<int, double> Freqs  {
            get { return freqs; }
            set { freqs = (SDict<int, double>)value; }
        }

        internal SDict<int, double> AccumFreqs
        {
            get { return accumFreqs; }
        }

        internal List<int> AccumFreqKeys
        {
            get { return accumFreqKeys; }
        }

        internal List<double> AccumFreqValues
        {
            get { return accumFreqValues; }
        }

        internal double TotFreqs
        {
            get
            {
                if (totalFreqs == -1) { CalculateStatistics(); }
                return totalFreqs;
            }
        }

        internal double Mean
        {
            get
            {
                if (mean == -1) { CalculateStatistics(); }
                return mean;
            }
        }

        internal double StDev
        {
            get
            {
                if (stDev == -1) { CalculateStatistics(); }
                return stDev;
            }
        }


        internal double RelError
        {
            get { return error / total; }
        }

        internal int NonZeroFreqs
        {
            get { return freqs.Count; }
        }

        #endregion

        #region internal Methods

        #region Getters

        internal double GetFreq(double value)
        {
            int key = GetKey(value);
            if (!freqs.ContainsKey(key)) { return -1; }
            return freqs[key];
        }

        internal double GetAccumFreq(double value)
        {
            int key = GetKey(value);
            if (!accumFreqs.ContainsKey(key)) { return -1; }
            return accumFreqs[key];
        }

        internal bool Contains(double value)
        {
            int key = GetKey(value);
            return freqs.ContainsKey(key);
        }

        internal double GetNormValue(double value)
        {
            if (!normalized) { return value; }
            return GetValue(GetKey(value));
        }

        #endregion

        #region Load Data

        internal void LoadData(List<double> values)  {
            SetDataMinMax(values);
            normalized = (Range > maxClasses);
            foreach (double value in values) { AddValue(value); }
            loads++;
        }

        internal void LoadDist(List<double> freqs)  {
            SetDistMinMax(freqs);
            normalized = (Range > maxClasses);
            for (int i = (int)min; i <= (int)max; i++) { AddValue((double)i, freqs[i]); }
            loads++;
        }

        internal void LoadDist(SDict<int, double> sortFreqs)  {
            SetDistMinMax(sortFreqs);
            normalized = (Range > maxClasses);
            if (!normalized)  {
                freqs = sortFreqs;
            }
            else {
                foreach (int key in sortFreqs.Keys)  {
                    AddValue((double)key, sortFreqs[key]);
                }
            }
            loads++;
        }

        internal void LoadDist(IDictionary<int, double> sortFreqs, double min, double max)  {
            if (min < this.min) { this.min = min; }
            if (max > this.max) { this.max = max; }
            normalized = (Range > maxClasses);
            foreach (int key in sortFreqs.Keys)
            {
                AddValue((double)key, sortFreqs[key]); //TODO: revisar si debe ser key o value el primer parametro
            }
            loads++;
        }

        internal void LoadHist(Histogram hist)  {
            this.min = hist.Min;
            this.max = hist.Max;
            this.totalFreqs = hist.TotFreqs;
            normalized = (Range > maxClasses);

            var sortedkeys = new List<int>(hist.freqs.Keys);
            sortedkeys.Sort();
            foreach (int key in sortedkeys)  {
                AddValue(hist.GetValue(key), hist.freqs[key]);
            }
        }

        internal void LoadHist(double min, double max, double totFreqs, double range, int maxClasses, SDict<int,double> freqs)  {
            this.min = min;
            this.max = max;
            this.totalFreqs = totFreqs;
            normalized = (Range > maxClasses);

            var sortedkeys = new List<int>(freqs.Keys);
            sortedkeys.Sort();
            foreach (int key in sortedkeys)  {
                AddValue(GetValue(key), freqs[key]);
            }
        }

        internal void LoadHist(Histogram h1, Histogram h2)  {
            List<double> freqs = h1.GetFreqs();
            freqs.AddRange(h2.GetFreqs());
            this.LoadDist(freqs);
        }

        internal void LoadData(List<double> values, IList<double> weights)
        {
            SetDataMinMax(values);
            normalized = (Range > maxClasses);
            for (int i = 0; i < values.Count; i++) { AddValue(values[i], weights[i]); }
            loads++;
        }

        internal void CalculateStatistics()
        {
            if (statsCalculated) { return; }

            double accum = 0.0;
            double sum = 0.0;
            double sumSq = 0.0;
            double val, freq;

            var sortedkeys = new List<int>(freqs.Keys);
            sortedkeys.Sort();
            foreach (int key in sortedkeys)
            {
                val = GetValue(key);
                freq = freqs[key];
                accum += freq;
                accumFreqKeys.Add(key);
                accumFreqValues.Add(accum);
                accumFreqs.Add(key, accum);
                sum += (val * freq);
                sumSq += (val * val * freq);
            }
            totalFreqs = accum;
            mean = sum / (double)totalFreqs;
            double sqSum = sum * sum;
            if (totalFreqs == 0 || freqs.Count < 2) { stDev = 0; }
            else { stDev = Math.Sqrt((sumSq - (sqSum / totalFreqs)) / (totalFreqs)); }
            if (double.IsNaN(stDev))
            {
                throw new Exception("StDev cannot be negative");
            }
            statsCalculated = true;
        }

        internal void Clear()
        {
            freqs.Clear();
            min = double.MaxValue;
            max = double.MinValue;
            normalized = false;
        }

        #endregion

        #region To Lists

        internal List<double> GetValues()
        {
            List<double> values = new List<double>();
            foreach (int key in freqs.Keys)
            {
                values.Add(GetValue(key));
            }
            return values;
        }

        internal List<double> GetFreqs()
        {
            List<double> frequencies = new List<double>();
            foreach (int key in freqs.Keys)
            {
                frequencies.Add(freqs[key]);
            }
            return frequencies;
        }

        internal double[] GetArrayNormFreqs()
        {
            double[] arrayFreqs = new double[(int)max + 1];
            foreach (int key in freqs.Keys)
            {
                arrayFreqs[key] = freqs[key];
            }
            return arrayFreqs;
        }

        internal IEnumerator<int> GetKeysEnumerator()
        {
            return freqs.Keys.GetEnumerator();
        }

        internal List<int> GetKeys()
        {
            List<int> keys = new List<int>();
            foreach (int key in freqs.Keys) { keys.Add(key); }
            return keys;
        }

        internal List<double> GetFreqsList()
        {
            List<double> freqsList = new List<double>();
            for (int i = 0; i < maxClasses; i++)
            {
                if (freqs.ContainsKey(i)) { freqsList.Add(freqs[i]); }
                else { freqsList.Add(0.0); }
            }
            return freqsList;
        }

        internal double ProbabilityByValue(double value)
        {
            //if(cacheProbabilities.ContainsKey(value)) { return cacheProbabilities[value]; } 
            double freqValue = GetFreq(value);
            if (freqValue <= 0) { return 0; }
            double prob = freqValue / TotFreqs;
            //cacheProbabilities.Add(value, prob);
            return prob;

        }

        internal double ProbabilityByKey(int key)
        {
            if (cacheProbabilities.ContainsKey(key)) { return cacheProbabilities[key]; }
            double freqValue = freqs[key];
            if (freqValue <= 0) { return 0; }
            double prob = freqValue / TotFreqs;
            cacheProbabilities.Add(key, prob);
            return prob;
        }

        internal List<double> GetRawData()
        {
            double normFactor = 1000 / TotFreqs; ;
            List<double> values = GetValues();
            List<double> freqs = GetFreqs();
            List<double> rawData = new List<double>();
            for (int i = 0; i < values.Count; i++)
            {
                for (int j = 0; j < (int)(Math.Round(freqs[i] * normFactor)); j++) { rawData.Add(values[i]); }
            }
            return rawData;
        }

        #endregion

        #endregion

        #region Private Methods

        internal int GetKey(double value, ref double total, ref double error)
        {
            if (normalized) { value = Normalize(value); }
            int round = (int)Math.Round(value);
            total = Math.Max(value, round);
            error = Math.Abs(value - round);
            return round;
        }

        internal int GetKey(double value)
        {
            if (normalized) { value = Normalize(value); }
            int round = (int)Math.Round(value);
            return round;
        }

        internal double GetValue(int key)
        {
            if (normalized) { return UnNormalize((double)key); }
            return (double)key;
        }

        internal int GetConvKey(double value, int n)
        {
            if (normalized) { value = NormalizeConv(value, n); }
            int round = (int)Math.Round(value);
            return round;
        }

        internal double GetConvValue(int key, int n)
        {
            if (normalized) { return UnNormalizeConv((double)key, n); }
            return (double)key;
        }

        private void AddValue(double value)
        {
            double tot = -1;
            double err = -1;
            int key = GetKey(value, ref tot, ref err);
            total += tot;
            error += err;
            if (freqs.ContainsKey(key))
            {
                freqs[key] = freqs[key] + 1.0;
            }
            else
            {
                freqs.Add(key, 1.0);
                if (key > maxKey) { maxKey = key; }
            }
        }

        private void AddValue(double value, double freq)
        {
            if (freq == 0) { return; }
            double tot = -1;
            double err = -1;
            int key = GetKey(value, ref tot, ref err);
            total += tot * freq;
            error += err * freq;
            if (freqs.ContainsKey(key))
            {
                freqs[key] = freqs[key] + freq;
            }
            else
            {
                freqs.Add(key, freq);
                if (key > maxKey) { maxKey = key; }
            }
        }

        private void SetDataMinMax(List<double> values)
        {
            for (int i = 0; i < values.Count; i++)
            {
                if (values[i] < 0) { values[i] = 0; }
                if (values[i] < min) { min = values[i]; }
                if (values[i] > max) { max = values[i]; }
            }
        }

        private void SetDistMinMax(List<double> freqs)
        {
            for (int i = 0; i < freqs.Count; i++)
            {
                if (freqs[i] == 0) { continue; }
                if (min == double.MaxValue) { min = (double)i; }
                max = (double)i;
            }
        }

        private void SetDistMinMax(IDictionary<int, double> freqs)
        {
            foreach (int key in freqs.Keys)
            {
                if (min == double.MaxValue) { min = (double)key; }
                max = (double)key;
            }
        }

        private double Normalize(double value)
        {
            return ((value - min) / Range) * maxClasses;
        }

        private double UnNormalize(double value)
        {
            return (value * Range) / maxClasses + min;
        }

        private double NormalizeConv(double value, int n)
        {
            return ((value - min * n) / Range) * maxClasses;
        }

        private double UnNormalizeConv(double value, int n)
        {
            return (value * Range) / maxClasses + min * n;
        }

        #endregion
    }
}
