#region Imports

using System;
using System.Collections;
using System.Collections.Generic;

#endregion

namespace Maths {
    
    /** Method:  Miscellaneous functions */
    internal class Functions {

        #region Fields

        private Polynom poly;
   
        #endregion

        #region Constructor

        /** Method:  Constructor  */
        internal Functions() {
            poly = new Polynom();
        }

        #endregion

        #region Distances

        /** Method:  Euclidian distance 
        Vector1,2 - Vectors of dobles that represent points */
        internal double DistEuclid(double[] Vector1, double[] Vector2) {
            if(Vector1.Length != Vector2.Length) { throw new Exception("no se puede calcular la distancia"); }
            double dist = 0.0;
            for(int i=0;i<Vector1.Length;i++) {
                dist += Math.Pow(Vector1[i] - Vector2[i], 2);
            }
            return Math.Sqrt(dist);
        }

        #endregion

        #region Moving Average

        /** Method:  Moving average algorithm 
        values - Lista de valores para los cuales se desea obtener la media movil
        mw - Numero de observaciones con las que se obtiene la media de cada punto
        centered -  Si el numero de valores utilizados es mw/2 a la izquierda y otros tantos a la derecha. O si por el  contrario, son todos a la derecha del valor para el cual se calcula la media.*/
        internal List<double> MovingAverage(List<double> values, int mw, bool centered) {
            if(centered) { return MovingAverageCentered(values, mw); } else { return MovingAverage(values, mw); }
        }

        /** Method:  Promedios moviles sin centrar
        values - Lista de valores para los cuales se desea obtener la media movil
        mw - Numero de observaciones con las que se obtiene la media de cada punto*/
        private List<double> MovingAverage(List<double> values, int mw) {
            double average = 0.0;
            List<double> movingAverage = new List<double>();
            if(values.Count <= mw) {
                foreach(double val in values) {
                    average += val;
                }
                foreach(double val in values) {
                    movingAverage.Add(average/values.Count);
                }
            }
            else {
                //inicio
                for(int j=0;j<mw;j++) {
                    average += values[j];
                }
                for(int i=0;i<mw;i++) {
                    movingAverage.Add(average/mw);
                }
                //medio
                for(int i=mw;i<values.Count;i++) {
                    average = 0.0;
                    for(int j=i-mw+1;j<=i;j++) {
                        average += values[j];
                    }
                    movingAverage.Add(average/mw);
                }
            }
            return movingAverage;
        }

        /** Method:  Promedios moviles centrados
      values - Lista de valores para los cuales se desea obtener la media movil
        mw - Numero de observaciones con las que se obtiene la media de cada punto.*/
          private List<double> MovingAverageCentered(List<double> values, int mw) {
            List<double> movingAverage = new List<double>();
            int ini = (int)mw/2;
            int fin;
            double average = 0.0;
            //inicio
            for(int j=0;j<mw;j++) {
                average += values[j];
            }
            for(int i=0;i<ini;i++) {
                movingAverage.Add(average/mw);
            }
            //medio
            for(int i=ini;i<values.Count-ini;i++) {
                average = 0.0;
                fin = (mw%2==0)? i+ini : i+ini+1;
                for(int j=i-ini;j<fin;j++) {
                    average += values[j];
                }
                movingAverage.Add(average/mw);
            }
            //final
            average = 0.0;
            for(int j=values.Count-mw;j<values.Count;j++) {
                average += values[j];
            }
            for(int i=values.Count-(mw-ini)-1;i<values.Count;i++) {
                movingAverage.Add(average/mw);
            }
            return movingAverage;
        }

        #endregion
  
        #region Miscellaneous

        /** Method:  Sum of values of a vector */
        internal double Sum(IList<double> values) {
            return Sum(values, 0, values.Count-1);
        }

        /** Method:  Sum an interval of values of a vector */
        internal double Sum(IList<double> values, int start, int end) {
            if(start < 0 || end > values.Count) { throw new ApplicationException("Sum out of interval"); }
            double sum = 0.0;
            for(int i=start;i<=end;i++) { sum += (double)values[i]; }
            return sum;
        }

        /** Method:  Calculate min of an array of values */
        internal double Min(double[] valores) {
            double min = 0.0;
            foreach(double val in valores) {
                if(val < min) { min = val; }
            }
            return min;
        }

