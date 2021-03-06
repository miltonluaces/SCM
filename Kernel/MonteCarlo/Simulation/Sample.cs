#region Imports

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Statistics;
using Maths;

#endregion

namespace MonteCarlo {
    
    /**  Container class for a single sample and its distribution  */
    internal class Sample {
  
        #region Fields

        private int n;
        private List<double> datos;
        private List<double> valores = null;
        private List<double> frecuencias = null;
        private Histogram histogram;
        private Dist dist;
        private double mean;
        private double var;
        private RndGenerator randGen;
        private StatFunctions stat;
        private Functions func;
        private double[] weights;
        private double epsilon;
        
        #endregion

        #region Constructors

       /** Method:  Constructor  
        datos -  list of data 
        random -  if random should be applied */
        internal Sample(List<double> datos, bool random) {
            this.randGen = new RndGenerator();
            randGen.Reset();
            this.func = new Functions();
            this.stat = new StatFunctions();
            histogram = new Histogram();
            weights = new double[datos.Count];
            for(int i=0;i<datos.Count;i++) { weights[i] = 1.0; }
            LoadData(datos, weights, random);
  
            //SetStatistics();

        }

       /** Method:  Constructor  
        datos -  list of data 
        weights -  array of weights to apply 
        random -  if random should be applied */
        internal Sample(List<double> datos, double[] weights, bool random) {
            this.randGen = new RndGenerator();
            randGen.Reset();
            this.func = new Functions();
            histogram = new Histogram();
            LoadData(datos, weights, random);

            //SetStatistics();

        }

       /** Method:  Constructor  
        n -  number of data 
        dist -  distribution of data */
        internal Sample(int n, Dist dist) {
            this.randGen = new RndGenerator();
            this.func = new Functions();
            this.n = n;
            this.dist = dist;
            this.datos = new List<double>();
            randGen.Reset();
  
        }

        #endregion

        #region Properties

       /** Method:  Sample data wrappered */
        internal List<double> Datos {
            get { return datos; }
        }

       /** Method:  Sample data */
        internal List<double> Valores {
            get { return valores; }
        }

       /** Method:  Sample data frequencies */
        internal List<double> Frecuencias {
            get { return frecuencias; }
        }

       /** Method:  Histogram of sample distribution */
        internal Histogram Histogram {
            get { return histogram; }
        }

       /** Method:  Mean of sample data */
        internal double Mean {
            get { return mean; }
        }

        /** Method:  Variance of sample data */
        internal double Var {
            get { return var; }
        }

        /** Method:  Get a certain datum by its index */
        internal double GetDato(int i) {
            return (double)datos[i];
        }

        /** Method:  Get a certain datum by its index */
        internal double GetValor(int i) {
            return (double)valores[i];
        }

        /** Method:  Get a certain frequency by its index */
        internal int GetFrecuencia(int i) {
            return (int)frecuencias[i];
        }

        #endregion

        #region Internal Methods

        /** Method:  Load data for calculation 
        datos -  list of input data 
        weights -  array of weights to apply 
        random -  if random will be applied */
        internal void LoadData(List<double> datos, double[] weights, bool random) {
            this.epsilon = 0.00000000001;
            this.n = datos.Count;
            this.datos = datos;
            int dato;
            int index;
            SDict<int, double> sortFreqs = new SDict<int, double>();
            for(int i=0;i<datos.Count;i++) {
                if(random) { index = randGen.NextInt(0, datos.Count-1); } 
                else { index = i; }
                dato = Convert.ToInt32(datos[index]);
                if(dato < 0) { dato = 0; }
                if(!sortFreqs.ContainsKey(dato)) { sortFreqs.Add(dato, weights[index]*10.0); } 
                else { sortFreqs[dato] += weights[index]*10.0; }
            }
            List<int> removeKeys = new List<int>();
            double min = double.MaxValue;
            double max = double.MinValue;
            foreach (int key in sortFreqs.Keys) {
                if (sortFreqs[key] < epsilon) { 
                    removeKeys.Add(key); 
                } 
                else {
                    if (key < min) { min = key; }
                    if (key > max) { max = key; }
                }
            }
            foreach (int key in removeKeys) {
                sortFreqs.Remove(key);
            }
            this.histogram.Clear();
            this.histogram.LoadDist(sortFreqs, min, max);
        }

       /** Method:  Calculate statistics of data */
        internal void SetStatistics() {
            double[] valores = new double[datos.Count];
            for(int i=0;i<valores.Length;i++) {
                valores[i] = (double)datos[i];
            }
            
            mean = stat.Mean(valores,datos.Count,0);
            var =  stat.VarMuestral(valores,datos.Count,0);
        }


        #endregion

        #region Generar Archivo

       /** Method:  If a file should be generated 
        nombre -  path of the archive */
        internal void GenerarArchivo(string nombre) {
            System.IO.StreamWriter fs = new StreamWriter("..\\arch\\" + nombre + ".xls",false);
            foreach(double dato in datos) { 
                fs.WriteLine(dato.ToString("##0.000000000"));
            }
            fs.Close();
        }

        #endregion

        #region Enums

       /** Method:  Type of distribution */
        internal enum Dist { Uniform, Normal, Exponential };

        #endregion
     }
}
