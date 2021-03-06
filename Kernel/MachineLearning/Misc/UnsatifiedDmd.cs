#region Imports

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Maths;
using Statistics;

#endregion

namespace MachineLearning {

    internal class UnsatifiedDmd {

        #region Fields

        private int horizon;
        private int movingWindow;
        private Functions func;
        private StatFunctions stat;
        private MethodType method;
        private OutlierDetection outDet;
        private double alpha;
        private double leftLFThreshold;

        #endregion

        #region Constructor

        internal UnsatifiedDmd() {
            func = new Functions();
            stat = new StatFunctions();
            movingWindow = 3;
            method = MethodType.stEstim;
            outDet = new OutlierDetection();
            alpha = 0.05;
            leftLFThreshold = 0.6;
        }

        #endregion

        #region Properties

        internal int MovingWindow  {
            get { return movingWindow; }
            set { movingWindow = value; }
        }

        internal int Horizon  {
            get { return horizon; }
            set { horizon = value; }
        }

        internal double Alpha  {
            get { return alpha; }
            set { alpha = value; }
        }

        internal MethodType Method  {
            get { return method; }
            set { method = value; }
        }

        internal double LeftLFThreshold  {
            get { return leftLFThreshold; }
            set { leftLFThreshold = value; }
        }
        
        #endregion

        #region Internal Methods

        internal List<double> Calculate(List<double> hist, List<double> stock)  {
            if (hist.Count < horizon || stock.Count < horizon) {
                List<double> usd = new List<double>();
                for (int i = 0; i < hist.Count; i++) { usd.Add(0); }
                return usd; 
            }
            switch (method) { 
                case MethodType.basic:  return CalculateMovingAvg(hist, stock);
                case MethodType.stEstim:  return CalculateStDevEstim(hist, stock);
            }
            return null;
        }

        #endregion

        #region Private Methods

        private List<double> CalculateMovingAvg(List<double> hist, List<double> stock) {
            List<double> ma = func.MovingAverage(hist, movingWindow, true);
            if (hist.Count != stock.Count) { throw new Exception("Error"); }

            int stockouts = 0;
            List<double> usd = new List<double>();
            for (int i = 0; i < hist.Count; i++) {
                if (hist[i] == 0 && stock[i] == 0 && i > movingWindow) { 
                    usd.Add(Math.Round(ma[i], 2));
                    stockouts++; 
                } 
                else {
                    usd.Add(0);
                }
            }
            return usd;
        }

        private List<double> CalculateStDevEstim(List<double> hist, List<double> stock) {

            //mobile mean, stDev
            List<double> mean = func.MovingAverage(hist, movingWindow, true);
            List<double> stDev = MovingStDevCen(hist, movingWindow);

            //outlier filtering
            List<double> studDelRes = outDet.StudDelRes(hist, mean);
            double limit = Math.Abs(outDet.StudDelResLimit(hist, mean, alpha));
            List<int> filteredIndexes = new List<int>();
            for (int i = 0; i < hist.Count; i++) {
                if (hist[i] > 0 && Math.Abs(studDelRes[i]) > limit) {
                    hist[i] = 0;
                    filteredIndexes.Add(i);
                } 
            }

            if (filteredIndexes.Count > 0) {
                mean = func.MovingAverage(hist, movingWindow, true);
                stDev = MovingStDevCen(hist, movingWindow);
                foreach (int index in filteredIndexes) { hist[index] = mean[index]; }
            }

            //U estimation
            double[] U = new double[stock.Count];
            double[] S = new double[stock.Count];
            for (int t = 0; t < stock.Count; t++) {
                U[t] = (stock[t] - mean[t]) / stDev[t];
                S[t] = mean[t] + E(U[t]) * stDev[t];
            }
            
            //lost sales estimation
            int stockouts = 0;
            double s;
            int halfMw = (int)(movingWindow/2.0);
            List<double> usd = new List<double>();
            for (int i = 0; i < hist.Count; i++) {
                if (stock[i] == 0 && hist[i] >= 0 && i >= movingWindow && LeftLoadFactor(hist, i, halfMw) > leftLFThreshold) {
                    s = S[i] - hist[i];
                    if (s > 0) { usd.Add(s); } 
                    else { usd.Add(0); }
                    stockouts++;
                } 
                else {
                    usd.Add(0);
                }
            }
            return usd;
        }

     
        private double E(double U) {
            if (U < -1.2) { return 0.23 * U + 0.652; } else if (U < -0.1) { return 0.50 * U + 0.965; } else if (U < 1.2) { return 0.72 * U + 0.996; } else { return 0.87 * U + 0.803; }
        }

        public List<double> MovingStDevCen(List<double> values, int mw) {
            List<double> movingStDev = new List<double>();
            int ini = (int)mw / 2;
            int fin;
            double stDev = 0;
            for (int i = 0; i < ini; i++) {
                stDev = stat.StDev(values, 0, mw - 1);
                movingStDev.Add(stDev);
            }
            for (int i = ini; i < values.Count - ini; i++) {
                fin = (mw % 2 == 0) ? i + ini : i + ini + 1;
                if (fin >= values.Count) { fin = values.Count - 1; }
                stDev = stat.StDev(values, ini, fin);
                movingStDev.Add(stDev);
            }
            stDev = stat.StDev(values, values.Count - mw, values.Count - 1);
            for (int i = values.Count - (mw - ini) - 1; i < values.Count; i++) {
                movingStDev.Add(stDev);
            }
            return movingStDev;
        }


        private double GetReplacementValue(List<double> serie, int index) {
            if (index - 3 < 0 || index + 3 > serie.Count - 1) { return 1; }
            serie[index] = 0;
            return stat.Mean(serie, index-3, index + 3);
        }

        private double LeftLoadFactor(List<double> serie, int index, int halfMw) {
            if (index < halfMw) { return 0; }
            int nonZeros = 0;
            for (int i = index-halfMw; i < index; i++) {
                if (serie[i] > 0) { nonZeros++; }
            }
            return (double)nonZeros / (double)halfMw;
        }

        #endregion

        #region Enums

        internal enum MethodType { basic, stEstim };

        #endregion
    }
}
