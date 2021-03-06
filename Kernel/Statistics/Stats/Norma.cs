#region Imports

using System;
using System.Collections;
using System.Collections.Generic;
using Maths;
using Statistics;

#endregion


namespace Statistics {

    /** Method:  Data normalization class */
    internal class Norma {

        #region Fields

        private double[] datos;
        private double[] norm;
        private double min;
        private double max;
        private double normMax;
        private double rango;
        private int maxClasses;
        private NormType type;
        private StatFunctions stat;

        #endregion

        #region Constructors

        /** Method:  Constructor por defecto */
        internal Norma()
        {
            type = NormType.none;
            normMax = 1.0;
            stat = new StatFunctions();
        }

        /** Method:  Constructor con parametro */
        internal Norma(double[] datos)
        {
            this.datos = datos;
            type = NormType.none;
            normMax = 1.0;
        }

        /** Method:  Constructor con parametro */
        internal Norma(List<double> datos)
        {
            this.datos = datos.ToArray();
            type = NormType.none;
            normMax = 1.0;
        }

        /** Method:  Constructor con parametro */
        internal Norma(IList datos)
        {
            this.datos = new double[datos.Count];
            for (int i = 0; i < datos.Count; i++)
            {
                this.datos[i] = (double)datos[i];
            }
            normMax = 1.0;
        }

        #endregion

        #region Properties

        /** Method:  datos crudos */
        internal double[] Datos
        {
            get { return datos; }
            set { datos = value; }
        }

        /** Method:  datos normalizados */
        internal double[] Norm
        {
            get { return norm; }
        }

        /** Method:  Tipo de normalizacion */
        internal NormType Type
        {
            get { return type; }
        }

        /** Method:  Minimo */
        internal double Min
        {
            get { return min; }
        }

        /** Method:  Maximo */
        internal double Max
        {
            get { return max; }
        }

        /** Method:  Nuevo Maximo */
        internal double NormMax
        {
            get { return normMax; }
            set { normMax = value; }
        }

        /** Method:  Rango */
        internal double Rango
        {
            get { return rango; }
        }

        /** Method:  MaxClasses */
        internal int MaxClasses
        {
            get { return maxClasses; }
            set { maxClasses = value; }
        }

        #endregion

        #region internal Methods

        /** Method:  Normalize data */
        internal void Normalize(NormType type) {
            this.type = type;
            switch (type)   {
                case NormType.none:
                    this.norm = this.datos;
                    break;
                case NormType.minMax:
                    NormalizeMinMax();
                    break;
                case NormType.log:
                    NormalizeLog();
                    break;
                case NormType.logMinMax:
                    NormalizeLogMinMax();
                    break;
                case NormType.min:
                    NormalizeMin();
                    break;
                case NormType.max:
                    NormalizeMax();
                    break;
                case NormType.hash:
                    NormalizeHash();
                    break;
                case NormType.hashMinMax:
                    NormalizeHashMinMax();
                    break;
                case NormType.scale:
                    NormalizeScale();
                    break;

            }
        }

        /** Method:  Get Data */
        internal double GetValue(int index, bool normalized) {
            if (!normalized) { return datos[index]; }
            else { return norm[index]; }
        }

        #endregion

        #region Private Methods

        private void SetMinMax() {
            min = double.MaxValue;
            max = double.MinValue;
            for (int i = 0; i < datos.Length; i++)
            {
                if (datos[i] < min) { min = datos[i]; }
                if (datos[i] > max) { max = datos[i]; }
            }
            rango = max - min;
        }

        private void NormalizeMinMax() {
            SetMinMax();
            if (rango == 0) {
                norm = datos;
                return;
            }
            norm = new double[datos.Length];
            for (int i = 0; i < datos.Length; i++)  {
                norm[i] = ((datos[i] - min) / rango) * normMax;
            }
        }

        private void NormalizeMin()  {
            SetMinMax();
            norm = new double[datos.Length];
            for (int i = 0; i < datos.Length; i++)  {
                norm[i] = (datos[i] - min);
            }
        }

        private void NormalizeMax()  {
            SetMinMax();
            norm = new double[datos.Length];
            for (int i = 0; i < datos.Length; i++)   {
                norm[i] = datos[i] / max;
            }
        }

        private void NormalizeLog() {
            norm = new double[datos.Length];
            for (int i = 0; i < datos.Length; i++)   {
                norm[i] = Math.Log(datos[i]);
            }
        }

        private void NormalizeLogMinMax()  {
            SetMinMax();
            norm = new double[datos.Length];
            for (int i = 0; i < datos.Length; i++)   {
                norm[i] = Math.Log((datos[i] - min) / rango);
            }
        }

        private void NormalizeHash()  {
            double mean = stat.Mean(datos);
            double stDev = stat.StDev(datos);
            norm = new double[datos.Length];
            for (int i = 0; i < datos.Length; i++)  {
                norm[i] = (datos[i] - mean) / Math.Pow(stDev, 2);
            }
        }

        private void NormalizeHashMinMax()  {
            NormalizeHash();
            double min = double.MaxValue;
            double max = double.MinValue;
            for (int i = 0; i < norm.Length; i++)  {
                if (norm[i] < min) { min = norm[i]; }
                if (norm[i] > max) { max = norm[i]; }
            }
            rango = max - min;
            for (int i = 0; i < norm.Length; i++)
            {
                norm[i] = (norm[i] - min) / rango;
            }
        }

        private void NormalizeScale()  {

            SetMinMax();
            if (rango < maxClasses)  {
                norm = datos;
                return;
            }
            double[] normMinMax = new double[datos.Length];
            for (int i = 0; i < datos.Length; i++)  {
                normMinMax[i] = Math.Ceiling(((datos[i] - min) / rango) * 100);
            }

            norm = new double[normMinMax.Length];
            int width = (int)(Math.Ceiling((double)datos.Length / (double)maxClasses));
            int iniClass = 0;
            int nClass = 0;
            double totClass = 0.0;
            double mean;
            for (int i = 0; i < normMinMax.Length; i++)   {
                totClass += normMinMax[i];
                nClass++;
                if (nClass == width || i == normMinMax.Length - 1)  {
                    mean = totClass / nClass;
                    for (int j = iniClass; j <= i; j++) {
                        norm[j] = mean;
                    }
                    nClass = 0;
                    totClass = 0.0;
                    iniClass = i + 1;
                }
            }
        }

        /** Method:  Get scaled value */
        internal double GetScaled(double unSc) {
            return Math.Ceiling((unSc - min) / rango) * 100;
        }

        /** Method:  Get unscaled value */
        internal double GetUnScaled(double sc) {
            return ((sc / 100.0) * rango) + min;
        }

        #endregion

        #region Enums

        /** Method:  Type of normalization */
        internal enum NormType {
            /** Method:  No normalization */
            none,
            /** Method:  Min-Max normalization (from 0 to 1) */
            minMax,
            /** Method:  Logaritmized normalization */
            log,
            /** Method:  Logaritmized and then Min-Max */
            logMinMax,
            /** Method:  Only min normalization */
            min,
            /** Method:  Only max normalization */
            max,
            /** Method:  Hash normalization, optimizing hash level */
            hash,
            /** Method:  Hash normalization and then  min-max normalization */
            hashMinMax,
            /** Method:  Scale normalization */
            scale
        };

        #endregion
    }
}
