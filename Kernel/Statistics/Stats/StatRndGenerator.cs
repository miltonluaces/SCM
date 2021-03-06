#region Imports

using System;
using Statistics;
using System.Collections.Generic;
using Maths;

#endregion

namespace Statistics {

    /** Method: 
     RandomProvider.  Provides random numbers of all data types in specified ranges.  It also contains a couple of methods
     from Normally (Gaussian) distributed random numbers and Exponentially distributed random numbers.
     */
    internal class StatRndGenerator
    {

        #region Fields

        private Random rand;
        private double storedUniformDeviate;
        private bool storedUniformDeviateIsGood = false;
        private Functions func;

        #endregion

        #region Constructor

        /** Method:  Constructor */
        internal StatRndGenerator()
        {
            func = new Functions();
            Reset();
        }

        /** Method:  Reset seed */
        internal void Reset()
        {
            rand = new Random(Environment.TickCount);
        }

        #endregion

        #region Distributions

        #region Uniform

        /** Method:  Devuelve un double randómico en el rango [0,1) (cero inc) */
        internal double NextDouble()
        {
            double nextDouble = 0.0;
            lock (rand) { nextDouble = rand.NextDouble(); }
            return nextDouble;
        }

        /** Method:  Devuelve un booleano randómico */
        internal bool NextBoolean()
        {
            double nextBoolean = 0.0;
            lock (rand) { nextBoolean = rand.Next(0, 2); }
            return nextBoolean != 0;
        }

        /** Method:  Devuelve un int en el rango [min, max) */
        internal int NextInt(int min, int max)
        {
            if (max == min) { return min; } else if (max < min) { throw new ArgumentException("Max must be greater than min"); }
            double nextDouble = 0.0;
            lock (rand) { nextDouble = rand.NextDouble(); }
            int randInt = Convert.ToInt32(min + nextDouble * (max - min));
            return randInt;
        }

        /** Method:  Devuelve un double en el rango [min, max) */
        internal double NextDouble(double min, double max)
        {
            if (max <= min) { throw new ArgumentException("Max must be greater than min"); }
            double nextDouble = 0.0;
            lock (rand) { nextDouble = rand.NextDouble(); }
            double randDbl = min + nextDouble * (max - min);
            return randDbl;
        }

        /** Method:  Devuelve un double en el rango [0,1) con distribucion Uniforme */
        internal double NextUniform()
        {
            return NextDouble();
        }

        /** Method:  Devuelve un double en el rango dado con distribucion Uniforme */
        internal double NextUniform(double min, double max)
        {
            return NextDouble(min, max);
        }

        /** Method:  Generar array de randomicos */
        internal double[] GenerarArrayRandom(int cantElementos)
        {
            double[] array = new double[cantElementos];
            double nextDouble = 0.0;
            lock (rand) { nextDouble = rand.Next(); }
            for (int i = 0; i < cantElementos; i++) { array[i] = nextDouble; }
            return array;
        }

        #endregion

        #region Normal

        /** Method:  Devuelve variables tipificadas N(0,1) (sesgos) */
        internal double NextNormal()
        {
            // basado en algoritmo de Numerical Recipes
            if (storedUniformDeviateIsGood)
            {
                storedUniformDeviateIsGood = false;
                return storedUniformDeviate;
            }
            else
            {
                double rsq = 0.0;
                double v1 = 0.0, v2 = 0.0, fac = 0.0;
                while (rsq == 0.0 || rsq >= 1.0)
                {
                    v1 = NextDouble() * 2.0 - 1.0;
                    v2 = NextDouble() * 2.0 - 1.0;
                    rsq = Math.Pow(v1, 2) + Math.Pow(v2, 2);
                }
                fac = Math.Sqrt(Math.Log(rsq, Math.E) / rsq * -2.0);
                storedUniformDeviate = v1 * fac;
                storedUniformDeviateIsGood = true;
                return v2 * fac;
            }
        }

        /** Method:  Devuelve variables N(m,s) (sesgos) 
        m – mean.
        s - standard deviation. */
        internal double NextNormal(double m, double s)
        {
            double z = NextNormal();
            double x = s * z + m;
            return x;
        }


        #endregion

        #region Exponential

        /** Method:  Devuelve sesgos randómicos positivos con media = 1, con distribucion Exponencial */
        internal double NextExponential()
        {
            double dum = 0.0;
            while (dum == 0.0) { dum = NextUniform(); }
            return -Math.Log(dum);
        }

