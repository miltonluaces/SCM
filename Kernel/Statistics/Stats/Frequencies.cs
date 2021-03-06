#region Imports

using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using Maths;

#endregion

namespace Statistics {

    /** Method:  Frecuencies of a set of values  */
    internal class Frequencies {

        #region Fields

        private List<double> frec;
        private int nValues;
        private double min;
        private double max;
        private double sum;
        private double sum2;

        #endregion

        #region Constructor

        /** Method:  Constructor  */
        internal Frequencies()
        {
            frec = new List<double>();
            Clear();
        }

        #endregion

        #region Load Data

        /** Method:  Clears all frequencies  */
        internal void Clear()
        {
            frec.Clear();
            nValues = 0;
            min = double.MaxValue;
            max = -1;
            sum = 0.0;
            sum2 = 0.0;
        }

        /** Method:  Adds a value  */
        internal void Add(double valor)
        {
            int val = (int)valor;
            if (val < min) { min = val; }
            if (val >= frec.Count - 1)
            {
                for (int i = frec.Count; i <= val + 1; i++) { frec.Add(0.0); }
                max = val;
            }
            frec[val] = frec[val] + 1;
            nValues++;
            sum += val;
            sum2 += Math.Pow(val, 2);
        }

        /** Method:  Adds an absolute value   */
        internal void AddAbs(double valor)
        {
            Add(Math.Abs(valor));
        }

        /** Method:  Removes a value  */
        internal void Delete(double valor)
        {
            int val = (int)valor;
            if (frec[val] == 0) { throw new Exception("Cannot delete values with zero frequency"); }

            frec[val] = frec[val] - 1;
            nValues--;
            sum -= val;
            sum2 -= Math.Pow(val, 2);

            if (val == min && frec[val] == 0)
            {
                int newMin = val + 1;
                while (frec[newMin] == 0) { newMin++; }
                min = (double)newMin;
            }
            if (val == max && frec[val] == 0)
            {
                int newMax = val - 1;
                while (newMax == 0) { frec.RemoveAt(newMax); newMax--; }
                max = (double)newMax;
            }
        }

        /** Method:  Adds n values  */
        internal void AddRange(double[] valores)
        {
            foreach (int val in valores) { Add(val); }
        }

        /** Method:  Adds a value as double  */
        internal void Add(int val)
        {
            Add((double)val);
        }

        /** Method:  Adds n values in a IList of ints  */
        internal void AddRange(IList<int> valores)
        {
            foreach (int val in valores) { Add(val); }
        }

        /** Method:  Adds n values ina a IList of doubles  */
        internal void AddRange(IList<double> valores)
        {
            foreach (int val in valores) { Add(val); }
        }

        /** Method:  Add n values in a IList of doubles in absolute value  */
        internal void AddRangeAbs(IList<double> valores)
        {
            foreach (int val in valores) { AddAbs(val); }
        }

        /** Method:  Removes n values in a IList of doubles  */
        internal void DeleteRange(IList<double> valores)
        {
            foreach (int val in valores) { Delete(val); }
        }


        /** Method:  Normalized data load
         valores -  List of values 
         cotaSup -  superior cota */
        internal void LoadNormalizedData(List<double> valores, double cotaSup)
        {
            Clear();
            List<double> valoresNorm = new List<double>();
            Norma norma = new Norma(valores);
            norma.Normalize(Norma.NormType.minMax);
            if (norma.Max <= cotaSup)
            {
                valoresNorm = valores;
            }
            else
            {
                Hashtable clases = new Hashtable();
                Hashtable medias = new Hashtable();
                double key;
                for (int i = 0; i < valores.Count; i++)
                {
                    key = (int)(cotaSup * norma.GetValue(i, true));
                    if (clases[key] == null)
                    {
                        clases[key] = new List<double>();
                    }
                    ((List<double>)clases[key]).Add(valores[i]);
                }
                foreach (double keY in clases.Keys)
                {
                    double tot = 0.0;
                    List<double> values = (List<double>)clases[keY];
                    foreach (double val in values)
                    {
                        tot += val;
                    }
                    double prom = tot / values.Count;
                    medias[keY] = cotaSup * ((prom - norma.Min) / norma.Rango);
                }

                valoresNorm = new List<double>();
                for (int i = 0; i < valores.Count; i++)
                {
                    key = (int)(cotaSup * norma.GetValue(i, true));
                    valoresNorm.Add((double)medias[key]);
                }
            }
            AddRange(valoresNorm);
        }

