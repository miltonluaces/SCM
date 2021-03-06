#region Fields

using System;
using System.Collections.Generic;
using System.Text;
using Maths;

#endregion

namespace Statistics {

    /** Method:  Class for outlier detection */
    internal class OutlierDetection {

        #region Fields

        private double[] xArr = { 90.0, 90.5, 91.0, 91.5, 92.0, 92.5, 93.0, 93.5, 94.0, 94.5, 95.0, 95.5, 96.0, 96.5, 97.0, 97.5, 98.0, 98.5, 99.0, 99.3, 99.5, 99.7, 99.9 };
        private double[] yArr = { 1.88599968, 1.95, 2.05, 2.15, 2.24, 2.3, 2.41, 2.4, 2.55, 2.65, 2.919998884, 3.1, 3.3, 3.5, 3.8, 4.302995682, 4.8, 5.6, 6.964999676, 8.2, 9.925000668, 13.0, 22.32700014 };

        private Splines sp;
        private StatFunctions stat;

        #endregion

        #region Constructor

        /** Method:  Constructor */
        internal OutlierDetection()
        {
            this.sp = new Splines();
            this.stat = new StatFunctions();
        }

        #endregion

        #region Grubbs Test

        /** Method:  Test que indica si hay al menos un outlier en la serie dada */
        internal bool GrubbsTest(IList<double> datos, double prob)
        {
            int minLenghtForOutliers = 3;
            if (datos.Count <= minLenghtForOutliers) { return false; }
            //determinacion del valor G = máximo valor (val-m)/stDev
            double G = 0.0;
            double mean = stat.Mean(datos);
            double stDev = stat.StDev(datos);
            double g;
            foreach (double val in datos)
            {
                g = (val - mean) / stDev;
                if (g > G) { G = g; }
            }
            double k = GetCriticalValue(prob);

            //determinacion del valor critico (cv);
            int n = datos.Count;
            double cv = ((n - 1) / Math.Sqrt(n)) * Math.Sqrt(Math.Pow(k, 2) / (n - 2 + Math.Pow(k, 2)));

            //comparacion final
            //Console.WriteLine("G = "+ G + " , cv = " + cv + " , n = " + n + " , prob = " + prob);
            return G > cv;
        }

        /** Method:  Test de identificacion de outliers individual*/
        internal bool GrubbsTest(IList<double> datos, int index, double prob, double margen) {

            int minLenghtForOutliers = 3;
            if (datos.Count <= minLenghtForOutliers) { return false; }

            //determinacion del valor G = (val-m)/stDev
            double mean = stat.Mean(datos);
            double stDev = stat.StDev(datos);
            double G = (datos[index] - mean) / stDev;

            double k = GetCriticalValue(prob);
            //Console.WriteLine("df = " + df);

            //determinacion del valor critico (cv);
            int n = datos.Count;
            double cv = ((n - 1) / Math.Sqrt(n)) * Math.Sqrt(Math.Pow(k, 2) / (n - 2 + Math.Pow(k, 2)));

            //comparacion final
            return G / cv > 1 - margen / 100.0;
        }


        /** Method:  Test de identificacion de outliers individual sobre Frecuencias */
        internal bool GrubbsTest(Frequencies frec, double value, double prob, double margen) {

            if (value < 0.0)
                return true;
            int minLenghtForOutliers = 3;
            if (frec.NValues <= minLenghtForOutliers) { return false; }

            //determinacion del valor G = (val-m)/stDev

            int nValues = frec.NValues + 1;
            double mean = frec.Mean;
            double stDev = frec.StDev;

            double G = (value - mean) / stDev;

            double k = GetCriticalValue(prob);
            //Console.WriteLine("df = " + df);

            //determinacion del valor critico (cv);
            double cv = ((nValues - 1) / Math.Sqrt(nValues)) * Math.Sqrt(Math.Pow(k, 2) / (nValues - 2 + Math.Pow(k, 2)));

            //comparacion final
            return G / cv > 1 - margen / 100.0;
        }
   
        /** Method:  Cargar splines */
        internal void LoadSplines() {
            //determinacion del df para t de student n-2 grados de libertad
            List<double> Xdata = new List<double>();
            Xdata.AddRange(xArr);
            List<double> Ydata = new List<double>();
            Ydata.AddRange(yArr);
            sp = new Splines(Xdata, Ydata, true);
        }

        private double GetCriticalValue(double prob)
        {

            prob = Math.Round(prob, 1);

            double x = prob;
            double y = 0.0;
            double x1 = 0;
            double y1 = 0;
            double x2 = 0;
            double y2 = 0;
            for (int j = 0; j < xArr.Length; j++)
            {
                if (xArr[j] == x) { y = yArr[j]; return y; }
                else if (xArr[j] > x)
                {
                    x1 = xArr[j - 1];
                    x2 = xArr[j];
                    y1 = yArr[j - 1];
                    y2 = yArr[j];
                    if (sp == null) { LoadSplines(); }
                    y = sp.Interpolar(x, x1, y1, x2, y2);
                    return y;
                }
            }
            throw new Exception("No pude interpolarse el valor " + prob);
        }