        /** Method:  Devuelve sesgos randómicos positivos con media = m, con distribucion Exponencial */
        internal double NextExponential(double m)
        {
            return NextExponential() + m;
        }

        #endregion

        #region Empirical

        /** Method:  Get a random value with the histogram distribution */
        internal double GetRandomValue(Histogram hist)
        {

            double val = NextDouble(0, hist.TotFreqs);
            int valIndex = BinarySearch(hist.AccumFreqValues, val);
            int key = hist.AccumFreqKeys[valIndex];
            return hist.GetValue(key);
        }

        /** Method:  Get a secquence of n random values with the histogram distribution */
        internal List<double> GetRandomValues(Histogram hist, int n)
        {
            List<double> vals = new List<double>();
            for (int i = 0; i < n; i++) { vals.Add(GetRandomValue(hist)); }
            return vals;
        }

        /** Method:  Get a secquence of n random values with the histogram distribution with a certain likelyhood */
        internal List<double> GetRandomValues(Histogram hist, int n, double likelyhoodThreshold, ref double seriesProb)
        {
            List<double> vals = new List<double>();
            double val = -1.0;
            double prob;
            do
            {
                seriesProb = 0;
                vals.Clear();
                for (int i = 0; i < n; i++)
                {
                    prob = -1;
                    while (prob < 0)
                    {
                        val = GetRandomValue(hist);
                        prob = hist.GetFreq(val) / hist.TotFreqs;
                    }
                    if (seriesProb == 0) { seriesProb = prob; } else { seriesProb *= prob; }
                    vals.Add(val);
                }
            } while (seriesProb < likelyhoodThreshold);

            return vals;
        }

        /** Method:  Get a secquence of n random values with the histogram distribution with a certain likelyhood */
        internal List<double> GetRandomValues(Histogram hist, int n, double likelyhoodThreshold)
        {
            double seriesProb = 0;
            return GetRandomValues(hist, n, likelyhoodThreshold, ref seriesProb);
        }

        /** Method:  Get a set of grouped values (each group of nPerGroup elements) with their probabilities */
        internal SortedDictionary<int, double> GetRandomGroupedValues(Histogram hist, double nPerGroup, int nGroupedValues)
        {
            return GetRandomGroupedValues(hist, nPerGroup, nGroupedValues, -1.0);
        }

        /** Method:  Get a set of grouped values (each group of nPerGroup elements) with their probabilities under a certain likekyhood threshold */
        internal SortedDictionary<int, double> GetRandomGroupedValues(Histogram hist, double nPerGroup, int nGroupedValues, double likelyhoodThreshold)
        {
            int nPerGroupInt = (int)nPerGroup;
            double diff = nPerGroup - (double)nPerGroupInt;

            SortedDictionary<int, double> groupedVals = new SortedDictionary<int, double>();
            double val;
            double prob = 0;
            int key;
            List<double> seriesVals = new List<double>();
            for (int i = 0; i < nGroupedValues; i++)
            {
                seriesVals = GetRandomValues(hist, nPerGroupInt + 1, likelyhoodThreshold, ref prob);
                val = func.Sum(seriesVals, 0, seriesVals.Count - 2);
                val += seriesVals[seriesVals.Count - 1] * diff;
                key = (int)Math.Round(val);
                if (!groupedVals.ContainsKey(key)) { groupedVals.Add(key, prob); }
                else { groupedVals[key] = groupedVals[key] + prob; }
            }
            return groupedVals;
        }

        private int BinarySearch(IList<double> values, double value)
        {
            return BinarySearch(values, value, 0, values.Count - 1);
        }

        private int BinarySearch(IList<double> values, double value, int low, int high)
        {
            if (high == -1 && (value <= values[low + 1])) { return low; }
            if (low == -1 && (high == 0 || value >= values[high - 1] && value >= values[high])) { return high; }
            if (low > high) { return -1; }
            if (high - low <= 1) { return (value < values[low]) ? low : high; }
            int i = low + (high - low) / 2;
            if ((value >= values[i - 1] && value <= values[i])) { return i; }
            if (value < values[i]) { return BinarySearch(values, value, low, i - 1); }
            else { return BinarySearch(values, value, i + 1, high); }

        }

        #endregion

        #endregion

    }
}