        /** Method:  Data load from a dictionary   */
        internal void LoadData(Dictionary<int, double> dict)
        {

            Clear();
            double val;
            foreach (int key in dict.Keys)
            {
                val = dict[key];
                if (key < min) { min = key; }
                nValues += (int)val;
                sum += key * val;
                sum2 += Math.Pow(key, 2) * val;
                if (key >= frec.Count - 1)
                {
                    for (int i = frec.Count; i <= key + 1; i++) { frec.Add(0.0); }
                    max = key;
                }
                frec[key] = val;
            }
        }

        /** Method:  Returns a collection of values repeated any times acording to each frequency/  */
        internal List<double> GetValuesXFrec()
        {
            List<double> valXFrec = new List<double>();
            for (int v = 0; v < this.frec.Count; v++)
            {
                for (int f = 0; f < (int)frec[v]; f++)
                {
                    valXFrec.Add(v);
                }
            }
            return valXFrec;
        }

        /** Method:  Loads a dictionary of values and its not-null frequencies  */
        internal void GetValuesXFrecDict(Dictionary<int, double> valXFrec)
        {
            for (int i = 0; i < this.frec.Count; i++)
            {
                if (frec[i] != 0)
                {
                    valXFrec[i] = frec[i];
                }
            }
        }

        #endregion

        #region Properties

        /** Method:  Minimum  */
        internal double Min
        {
            get { return min; }
        }

        /** Method:  Maximum  */
        internal double Max
        {
            get { return max; }
        }

        internal double Range
        {
            get { return max - min; }
        }

        /** Method:  Number of values  */
        internal int NValues
        {
            get { return nValues; }
        }

        /** Method:  Number of frequencies  */
        internal int NFrecs
        {
            get { return frec.Count; }
        }

        /** Method:  Frequencies (included zero in max + 1)  */
        internal List<double> ValFrecs
        {
            get { return frec; }
        }

        /** Method:  Sum of values  */
        internal double Sum
        {
            get { return sum; }
        }

        /** Method:  Squared sum of values  */
        internal double Sum2
        {
            get { return sum2; }
        }

        /** Method:  Values mean  */
        internal double Mean
        {
            get { return Sum / NValues; }
        }

        /** Method:  Values variance  */
        internal double Var
        {
            get { return (NValues * sum2 - Math.Pow(sum, 2)) / (NValues * (NValues - 1.0)); }
        }

        /** Method:  Standard deviation  */
        internal double StDev
        {
            get { return Math.Sqrt(Var); }
        }

        /** Method:  Populates an arrayList of values  */
        internal ArrayList Values
        {
            get
            {
                ArrayList values = new ArrayList();
                for (int i = 0; i < NFrecs; i++) { values.Add((double)i); }
                return values;
            }
        }

        /** Method:  Populates an arrayList of frequencies  */
        internal ArrayList Frecs
        {
            get
            {
                ArrayList frecs = new ArrayList();
                for (int i = 0; i < NFrecs; i++) { frecs.Add((double)frec[i]); }
                return frecs;
            }
        }

        #endregion

        #region ToString Override

        /** Method:  ToString override   */
        public override string ToString()
        {
            string str = "";
            for (int i = 0; i < NFrecs; i++)
            {
                str += (i + "\t" + frec[i].ToString("0.000")) + "\n";
            }
            return str;
        }

        /** Method: To String for not null frequencies */
        internal string ToStringNotNull()
        {
            string str = "";
            for (int i = 0; i < NFrecs; i++)
            {
                if (frec[i] != 0)
                {
                    str += (i + "\t" + frec[i].ToString("0.000")) + "\n";
                }
            }
            return str;
        }

        #endregion

    }
}
