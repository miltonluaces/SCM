#region Imports

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Statistics;
using Maths;

#endregion

namespace MonteCarlo  {

    /** Method:  Algorithmic repository class for jacknife resampling calculations */
    internal class JacknifeEstimation {

        #region Fields

        private List<double> datos;
        private KernelDist sDist;
        private double cotaSup;
        private bool searchExtremeValues;
        private int iterations = 30;
        private int maxIterations = 30;
        private double s = 1.0;
        private double eps = 0.01;
        private double noisePerc = 0.1;
        private int maxClasses = 100;
        private double maxValueThreshold = 0.3;
        private Functions func;
        private StatFunctions stat;
        private int maxConvolIterations = 1500;

        #endregion

        #region Constructors

        /** Method:  Constructor */
        internal JacknifeEstimation(List<double> datos) {
            this.func = new Functions();
            this.stat = new StatFunctions();
            this.cotaSup = 20;
            this.searchExtremeValues = false;
            LoadNormalizedData(datos);
            sDist = new KernelDist(iterations, maxConvolIterations, 1, s, eps, noisePerc, maxClasses, maxValueThreshold, maxIterations, -1, 20, 8, 8, 1);
        }

        /** Method:  Constructor  
         datos -  list of data 
         cotaSup -  upper limit of data */
        internal JacknifeEstimation(List<double> datos, double cotaSup) {
            this.func = new Functions();
            this.cotaSup = cotaSup;
            this.searchExtremeValues = false;
            LoadNormalizedData(datos);
            sDist = new KernelDist(iterations, maxConvolIterations, 1, s, eps, noisePerc, maxClasses, maxValueThreshold, maxIterations, -1, 20, 8, 8, 1);
        }

        /** Method:  Constructor  
         datos -  list of data 
         cotaSup -  upper limit of data 
         searchExtremeValues -  if only extreme values should be checked  */
        internal JacknifeEstimation(List<double> datos, double cotaSup, bool searchExtremeValues) {
            this.func = new Functions();
            this.cotaSup = cotaSup;
            this.searchExtremeValues = searchExtremeValues;
            LoadNormalizedData(datos);
            sDist = new KernelDist(iterations, maxConvolIterations, 1, s, eps, noisePerc, maxClasses, maxValueThreshold, maxIterations, -1, 20, 8, 8, 1);
        }

        /** Method:  Constructor  
         frec -  frequencies 
         cotaSup -  upper limit of data 
         searchExtremeValues -  if only extreme values should be checked */
        internal JacknifeEstimation(Frequencies frec, double cotaSup, bool searchExtremeValues) {
            this.func = new Functions();
            this.cotaSup = cotaSup;
            this.searchExtremeValues = searchExtremeValues;
            LoadFrequencies(frec);
            sDist = new KernelDist(iterations, maxConvolIterations, 1, s, eps, noisePerc, maxClasses, maxValueThreshold, maxIterations, -1, 20, 8, 8, 1);
        }


        private void LoadFrequencies(Frequencies frec)
        {
            this.datos = frec.GetValuesXFrec();
        }

        private void LoadNormalizedData(List<double> datos)
        {
            Norma norma = new Norma(datos);
            norma.Normalize(Norma.NormType.minMax);
            if (norma.Max <= cotaSup)
            {
                this.datos = datos;
            }
            else
            {
                Hashtable clases = new Hashtable();
                Hashtable medias = new Hashtable();
                double key;
                for (int i = 0; i < datos.Count; i++)
                {
                    key = (int)(cotaSup * norma.GetValue(i, true));
                    if (clases[key] == null)
                    {
                        clases[key] = new List<double>();
                    }
                    ((List<double>)clases[key]).Add(datos[i]);
                }
                foreach (double keY in clases.Keys)
                {
                    double tot = 0.0;
                    List<double> valores = (List<double>)clases[keY];
                    foreach (double val in valores)
                    {
                        tot += val;
                    }
                    double prom = tot / valores.Count;
                    medias[keY] = cotaSup * ((prom - norma.Min) / norma.Rango);
                }

                this.datos = new List<double>();
                for (int i = 0; i < datos.Count; i++)
                {
                    key = (int)(cotaSup * norma.GetValue(i, true));
                    this.datos.Add((double)medias[key]);
                }
            }
        }

        #endregion

        #region Outlier Detection

        /** Method: 
        Prueba si el valor es un outlier, considerando que no se encuentra en ninguno de los intervalos de
        confianza positivos correspondientes a un nivel dado de probabilidad.
        index -  value index 
        probability -  probability of outliers */
        internal bool IsOutlier(int index, double probability)
        {
            probability = probability - 5;
            List<double> jackData = new List<double>(datos);
            jackData.RemoveAt(index);
            int its = 0;
            sDist.MaxLag = 0;
            sDist.MwAggregation = false;
            sDist.LoadData(jackData, 1, 0, false);
            double p = sDist.GetPercentile(probability / 100.0);
            return (datos[index] >= sDist.GetPercentile(probability));
        }

        /** Method: 
         Prueba si el valor nuevo es un outlier, considerando que no se encuentra en ninguno de los intervalos de
         confianza positivos correspondientes a un nivel dado de probabilidad.
         value -  value itself 
         probability -   probability of outliers */
        internal bool IsOutlier(double value, double probability)
        {
            if (value < 0.0) return true;
            int minLenghtForOutliers = 3;
            if (this.datos.Count <= minLenghtForOutliers) { return false; }

            probability = probability - 5;
            int its = 0;
            sDist.MaxLag = 0;
            sDist.MwAggregation = false;
            sDist.LoadData(datos, 1, 0, false);
            double p = sDist.GetPercentile(probability); //not divided by 100
            return (value >= sDist.GetPercentile(probability));
        }

        /** Method: 
         Obtiene los outliers de una serie dada para un determinado nivel de confianza
         */
        internal List<double> GetOutliers(double probability)
        {
            List<double> outs = new List<double>();
            for (int i = 0; i < datos.Count; i++)
            {
                if (IsOutlier(i, probability)) { outs.Add(datos[i]); }
            }
            return outs;
        }

        #endregion

    }
}
