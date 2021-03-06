#region Imports

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Maths;

#endregion

namespace MachineLearning {

    internal class Predictability {

        #region Fields

        private TDNNFcst tdnn;
        private int mw;
        private int ciclos;
        private int fcstHorizon;
        private double umbral;
        private int nUltPeriodos;
        private double fc;

        #endregion

        #region Constructor

        internal Predictability(int mw, int ciclos, int fcstHorizon, double umbral, int nUltPeriodos, double fc) {
            this.mw = mw;
            this.ciclos = ciclos;
            this.fcstHorizon = fcstHorizon;
            this.umbral = umbral;
            this.nUltPeriodos = nUltPeriodos;
            this.fc = fc;
        }

        #endregion

        #region Internal Methods

        internal bool IsForecastable(List<double> serie) {
            if (serie.Count < mw * 2) { return false; }
            if (GetFc(serie) < fc) { return false; }
            if (HasUltPerZero(serie)) { return false; }
            return HowForecastable(serie) >= umbral;
        }

        internal double HowForecastable(List<double> serie) {
            return HowForecastable(serie, true);
        }

        internal double HowForecastable(List<double> serie, bool trunk) {
            if (serie.Count < fcstHorizon) { throw new Exception("El_horizonte_de_prediccion_no_puede_ser_mayor_que_el_largo_de_la_serie"); }

            double[] real = new double[fcstHorizon];
            int size = serie.Count;
            double maxError = 0.0;
            for (int i = 0; i < fcstHorizon; i++) {
                real[i] = (double)serie[size - fcstHorizon + i];
                maxError += real[i];
            }
            List<double> datos = new List<double>();
            datos.AddRange(serie);
            datos.RemoveRange(size - fcstHorizon, fcstHorizon);
            tdnn = new TDNNFcst(6, 30);
            ((ITsForecast)tdnn).LoadData(datos, 0);
            tdnn.Train(ciclos);
            double[] fcst = tdnn.GetFcst(fcstHorizon);
            double error = GetLagError(fcst, real, 1);
            if (!trunk) { return error; }
            double relError = error / maxError;
            if (relError > 1) { relError = 1; }
            double forecastable = 1.0 - relError;
            return forecastable;
        }

        #endregion

        #region Private Methods

        private bool HasUltPerZero(List<double> serie) {
            for (int i = serie.Count - nUltPeriodos; i < serie.Count; i++) {
                if ((double)serie[i] != 0) { return false; }
            }
            return true;
        }

        private double GetFc(List<double> serie) {
            int totNoZero = 0;
            foreach (double val in serie) {
                if (val != 0) { totNoZero++; }
            }
            return (double)totNoZero / (double)serie.Count;
        }

        private double GetLagError(double[] fcst, double[] real, int lag) {
            double errNorm = GetError(fcst, real, lag, lag, fcst.Length - lag);
            double errLag = GetError(fcst, real, 0, lag, fcst.Length - lag);
            return Math.Min(errNorm, errLag);
        }

        private double GetError(double[] fcst, double[] real) {
            return GetError(fcst, real, 0, 0, fcst.Length);
        }

        private double GetError(double[] fcst, double[] real, int index1, int index2, int nDatos) {
            double error = 0.0;
            for (int i = 0; i < nDatos; i++) {
                error += Math.Abs(fcst[index1 + i] - real[index2 + i]);
            }
            return error;
        }

        #endregion

    }
}