        #endregion

        #region Studentized deletion residuals

        #region internal Methods

        /** Method:  Standardized residuals */
        internal List<double> StdRes(IList<double> serie, IList<double> model)
        {
            if (serie.Count != model.Count) { throw new Exception("must have the same size as model"); }

            double s = Math.Sqrt(Mse(serie, model));
            List<double> stdres = new List<double>();
            for (int i = 0; i < serie.Count; i++) { stdres.Add((serie[i] - model[i]) / s); }
            return stdres;
        }

        /** Method:  Studentized residuals */
        internal List<double> StudRes(IList<double> serie, IList<double> model)
        {
            if (serie.Count != model.Count) { throw new Exception("must have the same size as model"); }

            List<double> studres = new List<double>();
            List<double> stdres = StdRes(serie, model);
            List<double> hat = Hat(serie, model);
            for (int i = 0; i < serie.Count; i++) { studres.Add(stdres[i] / Math.Sqrt(1 - hat[i])); }
            return studres;
        }

        /** Method:  Studentized deletion residuals */
        internal List<double> StudDelRes(IList<double> serie, IList<double> model)
        {
            if (serie.Count != model.Count) { throw new Exception("must have the same size as model"); }

            List<double> studDelRes = new List<double>();
            List<double> r = StudRes(serie, model);
            List<double> hat = Hat(serie, model);
            double n = serie.Count;
            for (int i = 0; i < n; i++)
            {
                if (n - 2 - Math.Pow(r[i], 2) < 0) { studDelRes.Add(double.MaxValue); } //TODO: revisar
                else { studDelRes.Add(r[i] * Math.Sqrt((n - 3) / (n - 2 - Math.Pow(r[i], 2)))); }
            }
            //for(int i=0;i<n;i++) { studDelRes.Add((serie[i]-model[i]) / (1-hat[i])); } //TODO: revisar si el numerador es r o el raw residual
            //for(int i=0;i<n;i++) { studDelRes.Add(r[i]/ (1-hat[i])); }
            return studDelRes;
        }

        /** Method:  Limit for outlier filtering with studentized deletion residuals */
        internal double StudDelResLimit(IList<double> serie, IList<double> model, double alpha)
        {
            return stat.TStudent_quantil(alpha, serie.Count, true);
        }

        /** Method:  List of indexes of detected outliers */
        internal List<int> StudDelResOutliers(IList<double> serie, IList<double> model, double alpha)
        {
            List<int> outliers = new List<int>();
            List<double> studDelRes = StudDelRes(serie, model);
            double limit = StudDelResLimit(serie, model, alpha);
            for (int i = 0; i < studDelRes.Count; i++)
            {
                if (Math.Abs(studDelRes[i]) > limit) { outliers.Add(i); }
            }
            return outliers;
        }

        /** Method:  If it is an outlier according to standardized deletion resiuduals */
        internal bool IsStudDelResOutlier(IList<double> serie, IList<double> model, double alpha, double value, double modelValue)
        {
            serie.Add(value);
            model.Add(modelValue);
            List<double> studDelRes = StudDelRes(serie, model);
            double limit = StudDelResLimit(serie, model, alpha);
            if (Math.Abs(studDelRes[studDelRes.Count - 1]) > limit) { return true; }
            return false;
        }

        /** Method:  Leverage values */
        internal List<double> Leverage(IList<double> serie, IList<double> model)
        {
            if (serie.Count != model.Count) { throw new Exception("must have the same size as model"); }
            List<double> hat = Hat(serie, model);
            List<double> lev = new List<double>();
            for (int i = 0; i < serie.Count; i++) { lev.Add(1 - hat[i]); }
            return lev;
        }

        #endregion

        #region Private Methods

        private double Sse(IList<double> serie, IList<double> model)
        {
            if (serie.Count != model.Count) { throw new Exception("must have the same size as model"); }

            double sse = 0.0;
            for (int i = 0; i < serie.Count; i++) { sse = sse + Math.Pow(serie[i] - model[i], 2); }
            return sse;
        }

        private double Mse(IList<double> serie, IList<double> model)
        {
            if (serie.Count != model.Count) { throw new Exception("must have the same size as model"); }
            double sse = Sse(serie, model);
            return sse / (serie.Count - 2);
        }

        private List<double> Hat(IList<double> serie, IList<double> model)
        {
            if (serie.Count != model.Count) { throw new Exception("must have the same size as model"); }

            double sse = Sse(serie, model);
            List<double> hat = new List<double>();
            for (int i = 0; i < serie.Count; i++) { hat.Add(1 / serie.Count + Math.Pow(serie[i] - model[i], 2) / sse); }
            return hat;
        }

        #endregion

        #endregion

    }
}