        internal int Max(int a, int b, int c)
        {
            int max = a;
            if (b > max) { max = b; }
            if (c > max) { max = c; }
            return max;
        }

        internal double Max(double a, double b, double c)
        {
            double max = a;
            if (b > max) { max = b; }
            if (c > max) { max = c; }
            return max;
        }
        
        /** Method:  Calculate max of an array of values */
        internal double Max(List<double> valores) {
            double max = 0.0;
            foreach(double val in valores) {
                if(val > max) { max = val; }
            }
            return max;
        }

        /** Method:  Calculate max index of an array of values */
        internal int MaxIndex(List<double> valores) {
            double max = 0.0;
            int index = -1;
            for(int i=0;i<valores.Count;i++) {
                if (valores[i] > max) { max = valores[i]; index = i; }
            }
            return index;
        }

        /** Method:  Normalize to positive data */
        internal List<double> ConvertToPositive(List<double> data) {
            double mindata = 0;
            List<double> v = new List<double>();

            for(int i = 0;i < data.Count;i++)
                if(data[i] < mindata)
                    mindata = data[i];
            mindata = Math.Abs(mindata);
            for(int i = 0;i < data.Count;i++)
                v.Add(data[i] + mindata);
            return v;
        }

        /** Method:  Calculate first intersection (from left to right) point between two functions */
        internal int GetFirstIntersectionIndex(double[] arr1, double[] arr2) {
            double lastSign = Math.Sign(arr1[0] - arr2[0]);
            double sign;
            for(int i=1;i<arr1.Length;i++) {
                sign = Math.Sign(arr1[i] - arr2[i]);
                if(sign != lastSign) { return i; }
            }
            if(lastSign > 0) { return -1; }  //No se cortan, siendo el primer array siempre mayor
            else if(lastSign < 0) { return -2; }  //No se cortan, siendo el segundo array siempre mayor
            return 0;
        }
        
        /** Method:  Calculates series of increments or decrements (proportions) upon last value (multiplicative series) */
        internal List<double> GetMultiplicative(List<double> serie) {
            List<double> incFactorSerie = new List<double>();
            incFactorSerie.Add(1);
            for (int i = 1; i < serie.Count; i++) { 
                incFactorSerie.Add(serie[i]/ serie[i-1]);
            }
            return incFactorSerie;
        }

        #endregion

        #region Double lists operations

        /** Method:  Substract the second list from the first list */
        internal List<double> Substract(List<double> s1, List<double> s2, bool abs, bool sort) {
            if(s1.Count != s2.Count) { throw new Exception("both must have same size"); }

            List<double> res = new List<double>();
            for(int i=0;i<s1.Count;i++) {
                if(abs) {
                    res.Add(Math.Abs(s1[i]) - s2[i]);
                } else {
                    res.Add(s1[i] - s2[i]);
                }
            }
            if(sort) { res.Sort(); }
            return res;
        }

        /** Method:  Calculate lower limit of the confidence interval (naive)*/
        internal double GetInfLimitCI(List<double> sortedList, double probability, bool skipZeros) {
            if(probability < 0.0 || probability > 1.0) { throw new Exception("probability must be between 0 and 1"); }
            int lastIndex = 0;
            int firstNoZero = 0;
            int nValues = 0;
            if(skipZeros) {
                for(int i=0;i<sortedList.Count;i++) {
                    if(sortedList[i] != 0) { firstNoZero = i; nValues = sortedList.Count - firstNoZero; break; }
                }
            } else {
                nValues = sortedList.Count-1;
            }
            lastIndex = firstNoZero + (int)Math.Floor(nValues * probability); //+1
            if(lastIndex >= sortedList.Count) { return double.MaxValue; }
            return sortedList[lastIndex];
        }

        /** Method:  Filter outliers (naive) */
        internal List<double> FilterOutliers(List<double> serie, List<double> model, double probability, ref double infLim) {
            List<double> resid = Substract(serie, model, false, false);
            List<double> sortedResid = new List<double>();
            sortedResid.AddRange(resid);
            sortedResid.Sort();

            infLim = GetInfLimitCI(sortedResid, probability, true);
            //Console.WriteLine("InfLim = " + infLim);
            List<double> filteredList = new List<double>();
            double inf = double.MaxValue;
            for(int i=0;i<resid.Count;i++) {
                if(resid[i] < infLim) { filteredList.Add(serie[i]); } else { if(serie[i] < inf) { inf = serie[i]; } }
                //Console.WriteLine("res = " + resid[i].ToString("0.0#") + " - serie = " + serie[i]);
            }
            infLim = inf;
            return filteredList;
        }

        #endregion

        #region Forgetting Function

        /** Method:  Memory forgetting function for a serie */
        internal double[] CalcTempWeights(int totalPeriods, int forgetInitPeriods, int forgetEndPeriods, double forgetEndProportion) {
            double[] weights = new double[totalPeriods];
            for (int j = 0; j < totalPeriods; j++) { weights[j] = 1.00; }
            ApplyForgettingFunction(weights, forgetInitPeriods, forgetEndPeriods, forgetEndProportion);
            return weights;
        }


        /** Method:  Memory forgetting function  */
        internal void ApplyForgettingFunction(double[] weights, int forgetInitPeriods, int forgetEndPeriods, double forgetEndProportion) {
            if(forgetInitPeriods == forgetEndPeriods) { return; }
            int forgetInitIndex = weights.Length - forgetInitPeriods;
            int forgetEndIndex = weights.Length - forgetEndPeriods;
            if(forgetInitIndex < 0) { forgetInitIndex = 0; }
            if(forgetEndIndex < 0) {
                forgetEndProportion = LinearFunction(0, forgetEndIndex, forgetEndProportion, forgetInitIndex, 1.0);
                forgetEndIndex = 0;
            }
            if (forgetInitIndex == forgetEndIndex) { return; }

            //constant final forgetting
            for(int i=0;i<forgetEndIndex;i++) {
                weights[i] *= forgetEndProportion;
            }

            //variable ramp forgetting 
            double x1 = forgetEndIndex;
            double y1 = forgetEndProportion;
            double x2 = forgetInitIndex;
            double y2 = 1.0;

            double a = (y2 - y1) / (x2 - x1);
            double b = y1 - (x1 * (y2 - y1) / (x2 - x1));
            for(int x=forgetEndIndex;x<forgetInitIndex;x++) {
                weights[x] *= LinearFunction(x, x1, y1, x2, y2);
            }
        }

        /** Method:  Linear function (using line equation) */
        /// <param name="x"> independent variable </param>
        /// <param name="x1"> first defining point (x axis value) </param>
        /// <param name="y1"> first defining point (y axis value) </param>
        /// <param name="x2"> second defining point (x axis value) </param>
        /// <param name="y2"> second defining point (y axis value)</param>
        /// <returns> functional value for x </returns>
        internal double LinearFunction(double x, double x1, double y1, double x2, double y2) {
            double a = (y2 - y1) / (x2 - x1);
            double b = y1 - (x1 * (y2 - y1) / (x2 - x1));
            return a * x + b;
        }

        /** Method:  Sigmoidal function between two points */
        internal double SigmoidalFunction(double x, double x1, double y1, double x2, double y2) {
            return 1.0 / (1.0 + Math.Exp(-x));
        }

        #endregion

        #region Magnitude split


        /** Method:  Discriminate magnitudes 
        values - values of the set or serie
        magnitudeBase - base of logarithm for magnitude split
        normalized - if values should be stored normalized or not */
        internal List<List<double>> MagnitudeSplit(List<double> values, double magnitudeBase, bool normalized) {
            List<List<double>> magnitudes = new List<List<double>>();
            for(int i=0;i<10;i++) { magnitudes.Add(new List<double>()); }
            int exp;
            double denom;
            foreach(double value in values) {
                exp = (int)Math.Log(value, magnitudeBase);
                if(normalized) {
                    denom =  Math.Pow(magnitudeBase, exp);
                    magnitudes[exp].Add(Math.Round(value/denom));
                }
                else {
                    magnitudes[exp].Add(value);
                }
            }
            while(magnitudes[magnitudes.Count-1].Count == 0) { magnitudes.RemoveAt(magnitudes.Count-1); }
            return magnitudes;
        }

        #endregion

    }
}
