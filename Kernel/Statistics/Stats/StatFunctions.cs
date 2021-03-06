#region Imports

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Maths;

#endregion

namespace Statistics {

    internal class StatFunctions   {

        #region Fields

        private Functions func;
        private Polynom polynom;

        
		private double ITMAX;
		private double EPS;
		private double FPMIN;
		private double M_LN_SQRT_2PI;
		private double M_2PI;
		private double S0; // 1/12
        private double S1; // 1/360
		private double S2; // 1/1260
		private double S3; // 1/1680 
        private double S4; // 1/1188
		private double [] a;
		private double [] b;
        private double [] c;
		private double [] d;

	    #endregion

        #region Constructor

        /*Constructor*/
        internal StatFunctions() {

            func = new Functions();
            polynom = new Polynom();

   	        ITMAX = 100;
		    EPS = (double) 3.0e-7;
		    FPMIN = (double ) 1.0e-20;
		    M_LN_SQRT_2PI = (double) 0.91893853320467274178032973;
		    M_2PI = (double) 6.28318530717958647692528676;
		    S0 = 0.083333333333333333333;        
		    S1 = 0.00277777777777777777778;      
		    S2 = 0.00079365079365079365079;   
		    S3 = 0.00059523809523809523809; 
		    S4 = 0.00084175084175084175084;

		    a = new double[6];
            a[0] = -3.969683028665376e+01; a[1] = 2.209460984245205e+02; a[2] = -2.759285104469687e+02; a[3] =  1.383577518672690e+02; a[4] = -3.066479806614716e+01; a[5] = 2.506628277459239e+00;
		    b = new double[5];
            b[0] = -5.447609879822406e+01; b[1] = 1.615858368580409e+02; b[2] = -1.556989798598866e+02; b[3] =  6.680131188771972e+01; b[4] = -1.328068155288572e+01;
            c = new double[6];
            c[0] = -7.784894002430293e-03; c[1] = -3.22396458041136e-01; c[2] = -2.400758277161838e+00; c[3] = -2.549732539343734e+00; c[4] = 4.374664141464968e+00; c[5] = 2.938163982698783e+00;
            d = new double[4];
            d[0] =  7.784695709041462e-03; d[1] = 3.224671290700398e-01; d[2] =	 2.445134137142996e+00; d[3] =  3.754408661907416e+00;

	    }

        #endregion

        #region internal Methods

        #region Classic Statistics

        #region Basic Functions

        internal double Mean(IList<double> values) {
            if(values.Count == 0) { return 0.0; }
            
            double tot = 0.0;
            for(int i=0;i<values.Count;i++) {
               tot += values[i];
            }
            return tot/values.Count;
        }

        internal double Variance(IList<double> values) {
            int n = values.Count;
            if(n == 1) { return 0; }
            double Sum = 0.0, SumSquares = 0.0;

            for(int i=0;i<n;i++) {
                Sum += values[i];
                SumSquares += values[i] * values[i];
            }
            return (n * SumSquares - (Sum * Sum)) / (n*n - 1);
        }

        internal double Range(IList<double> values) {
            double min = double.MaxValue;
            double max = double.MinValue;
            foreach (double val in values) {
                if (val < min) { min = val; }
                if (val > max) { max = val; }
            }
            return max - min;
        }

        internal double StDev(IList<double> values) {
            return Math.Sqrt(Variance(values));
        }

        internal double VarCoeff(IList<double> values) {
            if(values.Count <= 1) { return 0; }
            double mean = Mean(values);
            if(mean == 0) { return 1; }
            double varCoeff =  Math.Abs(StDev(values)/ mean);
            return varCoeff; 
        }

        internal double Variance(int count, double sum, double sqSum) {
            if(count <= 1 || sum == 0) { return 0.0; } 
            return (count * sqSum - (sum * sum)) / (count*count - 1);
        }

        internal double StDev(int count, double sum, double sqSum) {
            double variance = Variance(count, sum, sqSum);
            if(variance < 0.01) { return 0.0; }
            return Math.Sqrt(variance);  
        }

        internal double StdError(IList<double> values) {
            if(values.Count == 0) { throw new Exception("empty list"); }
            return StDev(values)/Math.Sqrt(values.Count);
        }

        internal double MSError(IList<double> data1, IList<double> data2) {
            if(data1.Count != data2.Count) { throw new Exception("s1 must be of the same size than s2"); }

            double tot = 0.0;
            for(int i=0;i<data1.Count;i++) {
                tot += Math.Pow((data1[i] - data2[i]),2);
            }
            return Math.Sqrt(tot)/(double)data1.Count;
        }

        internal double MSError(IList<double> data1, IList<double> data2, double percCheck) {
            if(data1.Count > data2.Count) {
                IList<double> aux = data1;
                data1 = data2;
                data2 = aux;
            }
            int firstIndex = data2.Count - (int)(percCheck * 0.01 * data2.Count); 
            double tot = 0.0;
            for(int i=firstIndex;i<data1.Count;i++) {
                tot += Math.Pow((data1[i] - data2[i]), 2);
            }
            for(int i=data1.Count;i<data2.Count;i++) {
                tot += Math.Pow(data2[i], 2);
            }
            return Math.Sqrt(tot)/(double)(data2.Count - firstIndex);
        }

        internal double Cov(IList<double> data1, IList<double> data2) {
            if(data1.Count != data2.Count) { throw new Exception("both must have same size"); }

            double mean1 = Mean(data1);
            double mean2 = Mean(data2);
            double sumProds = 0;
            for(int i = 0;i < data1.Count;i++) {
                sumProds += ((data1[i] - mean1) * (data2[i] - mean2));
            }
            return sumProds/data1.Count;
        }

        internal double R(IList<double> data1, IList<double> data2) {
            if(StDev(data1) * StDev(data2) == 0) { return 0; }

            return Cov(data1, data2) / (StDev(data1) * StDev(data2));
        }

        internal double R2(IList<double> data1, IList<double> data2) {
            return Math.Pow(R(data1, data2), 2);
        }

        internal double AdjR2(IList<double> data1, IList<double> data2, double p) {
            if (data1.Count != data2.Count) { throw new Exception("both must have same size"); }
            int n = data1.Count;
            double r2 = R2(data1, data2);
            return 1 - (1-r2) * (n-1)/(n-p-1);
        }
        
        #endregion

        #region Interval functions

        internal double Mean(IList<double> values, int ini, int end) {
            if (values.Count == 0) { return 0.0; }
            if (ini < 0 || ini >= end || end > values.Count) { throw new Exception("Error"); }

            double tot = 0.0;
            for (int i = ini; i <= end; i++) {
                tot += values[i];
            }
            return tot / (end - ini + 1);
        }

        internal double Variance(IList<double> values, int ini, int end) {
            if (ini < 0 || ini >= end || end > values.Count) { throw new Exception("Error"); }

            int n = end - ini + 1;
            if (n == 1) { return 0; }
            double Sum = 0.0, SumSquares = 0.0;

            for (int i = ini; i <= end; i++) {
                Sum += values[i];
                SumSquares += values[i] * values[i];
            }
            return (SumSquares - (Sum * Sum)/n) / (n - 1);
        }

        internal double Range(IList<double> values, int ini, int end) {
            if (ini < 0 || ini >= end || end > values.Count) { throw new Exception("Error"); }

            double min = double.MaxValue;
            double max = double.MinValue;
            for(int i=ini;i<=end;i++) {
                if (values[i] < min) { min = values[i]; }
                if (values[i] > max) { max = values[i]; }
            }
            return max - min;
        }

        internal double StDev(IList<double> values, int ini, int end) {
            return Math.Sqrt(Variance(values, ini, end));
        }

        internal double Cov(IList<double> data1, IList<double> data2, int ini, int end) {
            if (data1.Count != data2.Count) { throw new Exception("both must have same size"); }
            if (ini < 0 || ini >= end || end > data1.Count) { throw new Exception("Error"); }

            double mean1 = Mean(data1, ini, end);
            double mean2 = Mean(data2, ini, end);
            double sumProds = 0;
            for (int i = ini; i <= end; i++) {
                sumProds += ((data1[i] - mean1) * (data2[i] - mean2));
            }
            return sumProds / (end - ini + 1);
        }

        internal double R(IList<double> X, IList<double> Y, int ini, int end) {
            double m1 = Mean(X, ini, end);
            double m2 = Mean(Y, ini, end);
            double s1 = StDev(X, ini, end);
            double s2 = StDev(Y, ini, end);
            if (s1 * s2 == 0) {
                if(m1 == m2) { return 1; }
                else { return 0; } 
            }

            double n = end - ini + 1;

            double sX = 0;
            double sY = 0;
            double sX2 = 0;
            double sY2 = 0;
            double sXY = 0;
            for (int i = ini; i <= end; i++) {
                sX += X[i];
                sX2 += (X[i] * X[i]);
                sY += Y[i];
                sY2 += (Y[i] * Y[i]); 
                sXY += (X[i] * Y[i]); 
                
            }

            double r = (n * sXY - sX * sY) / Math.Sqrt((n * sX2 - Math.Pow(sX, 2)) * (n * sY2 - Math.Pow(sY, 2)));
            return r;

            /*
            double r = Cov(data1, data2, ini, end) / (s1 * s2);
            if (r < -1) { r = -1.0;  }
            else if (r > 1) { r = 1.0; }
            return r;
            */
        }

        internal double R2(IList<double> data1, IList<double> data2, int ini, int end) {
            return Math.Pow(R(data1, data2, ini, end), 2);
        }

        internal List<double> R2Mw(IList<double> data1, IList<double> data2, int mw) {
            if (data1.Count != data2.Count) { throw new Exception("both must have same size"); }
            
            List<double> r2List = new List<double>();
            double r2;
            for (int i = 0; i < data1.Count-mw; i++) {
                r2 = R2(data1, data2, i, i + mw);
                r2List.Add(r2);
            }
            return r2List;
        }

        internal List<double> R2ToEnd(IList<double> data1, IList<double> data2) {
            if (data1.Count != data2.Count) { throw new Exception("both must have same size"); }

            List<double> r2List = new List<double>();
            double r2;
            for (int i = 0; i < data1.Count-1; i++) {
                r2 = R2(data1, data2, i, data1.Count);
                r2List.Add(r2);
            }
            return r2List;
        }

        internal double R(IList<double> data1, IList<double> data2, int ini, int end, int lag) {
            if(ini + lag >= end) { return 0; }
            List<double> data1Lag = new List<double>(data1);
            data1Lag.RemoveRange(0, lag);
            List<double> data2Lag = new List<double>(data2);
            data2Lag.RemoveRange(data2.Count-lag, lag);
            return R(data1Lag, data2Lag, ini, end-lag);
        }

        internal double R2(IList<double> data1, IList<double> data2, int ini, int end, int lag) {
            return Math.Pow(R(data1, data2, ini, end, lag), 2);
        }

        internal List<double> R2Mw(IList<double> data1, IList<double> data2, int mw, int lag) {
            if (data1.Count != data2.Count) { throw new Exception("both must have same size"); }
            List<double> data1Lag = new List<double>(data1);
            data1Lag.RemoveRange(0, lag);
            List<double> data2Lag = new List<double>(data2);
            data2Lag.RemoveRange(data2.Count - lag, lag);
       
            List<double> r2List = new List<double>();
            double r2;
            for (int i = 0; i < data1Lag.Count - mw; i++) {
                r2 = R2(data1Lag, data2Lag, i, i + mw);
                r2List.Add(r2);
            }
            return r2List;
        }

        internal int FirstR2AboveThres(List<double> r2List, double threshold) {
            if(r2List[r2List.Count-1] < threshold) { return -1; }
            for (int i = r2List.Count - 2; i > 0; i--) {
                if (r2List[i] < threshold) { return i + 1; }
            }
            return 0;
        }

        #endregion

        #region Basic functions in arrays

        internal double MeanWithout(IList<double> datos, int indice) {
            double total = 0.0;
            for(int i=0;i<datos.Count;i++) {
                if(i != indice) {
                    total += datos[i];
                }
            }
            return total / (datos.Count - 1);
        }

        internal double StDevWithout(IList<double> datos, int indice) {
            double total = 0.0;
            double tot2 = 0.0;

            for(int i=0;i<datos.Count;i++) {
                if(i != indice) {
                    total += datos[i];
                    tot2 += Math.Pow(datos[i], 2);
                }
            }
            return Math.Sqrt(((datos.Count - 1) * tot2 -  Math.Pow(total, 2)) / ((datos.Count - 1) * (datos.Count - 2)));

        }

        internal double BiasWithout(IList<double> datos, int indice) {
            double mean = MeanWithout(datos, indice);
            return Math.Abs(datos[indice] - mean);
        }

        internal double IntervalMean(IList<double> datos, int indice, int r) {
            double total = 0.0;
            int n = 0;
            for(int i=indice-r;i<=indice+r;i++) {
                if(i != indice && i >= 0 && i < datos.Count) {
                    total += datos[i];
                    n++;
                }
            }
            return total / n;
        }

        internal double IntervalBias(IList<double> datos, int indice, int r) {
            double mean = IntervalMean(datos, indice, r);
            return Math.Abs(datos[indice] - mean);
        }

        #endregion

        #endregion

        #region Robust statistics

        #region Trimmed statistics 

        internal List<double> Trim(List<double> values, double cutoff, bool sort) {
            if(sort) { values.Sort(); }
            int half = Convert.ToInt32((double)values.Count/2);
            int nCutoff = Convert.ToInt32((double)half * cutoff);
            int nValues = values.Count - nCutoff*2;
            if(nValues == 0) { return values; }
            List<double> trimValues = new List<double>();
            for(int i = nCutoff;i < nValues-nCutoff;i++) { trimValues.Add(values[i]); }
            return trimValues;
        }

        internal double TrimRange(List<double> values, double cutoff, bool sort) {
            List<double> trimValues = Trim(values, cutoff, sort);
            if (values.Count == 0) { return 0.0; }
            return Range(trimValues);
        }

        internal double TrimRange(List<double> values, int ini, int end, double cutoff, bool sort) {
            List<double> trimValues = Trim(values, cutoff, sort);
            if (values.Count == 0) { return 0.0; }
            return Range(trimValues);
        }

        internal double TrimMean(List<double> values, double cutoff, bool sort) {
            List<double> trimValues = Trim(values, cutoff, sort);
            if(values.Count == 0) { return 0.0; }
            return Mean(trimValues);
        }

        internal double TrimVar(List<double> values, double cutoff, bool sort) {
            List<double> trimValues = Trim(values, cutoff, sort);
            if(trimValues.Count <= 1) { return 0.0; }
            return Variance(trimValues);
        }

        internal double TrimSd(List<double> values, double cutoff, bool sort) {
            List<double> trimValues = Trim(values, cutoff, sort);
            if(trimValues.Count <= 1) { return 0.0; }
            return StDev(trimValues);
        }

        internal double TrimMeanNonZero(List<double> values, double rec) {
            int mitad = Convert.ToInt32((double)values.Count/2);
            int nRec = Convert.ToInt32((double)mitad * rec);
            int nValues = 0;
            double sum = 0.0;
            for(int i = nRec;i < values.Count-nRec;i++) {
                if(values[i] != 0) {
                    sum += values[i];
                    nValues++;
                }

            }
            if(nValues == 0) { return 0; }
            return sum/nValues;
        }
        
        #endregion

        #region Windsored statistics 

        internal double WinMean(double[] values, double rec) {
            int mitad = Convert.ToInt32((double)values.Length/2);
            int nRec = Convert.ToInt32((double)mitad * (1 -rec));
            int nValues = values.Length;
            if(nValues == 0) { return 0.0; }
            double sum = 0.0;
            for(int i = 0;i<nRec;i++) {
                sum += values[nRec];
            }
            for(int i = nRec;i<nValues-nRec;i++) {
                sum += values[i];
            }
            for(int i= nValues-nRec;i<nValues;i++) {
                sum += values[nValues-nRec-1];
            }
            return sum/nValues;
        }

        internal List<double> Windsor(List<double> values, double cutoff, bool sort) {
            List<double> valSort = values;
            if(sort) { 
                valSort = new List<double>(values); 
                valSort.Sort(); 
            }
            int half = Convert.ToInt32((double)valSort.Count/2);
            int nCutoff = Convert.ToInt32((double)half * cutoff);
            int nValues = valSort.Count - nCutoff*2;
            if(nValues == 0) { return values; }
            double lwr = valSort[nCutoff];
            double upr = valSort[valSort.Count-nCutoff-1];
            
            List<double> windValues = new List<double>();
            for(int i=0;i<values.Count;i++) {
                if (values[i] < lwr) { windValues.Add(lwr); } 
                else if (values[i] > upr) { windValues.Add(upr); } 
                else { windValues.Add(values[i]); }
            }
            return windValues;
        }

        internal double WindMean(List<double> values, double cutoff, bool sort) {
            List<double> windValues = Windsor(values, cutoff, sort);
            if(values.Count == 0) { return 0.0; }
            return Mean(windValues);
        }

        internal double WindVar(List<double> values, double cutoff, bool sort) {
            List<double> windValues = Windsor(values, cutoff, sort);
            if(values.Count <= 1) { return 0.0; }
            return Variance(windValues);
        }

        internal double WindSd(List<double> values, double cutoff, bool sort) {
            List<double> windValues = Windsor(values, cutoff, sort);
            if(values.Count <= 1) { return 0.0; }
            return StDev(windValues);
        }
        
        #endregion

        #region Weighted statistics

        internal double WeightedMean(IList<double> x, IList<double> w, bool wNormalized) { 
            if(x.Count != w.Count) { throw new Exception("Error. x and w must have same size"); }
            double wMean = 0;
            for (int i = 0; i < x.Count; i++) {  wMean += w[i] * x[i]; }
            if (wNormalized) { 
                return wMean; 
            }
            else {
                double sumW = func.Sum(w);
                return wMean / sumW;
            }
        }
        
        #endregion

        #endregion

        #region Entropy

        internal double CalcEntropy(IList<double> values) {
            
            NormalDistrib nd = new NormalDistrib();
            double mean = Mean(values);
            double stDev = StDev(values);
            List<double> stdValues = new List<double>();
            if(stDev == 0) { return 0.0; }
            foreach(double val in values) { stdValues.Add((val - mean)/stDev); }
            double p, info;
            double entropy = 0.0;
            foreach(double val in stdValues) {
                p = nd.pNorm(val - 0.5, val + 0.5, 0, 1);
                info = Math.Log(1.0/p);
                entropy += p * info;
            }
            return entropy;
        }

        #endregion

        #region Distributions

        #region t student

        internal double pt(double t, int df) {
            if(df <=0) { throw new Exception("Degrees of freedom must be zero"); }

            double epsilon = 0.00001;
            double result = 0;
            double x = 0;
            double rk = 0;
            double z = 0;
            double f = 0;
            double tz = 0;
            double p = 0;
            double xsqk = 0;
            int j = 0;

            if( t == 0.0) { return 0.5;  }
            if( t < -2.0) {
                rk = df;
                z = rk/(rk + t*t);
                result = 0.5 * Betai(0.5 * rk, 0.5, z);
                return result;
            }
            if( t < 0) { x = -t; }
            else { x = t; }
            rk = df;
            z = 1.0+x*x/rk;
            if(df%2!=0) {
                xsqk = x/Math.Sqrt(rk);
                p = Math.Atan(xsqk);
                if(df>1) {
                    f = 1.0;
                    tz = 1.0;
                    j = 3;
                    while(j<=df-2 & (double)(tz/f) > epsilon) {
                        tz = tz*((j-1)/(z*j));
                        f = f+tz;
                        j = j+2;
                    }
                    p = p + f*xsqk/z;
                }
                p = p*2.0/Math.PI;
            }
            else {
                f = 1.0;
                tz = 1.0;
                j = 2;
                while(j<=df-2 & (double)(tz/f) > epsilon) {
                    tz = tz*((j-1)/(z*j));
                    f = f+tz;
                    j = j+2;
                }
                p = f*x / Math.Sqrt(z*rk);
            }
            if(t < 0) { p = -p; }
            result = 0.5 + 0.5*p;
            return result;
        }

        internal double qt(double p, int k) {
            if(p <= 0 || p > 1) throw new Exception("Probability must be between 0 and 1");
          
            double maxReal = 1000000;
            double result = 0;
            double t = 0;
            double rk = 0;
            double z = 0;
            int rflg = 0;

            rk = k;
            if( p > 0.25 && p < 0.75) {
                if(p == 0.5) {
                    result = 0;
                    return result;
                }
                z = 1.0-2.0 * p;
                z = betacf(0.5, 0.5*rk, Math.Abs(z));
                t = Math.Sqrt(rk*z/(1.0-z));
                if( p < 0.5) { t = -t; }
                result = t;
                return result;
            }
            rflg = -1;
            if(p >= 0.5) {
                p = 1.0-p;
                rflg = 1;
            }
            z = betacf(0.5*rk, 0.5, 2.0*p);
            if( (double)(maxReal*z)< rk) {
                result = rflg * maxReal;
                return result;
            }
            t = Math.Sqrt(rk/z-rk);
            result = rflg*t;
            return result;
        }

        #endregion

        # region Beta, Gamma

        internal double B(double a, double b) {
            return (G(a) * G(b)) / G(a + b);
        }

        internal double B(IList<double> A) {
            double sumA = 0;
            double prodGA = 1;
            foreach (double a in A) {
                sumA += a;
                prodGA *= G(a); 
            }
            return prodGA / G(sumA);
        }

        internal double G(double k) {
            return Math.Exp(LogG(k));
        }

        internal double LogG(double k) {
            double x = k, y = k, tmp, ser;
            double[] cof =  {76.18009172947146, -86.50532032941677, 24.01409824083091, -1.2317395724500155, 0.1208650973866179e-2, -0.5395239384953e-5};

            tmp = x + 5.5;
            tmp -= (x + 0.5) * Math.Log(tmp);
            ser = 1.000000000190015;
            for (int j = 0; j <= 5; j++) ser += cof[j] / ++y;
            return -tmp + Math.Log(2.5066282746310005 * ser / x);
        }

        internal double BetaInc(double a, double b, double x) {
            return Betai(a, b, x);  //refactor to avoid static call
		}
        #endregion

        #endregion

        #region Homoscedasticity

        internal List<double> LogReturns(IList<double> serie, bool abs) {
            double min = double.MaxValue;
            foreach(double val in serie) {
                if(val < min) { min = val; }
            }
            if(min <= 0) {
                for(int i=1;i<serie.Count;i++) {
                    serie[i] = serie[i] - min + 1;
                }
            }
            List<double> logReturns = new List<double>();
            for(int i=1;i<serie.Count;i++) {
                if(serie[i] < 0) { serie[i] = 0; }
                if(abs) { logReturns.Add(Math.Abs(Math.Log(serie[i]) - Math.Log(serie[i-1]))); }
                else { logReturns.Add(Math.Log(serie[i]) - Math.Log(serie[i-1])); }
            }
            return logReturns;
        }

        internal List<double> WeightedVar(IList<double> serie, double s) {
            List<double> wv = new List<double>();
            for(int i=0;i<serie.Count;i++) {
                wv.Add(WeightedVar(serie, i, s)); 
            }
            return wv;
        }

        internal double WeightedVar(IList<double> serie, int index, double s) {
            NormalDistrib nd = new NormalDistrib();
            List<double> dist = new List<double>();
            double eps = 0.01;
            double w = 1;
            int m, x = 0;
            m = index;
            x = 0;
            while(w > eps && m-x >= 0) {
                w = nd.pNormDeriv(m-x, m, s);
                for(int i=0;i<w*100;i++) {
                    if(serie[m-x] < 0) { dist.Add(0); }
                    else { dist.Add(serie[m-x]); }
                }
                x++;
            }
            x = 0;
            while(w > eps && m+x < serie.Count) {
                w = nd.pNormDeriv(m+x, m, s);
                for(int i=0;i<w*100;i++) {
                    if(serie[m+x] < 0) { dist.Add(0); }
                    else { dist.Add(serie[m+x]); }
                }
                x++;
            }
            return Variance(dist);
        }

        #endregion

        #region ACF

        internal double Autocov(IList<double> serie, int lag) {
            double m = Mean(serie);
            double n = serie.Count;
            double ac = 0;
            for(int i=0;i<serie.Count-lag;i++) { ac += (serie[i] - m) * (serie[i+lag] - m); }
            return ac/n;
        }


        internal double Autocorr(IList<double> serie, int lag) {
            double m = Mean(serie);
            double ac = 0;
            double va = 0;
            double n = serie.Count;
            for(int i=0;i<n;i++) {
                if(i<n-lag) { ac += (serie[i] - m) * (serie[i+lag] - m); } 
                va += Math.Pow((serie[i] - m),2);
            }
            return ac/va;
        }

        internal List<double> ACF(IList<double> serie, int maxLag) {
            List<double> autocorrelation = new List<double>();
            for(int l=0;l<=maxLag;l++) {  autocorrelation.Add(Autocorr(serie, l)); }
            return autocorrelation;
        }

        internal int CalculateMaxAutocorrLag(List<double> serie, double minAutoCorr, int minLag, int maxLag, double epsilon) {
            double bestAc = double.MinValue;
            int bestLag = -1; 
            double ac;
            for (int lag = minLag; lag <= maxLag; lag++) {
                ac = Autocorr(serie, lag);
                if (ac > bestAc + epsilon) {
                    bestAc = ac;
                    bestLag = lag;
                }
            }
            if (bestAc > minAutoCorr) { return bestLag; } 
            else { return -1; }
        }

        internal List<int> CalculateEnoughAutocorrLags(List<double> serie, double minAutoCorr, int minLag, int maxLag) {
            List<int> enoughLags = new List<int>();
            double ac;
            for (int lag = minLag; lag <= maxLag; lag++) {
                ac = Autocorr(serie, lag);
                if (ac >= minAutoCorr) {
                    enoughLags.Add(lag);
                }
            }
            return enoughLags;           
        }


        internal List<double> CalculateLagSerie(List<double> serie, int lag, int mw) {
            List<double> lagSerie = new List<double>();
            for (int i = serie.Count - lag; i > 0; i = i - lag) {
                for (int j = i - mw / 2; j < i + (mw - mw / 2); j++) {
                    lagSerie.Add(serie[j]);
                }
            }
            return lagSerie;
        }

        #endregion

        #region Partial autocorrelation

        internal List<double> PACF(IList<double> acfs) {
            double[] pacfs = null;
            double[] var = null;
            double[][] alpha = null;
            
            // get acfs
            int lags = acfs.Count;
		    pacfs = new double[lags];
		    var = new double[lags];
		    alpha = new double[lags][];
            for(int i=0;i<lags;i++) { alpha[i] = new double[lags]; }

		    //values 0-1
            pacfs[0] = acfs[0];
		    var[0] = 0;
		    alpha[0][0] = 0;
		    alpha[1][0] = 0;
		    alpha[0][1] = 0;
		    pacfs[1] = acfs[1];
		    alpha[1][1] = pacfs[1];
		    var[1] = 1 - Math.Pow(pacfs[1], 2);

		    //values 2-lags with iterative formula
		    for (int k=1;k<lags-1;k++) {
			    double sum1 = 0;
			    for (int j=1;j<=k;j++) { sum1 += alpha[j][k] * acfs[k+1-j]; }
    			
			    pacfs[k+1] = (acfs[k+1]-sum1) / var[k];
			    alpha[k+1][k+1] = pacfs[k+1];

			    //now determine remaining alphas
			    for (int j=1;j<=k;j++) { alpha[j][k+1] = alpha[j][k] - pacfs[k+1] * alpha[k+1-j][k];  }
			    var[k+1] = var[k] * (1 - Math.Pow(pacfs[k+1], 2));
		    }
		    return new List<double>(pacfs);
	    }




        #endregion

        #region Seasonal index

        /** Method:  Calculate Seasonal index
        cutoff - cutoff for trimming*/
        internal double CalcSeasonalIndex(List<double> serie, int lag, double cutoff)
        {
            List<double> ma = func.MovingAverage(serie, lag, true);
            List<double> si = new List<double>();
            for (int i = 0; i < serie.Count; i++) { si.Add((serie[i] - ma[i]) / ma[i]); }
            List<double> trimSi = Trim(si, cutoff, true);
            List<double> ranges = new List<double>();
            int ini = 0;
            int end = lag;
            while (end < si.Count)
            {
                ranges.Add(Range(trimSi, ini, end));
                ini = end + 1;
                end = ini + 1 + lag;
            }
            return Mean(ranges);
        }

        /** Method:  Calculate Seasonal index 
       cutoff - cutoff for trimming*/
        /// <returns> list of seasonal indexes for each lag </returns>
        internal List<double> CalcSeasonalIndex(List<double> serie, List<int> lags, double cutoff)
        {
            List<double> seasIndexes = new List<double>();
            double seasIndex;
            List<double> ma, si, windSi, ranges;
            foreach (int lag in lags)
            {
                if (lag > serie.Count / 2.0) { continue; }
                ma = func.MovingAverage(serie, lag, true);
                si = new List<double>();
                for (int i = 0; i < serie.Count; i++) { si.Add((serie[i] - ma[i])); }
                windSi = Windsor(si, cutoff, true);
                ranges = new List<double>();
                int ini = 0;
                int end = lag;
                while (end < windSi.Count)
                {
                    ranges.Add(Range(windSi, ini, end));
                    ini = end + 1;
                    end = ini + 1 + lag;
                }

                seasIndex = Mean(ranges) / Mean(serie);
                seasIndexes.Add(seasIndex);
            }
            return seasIndexes;
        }

        internal int CalcBestSeasonalLag(List<double> serie, double cutoff, double minAutoCorr, double minAmplitude, int minLag, int maxLag) {
            List<int> lags = CalculateEnoughAutocorrLags(serie, minAutoCorr, minLag, maxLag);
            if (lags == null || lags.Count == 0) { return -1; }
            if (lags.Count == 1) { return lags[0]; }
            List<double> sis = CalcSeasonalIndex(serie, lags, cutoff);
            int index = func.MaxIndex(sis);
            if (index == -1 || sis[index] < minAmplitude) { return -1; }
            else { return lags[index]; }
        }

        #endregion

        #region Pendiente promedio

        /** Method:  Average slope of a function, using angular coeff of quadratic means */
        internal double AverageSlope(List<double> x, List<double> y)   {
            List<double> quadMins = polynom.Regression(x, y, 1);
            return quadMins[1];
        }

        /** Method:  Average slope of a function, using angular coeff of quadratic means */
        internal double AverageSlope(List<double> vals)  {
            List<double> xList = new List<double>();
            for (int i = 0; i < vals.Count; i++) { xList.Add((double)i); }
            List<double> quadMins = polynom.Regression(xList, vals, 1);
            return quadMins[1];
        }

        #endregion

        #region Solve for R

        /** Method: Solve 
        "Solve" by Montecarlo newD2 (new data on data2) for a given newD1 (new datum on data1) and a r.*/
        internal double Solve(IList<double> data1, IList<double> data2, double r, double newD1, int iterations, int movingWindow, double maxInc)
        {
            Random rand = new Random(Environment.TickCount);
            List<double> data1Sim = new List<double>(data1);
            List<double> data2Sim = new List<double>(data2);
            data1Sim.Add(newD1);
            data2Sim.Add(-1);
            double fcst = -1;
            double max = func.Max((List<double>)data2);
            double d2Sim, rSim;
            double minDiff = double.MaxValue;
            for (int i = 0; i < iterations; i++)
            {
                d2Sim = (double)rand.Next(0, (int)(max * (1 + maxInc)));
                data2Sim[data2Sim.Count - 1] = d2Sim;
                rSim = R(data1Sim, data2Sim, data1Sim.Count - movingWindow - 1, data1Sim.Count - 1);
                double diff = Math.Abs(r - rSim);
                if (diff < minDiff)
                {
                    minDiff = diff;
                    fcst = d2Sim;
                }
            }
            return fcst;
        }

        #endregion 

        #region  Stat

    	#region Required Functions

		/*
		   DESCRIPTION
		     Evaluates the "deviance part"
		 	 bd0(x,M) :=  M * D0(sample/M) = M*[ sample/M * log(sample/M) + 1 - (sample/M) ] =
		 	           =  x * log(sample/M) + M - sample
		     where M = E[X] = n*p (or = lambda), for	  x, M > 0
		 
		 	in a manner that should be stable (with small relative error)
		 	for all x and np. In particular for sample/np close to 1, direct
		    evaluation fails, and evaluation is based on the Taylor series
		 x Parametro x de la función Deviance Part.
		 np Parametro np de la función Deviance Part.*/
        internal double bd0(double x, double np) {
			double ej, s, s1, v;
			int j;

			if (Math.Abs(x-np) < 0.1*(x+np)) 
			{
				v = (x-np)/(x+np);
				s = (x-np)*v; /* s using v -- change by MM */
				ej = 2*x*v;
				v = v*v;
				for (j=1; ; j++) 
				{ /* Taylor series */
					ej *= v;
					s1 = s+ej/((j<<1)+1);
					if (s1==s) /* last term was effectively 0 */
						return(s1);
					s = s1;
				}
			}
			/* else:  | x - np |  is not too small */
			return(x*Math.Log(x/np)+np-x);
		}

		/*   DESCRIPTION
		
			   Computes the log of the error term in Stirling's formula.
		      For n Greater Than 15, uses the series 1/12n - 1/360n^3 + ...
		      For n Less or Equal than  15, integers or half-integers, uses stored values.
			     For other n Less than 15, uses lgamma directly (don't use this to
		        write lgamma!)
		
		n Parametro n de la función que devuelve el log del error de la formula de Stirling.*/
        internal double stirlerr(double n) {
			/*
			  error for 0, 0.5, 1.0, 1.5, ..., 14.5, 15.0.
			*/

			double [] sferr_halves = 
										{
											0.0, /* n=0 - wrong, place holder only */
											0.1534264097200273452913848,  /* 0.5 */
											0.0810614667953272582196702,  /* 1.0 */
											0.0548141210519176538961390,  /* 1.5 */
											0.0413406959554092940938221,  /* 2.0 */
											0.03316287351993628748511048, /* 2.5 */
											0.02767792568499833914878929, /* 3.0 */
											0.02374616365629749597132920, /* 3.5 */
											0.02079067210376509311152277, /* 4.0 */
											0.01848845053267318523077934, /* 4.5 */
											0.01664469118982119216319487, /* 5.0 */
											0.01513497322191737887351255, /* 5.5 */
											0.01387612882307074799874573, /* 6.0 */
											0.01281046524292022692424986, /* 6.5 */
											0.01189670994589177009505572, /* 7.0 */
											0.01110455975820691732662991, /* 7.5 */
											0.010411265261972096497478567, /* 8.0 */
											0.009799416126158803298389475, /* 8.5 */
											0.009255462182712732917728637, /* 9.0 */
											0.008768700134139385462952823, /* 9.5 */
											0.008330563433362871256469318, /* 10.0 */
											0.007934114564314020547248100, /* 10.5 */
											0.007573675487951840794972024, /* 11.0 */
											0.007244554301320383179543912, /* 11.5 */
											0.006942840107209529865664152, /* 12.0 */
											0.006665247032707682442354394, /* 12.5 */
											0.006408994188004207068439631, /* 13.0 */
											0.006171712263039457647532867, /* 13.5 */
											0.005951370112758847735624416, /* 14.0 */
											0.005746216513010115682023589, /* 14.5 */
											0.005554733551962801371038690  /* 15.0 */
										};
			double nn;

			if (n <= 15.0) 
			{
				nn = n + n;
				if (nn == (int)nn) return(sferr_halves[(int)nn]);
				return(lngamma (n + 1.0) - (n + 0.5)* Math.Log(n) + n - M_LN_SQRT_2PI);
			}

			nn = n*n;
			if (n>500) return((S0-S1/nn)/n);
			if (n> 80) return((S0-(S1-S2/nn)/nn)/n);
			if (n> 35) return((S0-(S1-(S2-S3/nn)/nn)/nn)/n);
			/* 15 < n <= 35 : */
			return((S0-(S1-(S2-(S3-S4/nn)/nn)/nn)/nn)/n);
		}
		#endregion Required Functions

        #region Clasical statistical distribution functions

        /* Devuelve la función Gamma */
        internal double gamma(double k) {
			double [] cof  =  {75122.6331530, 80916.6278952, 36308.2951477, 8687.24529705,
								  1168.92649479, 83.8676043424, 2.50662827511};
			double sum = (double) 0, prod = (double) 1, aux = 5.5 + k;

			for (int j = 0; j < 7; j++) 
			{
				sum += cof[j] * Math.Pow (k, j);
				prod *= (k + j);
			}
			return (sum / prod) * Math.Pow (aux, k+0.5) * Math.Pow (Math.E, -aux);
		}

		/*Devuelve la función Ln (Gamma (df))) */
        internal double lngamma(double k) {
			double x = k, y = k, tmp, ser;
			double [] cof  =  {76.18009172947146, -86.50532032941677, 24.01409824083091,
								  -1.2317395724500155, 0.1208650973866179e-2, -0.5395239384953e-5};

			tmp = x+5.5;
			tmp -= (x + 0.5) * Math.Log (tmp);
			ser = 1.000000000190015;
			for (int j = 0; j <= 5; j++) ser += cof[j]/++y;
			return -tmp + Math.Log (2.5066282746310005 * ser / x);
		}

		/* Returns the beta B(z, w) function. 
		z value of the beta function.
		w value of the beta function.*/
        internal double beta(double z, double w) {
			return Math.Exp (lngamma (z) + lngamma (w) - lngamma (z + w));
		}

		/* Returns the incomplete gamma P(a, x) function.  
		a a value of the incomplete gamma P(a, x) function.
		x x value of the incomplete gamma P(a, x) function.*/
        internal double gammap(double a, double x) {
			double gamser = 0.0, gammcf = 0.0, gln = 0.0;

			if ((x < 0.0) || (a <= 0.0)) throw new ArgumentException("Strings.Invalid_arguments_the_incomplete_gamma_function_P_a__x");

			if (x < (a + 1.0))
			{
				gser (ref gamser, a, x, ref gln);
				return gamser;
			}
			else
			{
				gcf (ref gammcf, a, x, ref gln);
				return 1.0 - gammcf;
			}
		}

		/* Returns the incomplete gamma Q(a, x) = 1 - P(a, x) function.  Tested using tables from internet.
		a a value of the incomplete gamma Q(a, x) function.
		x x value of the incomplete gamma Q(a, x) function.*/
        internal double gammaq(double a, double x) {
			double gamser = 0.0, gammcf = 0.0, gln = 0.0;

			if ((x < 0.0) || (a <= 0.0)) throw new ArgumentException("Invalid_arguments");

			if (x < (a + 1.0))
			{
				gser (ref gamser, a, x, ref gln);
				return 1.0 - gamser;
			}
			else
			{
				gcf (ref gammcf, a, x, ref gln);
				return gammcf;
			}
		}

		/* Requiered function for gamma, beta and incomplete beta and gamma functions.*/
        internal void gser(ref double gamser, double a, double x, ref double gln) {
			double sum, del, ap;

			gln = lngamma (a);

			if (x <= 0.0) 
			{
				if (x < 0.0) throw new ArgumentException("Invalid argument for function gser");
				gamser = 0.0;
				return;
			}
			else
			{
				ap = a;
				del = sum = 1.0 / a;
				for (int n = 1; n <= ITMAX; n++)
				{
					++ap;
					del *= x / ap;
					sum += del;
					if (Math.Abs (del) < Math.Abs(sum) * EPS)
					{
						gamser = sum * Math.Exp (-x + a * Math.Log (x) - gln);
						return;
					}
				}
				throw new ArgumentException("El argumento a es muy grande e ITMAX demasiado pequeño en gser");
			}

		}

		//*Requiered function for gamma, beta and incomplete beta and gamma functions.*/
        internal void gcf(ref double gammcf, double a, double x, ref double gln) {
			double an, b, c, d, del, h;
			int i = 0;

			gln = lngamma (a);
			b = x + 1.0 - a;
			c = 1.0 / FPMIN;
			d = 1.0 / b;
			h = d;

			for (i = 1; i <= ITMAX; i++)
			{
				an = -i * (i - a);
				b += 2.0;
				d = an * d + b;
				if (Math.Abs(d) < FPMIN) d = FPMIN;
				c = b + an / c;
				if (Math.Abs(c) < FPMIN) c = FPMIN;
				d = 1.0 / d;
				del = c * d;
				h *= del;
				if (Math.Abs(del - 1.0) < EPS) break;
			}

			if (i > ITMAX) throw new ArgumentException("El argumento a es muy grande e ITMAX demasiado pequeño en gcf");
			gammcf = Math.Exp (-x + a * Math.Log (x)- gln) * h;
		}

		/*Returns the error function.  */
        internal double erff(double x) {
			return x < 0.0 ? -gammap (0.5, Math.Pow (x, 2)) : gammap (0.5, Math.Pow (x, 2));
		}

		/* Returns the complementary error function. */
        internal double erffc(double x) {
			return x < 0.0 ? 1.0 + gammap (0.5, Math.Pow (x, 2)) : gammaq (0.5, Math.Pow (x, 2));
		}


		/* Returns the probability distribution of Chi-Cuadrado P(Chi-Cuadrado | v) */
        internal double chi_square_p(double chi, double v) {
			return gammap (v / 2.0, chi / 2.0);
		}

		/* Returns the complementary probability distribution of Chi-Cuadrado P(Chi-Cuadrado | v)*/
        internal double chi_square_q(double chi, double v) {
			return gammaq (v / 2.0, chi / 2.0);
		}

		/* Returns the incomplete Beta (a, b, x) = Ix(a, b) function. */
        internal double betai(double a, double b, double x) {
			double bt;

			if (x < 0.0 || x > 1.0) throw new ArgumentException("Strings.Invalid_argument_x_for_function_betai");
			if (x == 0.0 || x == 1.0)
				bt = 0.0;
			else
				bt = Math.Exp (lngamma (a + b) - lngamma (a) -lngamma (b) + a * Math.Log (x) + 
					b * Math.Log (1.0 - x));
			if (x < (a + 1.0) / (a + b + 2.0))
				return bt * betacf (a, b, x) / a;
			else
				return 1.0 - bt * betacf (b, a, 1.0 -x) / b;
		}

		/* Used by function betai. Evaluates continued fraction for incomplete beta function by modified Lentz`s method*/
        internal double betacf(double a, double b, double x) {
			int m, m2;
			double aa, c, d, del, h, qab, qam, qap;

			qab = a + b;
			qap = a + 1.0;
			qam = a - 1.0;
			c = 1.0;
			d = 1.0 - qab * x / qap;
			if (Math.Abs (d) < FPMIN) d = FPMIN;
			d = 1.0 / d;
			h = d;
			for (m = 1; m <= ITMAX; m++)
			{
				m2 = 2* m;
				aa = m * (b - m) * x / ((qam + m2) * (a + m2));
				d = 1.0 + aa * d;
				if (Math.Abs (d) < FPMIN) d= FPMIN;
				c = 1.0 + aa / c;
				if (Math.Abs (c) < FPMIN) c= FPMIN;
				d = 1.0 / d;
				h *= d * c;
				aa = -(a + m) * (qab + m) * x / ((aa + m2) * (qap + m2));
				d = 1.0 + aa * d;
				if (Math.Abs (d) < FPMIN) d= FPMIN;
				c = 1.0 + aa / c;
				if (Math.Abs (c) < FPMIN) c= FPMIN;
				d = 1.0 / d;
				del = d * c;
				h *= del;
				if (Math.Abs (del - 1.0) < EPS) break;
			}
			if (m > ITMAX) throw new ArgumentException("Argument a or b are too big or MAXIT is too small in betacf");
			return h;
		}

		/* Devuelve la probabilidad x de un distribución T-Student con n grados de libertad,
		moda m y parametro de escala c, Tn(m, c) 
		Returns the probability of a T-Student distribution with n degrees
		of freedom, mode m and scale parameter c, Tn(m, c) 
		x Value for which the probability is calculated .
		m Mode of the T-Student distribution.
		c Scale parameter of the T-Student distribution.
		n degrees of freedom of the T-Student distribution.
		give_log if true returns the log of the value, else just the value.*/
        internal double TStudent(double x, double m, double c, double n, bool give_log) {
/*			return ((gamma ((n + 1) / 2) * Math.Pow (n, n / 2)) / 
				(gamma (n / 2) * Math.Pow ((Math.PI * c), 0.5))) * 
				Math.Pow ((n + Math.Pow (x - m, 2) / c), - (n + 1) / 2);
*/
			double t, u, x_01 = (x - m) / c, x_01square = Math.Pow (x_01, 2);

			t = -bd0(n/2.0,(n+1)/2.0) + stirlerr((n+1)/2.0) - stirlerr(n/2.0);
			if ( x_01square > 0.2 * n )
				u = Math.Log( 1 + x_01square / n ) * n/2;
			else
				u = -bd0(n/2.0,(n+x_01square)/2.0) + Math.Pow (x_01, 2)/2.0;

				
			if (give_log)
				return (-0.5*Math.Log(M_2PI*(1+x_01square /n))+(t-u));
			else
				return (Math.Exp(t-u) / Math.Sqrt(M_2PI*(1+x_01square/n)));
		}
	


		/** Returns the log value of the probability of a T-Student distribution with n degrees
		of freedom, mode m and scale parameter c, ln (Tn(m, c))
		 
		0.5 * Ln (Pi * e) = 0.072364942924700087071713675676529 
		x Value for which the probability is calculated .
		m Mode of the T-Student distribution.
		c Scale parameter of the T-Student distribution.
		n degrees of freedom of the T-Student distribution.*/
        internal double lnTStudent(double x, double m, double c, double n) {
			double n1 = n + 1;

			return Math.Log (gamma (n1 / 2)) + n / 2 * Math.Log (n) - Math.Log (gamma (n / 2)) -
				0.072364942924700087071713675676529 - 
				n1 / 2 * Math.Log (n + Math.Pow (x - m, 2) / Math.E);
		}

		
		/** Returns the inverse accumulated probability of a T-Student distribution with n degrees
		of freedom, mode 0 and scale parameter 1, Tn(0, 1)
		The number o degrees of freedom has to be greater than 1 (n>=1), 
		if not the value is not correct. 
		p Probability to search for
		ndf Degrees of freedom
		lower_tail">lower_tail True = gets (-inf, x). False = gets (sample, +inf).*/
        internal double TStudent_quantil(double p, double ndf, bool lower_tail) {
			double eps=1e-12, P, prob, q, y, a, b, c, d, x;
			double M_PI_2=1.570796326794896619231321691640; // pi/2
			bool neg;

			// Si el numero de grados de libertad es menor que 1, el valor es incorrecto
			// FIXME: This test should depend on  ndf  AND p  !!
			//        and in fact should be replaced by
			//        something like Abramowitz & Stegun 26.7.5 (p.949)
			// Se ha comprobado que la aproximación de Abramowitz & Stegun 26.7.5 (p.949)
			// no funciona cuando el numero de grados de libertad es < 1
			// La aproximación en cuestión es la que tenemos a continuación

/*			if(p<=0 || p>=1) return -1;

			if (ndf < 1)
			{
				double x1,g1,g2,g3,g4;
				x1=Stat.InvNormal_acum (p);
				g1=(Math.Pow(x1,3.0)+x1)/4.0;
				g2=(5.0*Math.Pow(x1,5.0)+16.0*Math.Pow(x1,3.0)+3.0*x1)/
					96.0;
				g3=(3.0*Math.Pow(x1,7.0)+19.0*Math.Pow(x1,5.0)+
					17.0*Math.Pow(x1,3.0)-15.0*x1)/384.0;
				g4=(79.0*Math.Pow(x1,9.0)+776.0*Math.Pow(x1,7.0)+
					1482.0*Math.Pow(x1,5.0)-1920.0*Math.Pow(x1,3.0)-
					945.0*x1)/92160.0;
				return x1+g1/ndf+g2/Math.Pow(ndf,2.0)+
					g3/Math.Pow(ndf,3.0)+g4/Math.Pow(ndf,4.0);
			}
*/
			if(p<=0 || p>=1 || ndf<1) return -1;
			if((lower_tail && p > 0.5) || (!lower_tail && p < 0.5)) 
			{
				neg = false;
				P = 2 * (lower_tail ? (1 - p) : p);
			}
			else 
			{
				neg = true;
				P = 2 * (lower_tail ? p : (1 - p));
			}

			if(Math.Abs(ndf - 2) < eps) 
			{   /* df ~= 2 */
				q=Math.Sqrt(2 / (P * (2 - P)) - 2);
			}
			else if (ndf < 1 + eps) 
			{   /* df ~= 1 */
				prob = P * M_PI_2;
				q = Math.Cos(prob)/Math.Sin(prob);
			}
			else 
			{      /*-- usual case;  including, e.g.,  df = 1.1 */
				a = 1 / (ndf - 0.5);
				b = 48 / (a * a);
				c = ((20700 * a / b - 98) * a - 16) * a + 96.36;
				d = ((94.5 / (b + c) - 3) / b + 1) * Math.Sqrt(a * M_PI_2) * ndf;
				y = Math.Pow(d * P, 2 / ndf);
				if (y > 0.05 + a) 
				{
					/* Asymptotic inverse expansion about normal */
					//x = qnorm(0.5 * P, false);
                    x = qnorm(0.5 * P, 0, 1, false, false);
					y = x * x;
					if (ndf < 5)
						c += 0.3 * (ndf - 4.5) * (x + 0.6);
					c = (((0.05 * d * x - 5) * x - 7) * x - 2) * x + b + c;
					y = (((((0.4 * y + 6.3) * y + 36) * y + 94.5) / c - y - 3) / b + 1) * x;
					y = a * y * y;
					if (y > 0.002)
						y = Math.Exp(y) - 1;
					else 
					{ /* Taylor of    e^y -1 : */
						y = (0.5 * y + 1) * y;
					}
				}
				else 
				{
					y = ((1 / (((ndf + 6) / (ndf * y) - 0.089 * d - 0.822)
						* (ndf + 2) * 3) + 0.5 / (ndf + 4))
						* y - 1) * (ndf + 1) / (ndf + 2) + 1 / y;
				}
				q = Math.Sqrt(ndf * y);
			}
			if(neg) q = -q;
			return q;
		}

        internal double qnorm(double p, double mu, double sigma, bool lower_tail, bool log_p) {
            double p_, q, r, val;

            if (Double.IsNaN(p) || Double.IsNaN(mu) || Double.IsNaN(sigma))
                return p + mu + sigma;

            if (p == R_DT_0(lower_tail, log_p)) return ((-1.0) / 0.0);
            if (p == R_DT_1(lower_tail, log_p)) return (1.0 / 0.0);
            if (R_Q_P01_check(p, log_p))
                throw new ArgumentException("Invalid arguments to the R qnorm function Invalid p and or log p");

            if (sigma < 0) throw new ArgumentException("Invalid arguments to the R qnorm function Invalid sigma");
            if (sigma == 0) return mu;

            p_ = R_DT_qIv(p, log_p, lower_tail);/* real lower_tail prob. p */
            q = p_ - 0.5;

            /*-- use AS 241 --- */
            /* double ppnd16_(double *p, long *ifault)*/
            /*      ALGORITHM AS241  APPL. STATIST. (1988) VOL. 37, NO. 3

                    Produces the normal deviate Z corresponding to a given lower
                    tail area of P; Z is accurate to about 1 part in 10**16.

                    (original fortran code used PARAMETER(..) for the coefficients
                     and provided hash codes for checking them...)
            */
            if (Math.Abs(q) <= 0.425) {/* 0.075 <= p <= 0.925 */
                r = 0.180625 - q * q;
                val =
                    q * (((((((r * 2509.0809287301226727 +
                    33430.575583588128105) * r + 67265.770927008700853) * r +
                    45921.953931549871457) * r + 13731.693765509461125) * r +
                    1971.5909503065514427) * r + 133.14166789178437745) * r +
                    3.387132872796366608)
                    / (((((((r * 5226.495278852854561 +
                    28729.085735721942674) * r + 39307.89580009271061) * r +
                    21213.794301586595867) * r + 5394.1960214247511077) * r +
                    687.1870074920579083) * r + 42.313330701600911252) * r + 1.0);
            }
            else { /* closer than 0.075 from {0,1} boundary */

                /* r = min(p, 1-p) < 0.075 */
                if (q > 0.0)
                    r = R_DT_CIv(p, log_p, lower_tail); /* 1-p */
                else
                    r = p_;/* = R_DT_Iv(p) ^=  p */

                r = Math.Sqrt(-((log_p &&
                    ((lower_tail && q <= 0) || (!lower_tail && q > 0))) ?
                p : /* else */ Math.Log(r)));
                /* r = sqrt(-log(r))  <==>  min(p, 1-p) = exp( - r^2 ) */

                if (r <= 5.0) { /* <==> min(p,1-p) >= exp(-25) ~= 1.3888e-11 */
                    r += -1.6;
                    val = (((((((r * 7.7454501427834140764e-4 +
                        .0227238449892691845833) * r + .24178072517745061177) *
                        r + 1.27045825245236838258) * r +
                        3.64784832476320460504) * r + 5.7694972214606914055) *
                        r + 4.6303378461565452959) * r +
                        1.42343711074968357734)
                        / (((((((r *
                        1.05075007164441684324e-9 + 5.475938084995344946e-4) *
                        r + .0151986665636164571966) * r +
                        .14810397642748007459) * r + .68976733498510000455) *
                        r + 1.6763848301838038494) * r +
                        2.05319162663775882187) * r + 1.0);
                }
                else { /* very close to  0 or 1 */
                    r += -5.0;
                    val = (((((((r * 2.01033439929228813265e-7 +
                        2.71155556874348757815e-5) * r +
                        .0012426609473880784386) * r + .026532189526576123093) *
                        r + .29656057182850489123) * r +
                        1.7848265399172913358) * r + 5.4637849111641143699) *
                        r + 6.6579046435011037772)
                        / (((((((r *
                        2.04426310338993978564e-15 + 1.4215117583164458887e-7) *
                        r + 1.8463183175100546818e-5) * r +
                        7.868691311456132591e-4) * r + .0148753612908506148525)
                        * r + .13692988092273580531) * r +
                        .59983220655588793769) * r + 1.0);
                }

                if (q < 0.0)
                    val = -val;
                /* return (q >= 0.)? r : -r ;*/
            }
            return mu + sigma * val;
        }


        private double R_D_0(bool log_p) {
            return (log_p ? ((-1.0) / 0.0) : 0.0);
        }

        private double R_D_1(bool log_p) {
            return (log_p ? 0.0 : 1.0);
        }

        private double R_DT_0(bool lower_tail, bool log_p) {
            return (lower_tail ? R_D_0(log_p) : R_D_1(log_p));
        }

        private double R_DT_1(bool lower_tail, bool log_p) {
            return (lower_tail ? R_D_1(log_p) : R_D_0(log_p));
        }


        private bool R_Q_P01_check(double p, bool log_p) {
            return ((log_p && p > 0) || (!log_p && (p < 0 || p > 1)));
        }

        private double R_D_Cval(double p, bool lower_tail) {
            return (lower_tail ? (1 - (p)) : (p));
        }

        
        double R_DT_qIv(double p, bool log_p, bool lower_tail) {
            return (log_p ? (lower_tail ? Math.Exp(p) : -expm1(p)) : (lower_tail ? (p) : (1 - (p))));
        }
        
        private double R_DT_CIv(double p, bool log_p, bool lower_tail) {
            return (log_p ? (lower_tail ? -expm1(p) : Math.Exp(p)) : R_D_Cval(p, lower_tail));
        }

        private double DBL_EPSILON = 2.220446049250313e-16;
        
        /** Method: 
             Compute the Exponential minus 1.
             accurately also when x is close to zero, i.e. |sample| is near 1.
            x - Value for whcih to calculate Math (x) – 1. */
        internal double expm1(double x) {
            double y, a = Math.Abs(x);

            if (a < DBL_EPSILON) return x;
            if (a > 0.697) return (Math.Exp(x) - 1);

            if (a > 1e-8) y = Math.Exp(x) - 1;
            else y = (x / 2 + 1) * x; /* Taylor expansion, more accurate in this range */

            /* Newton step for solving   log(1 + y) = x   for y : */
            /* WARNING: does not work for y ~ -1: bug in 1.5.0 */
            y -= (1 + y) * (log1p(y) - x);
            return y;
        }

        /** Method: 
     Compute the relative error logarithm. log(1 + x).
    x - Value for which to calculate Math.Log (1 + x).*/
        internal double log1p(double x) {
            return (Math.Log(1 + x));
        }
        
        /** Returns the distribution probability function T-Student A(t | v), with n degrees
                                of freedom, mode m and scale parameter c, Tn(m, c)
                                t Value to be tested.
                                v degrees of freedom of the T-Student distribution.
                                m Mode of the T-Student distribution.
                                e Scale parameter of the T-Student distribution.*/
        internal double Test_TStudent(double t, double v, double m, double e) {
			return 1.0 - betai (v / 2.0, 0.5, v / (v + Math.Pow ((t - m) / e, 2)));
		}

		/** Returns the probability that c chi-2distrubution with dof degrees of freedom is
		/// less or equal to x
		x sample Value to be tested.
		dof degrees of freedom of the T-Student distribution*/
        internal double Test_Chi2(double x, double dof) {
			return 1 - (gammap (dof/2, x/2) / gamma (dof/2));
		}

		/** Returns the probability function F, Q(F | v1, v2)
		/// where t es the value to be tested and v1 and v2 the variances or degrees of freedom
		F Value to be tested.
		v1 First variance or degrees of freedom of the F function.
		v2 Second variance or degrees of freedom of the F function.*/
        internal double Test_F(double F, double v1, double v2) {
			return 1 - betai (v2 / 2.0, v1 / 2.0, v2 / (v2 + (v1 * F)));
		}

        /** F Distribution 
        f statistic </param>
       	v1 First variance or degrees of freedom of the F function.
		v2 Second variance or degrees of freedom of the F function.*/
        internal double FDistribution(double f, double v1, double v2) {
            if((v1*f)/(v1 + v2*f) > 1) { return 0; }  //TODO: Verificar
            return  betai(v1/2, v2/2, v1/(v1 + v2*f));
        }

        /** the incomplete beta function from 0 to x with parameters a, b. x must be in (0,1)*/ 
        private double Betai(double a, double b, double x) {
            double bt=0;
            double beta = double.MaxValue;
            if(x==0 || x==1) {
                bt = 0;
            } 
            else if((x>0)&&(x<1)) {
                bt = gamma(a+b)*Math.Pow(x, a)*Math.Pow(1-x, b)/(gamma(a)*gamma(b));
            }
            if(x<(a+1)/(a+b+2)) {
                beta = bt*betacf(a, b, x)/a;
            } 
            else {
                beta = 1-bt*betacf(b, a, 1-x)/b;
            }
            return beta;
        }

  
		#endregion Clasical statistical distribution function

		#region Descriptive statistical function


        /** Returns the Mean value "Values".
		If FirstP is less than 0, returns -1
		If FirstP is greater than NValues, returns -2
		/// FirstP starts to count on 0.
		/// </summary>
		/// <param name="Values">Values = Array of values from which to calculate the mean.</param>
		/// <param name="NValues">NValues = Number of values passed in the "Values" array.</param>
		/// <param name="FirstP">FirstP = First period to consider of the values, 0 all periods.</param>
        /// <returns> mean value </returns>
		internal static double Mean (double  [] Values, int NValues, int FirstP)	{
			double Sum = (double) 0;

			if (FirstP < 0) return (-1);
			if (FirstP > NValues) return (-2);

			if (NValues != 0) 
			{
				for (int i = FirstP; i < NValues; i++) Sum += Values [i];
				return (Sum / (NValues - FirstP));
			}
			else return ((double) 0);
		}

        /// <summary>
        /// Returns the Mean value "Values".
        /// </summary>
        /// <param name="Values">Values = Array of values from which to calculate the mean.</param>
        /// <returns> mean values </returns>
        internal static double Mean(double[] Values) {
            double Sum = 0.0;
            int NValues = Values.Length;

            if (NValues != 0)
            {
                for (int i = 0; i < NValues; i++) Sum += Values[i];
                return (Sum / NValues);
            } else return (0.0);
        }
        
        /// <summary>
		/// Returns the Mean value "Values".
		/// If FirstP is less than 0, returns -1
		/// If FirstP is greater than NValues, returns -2
		/// FirstP starts to count on 0.
		/// </summary>
		/// <param name="Values">Values = Array of values from hich to calculate the mean.</param>
		/// <param name="NValues">NValues = Number of values passed in the "Values" array.</param>
		/// <param name="UseValue">UseValue = if 1, the value is used. If 0, the values is not used.</param>
		/// <param name="FirstP">FirstP = First period to consider of the values, 0 all periods.</param>
        /// <returns> calculated mean </returns>*/
        internal static double Mean(double[] Values, int NValues, int[] UseValue, int FirstP) {
			double Sum = (double) 0; 
			int values = 0;

			if (FirstP < 0) return (-1);
			if (FirstP > NValues) return (-2);

			if (NValues != 0) 
			{
				for (int i = FirstP; i < NValues; i++) 
					if (UseValue [i] == 1) 
					{
						Sum += Values [i];
						values ++;
					}
				return (Sum / values);
			}
			else return ((double) 0);
		}

		/*Returns the varianza Muestral of array Values.
		If FirstP is less than 0, returns -1
		If FirstP is greater than NValues, returns -2
		FirstP starts to count on 0.
		Values = Array of values from hich to calculate the mean.
		NValues = Number of values passed in the Values array.
		FirstP = First period to consider of the values, 0 all periods. */
        internal double VarMuestral(double[] Values, int NValues, int FirstP) {
            if(Values.Length == 1) { return 0; }
            int values = 0;
			double Sum = (double) 0, SumSquares = (double) 0;

			if (FirstP < 0) return (-1);
			if (FirstP > NValues) return (-2);

			for (int i = FirstP; i < NValues; i++) 
			{
				Sum += Values [i];
				SumSquares += Math.Pow (Values [i], 2);
				values ++;
			}
			return ((values * SumSquares -  Math.Pow (Sum, 2)) / (values * (values - 1)));
		}

        /// <summary>
        /// Returns the varianza Muestral of array "Values".
        /// If FirstP is less than 0, returns -1
        /// If FirstP is greater than NValues, returns -2
        /// FirstP starts to count on 0.
        /// </summary>
        /// <param name="Values">Values = Array of values from hich to calculate the mean.</param>
        /// <returns> variance value </returns>
        internal double VarMuestral(double[] Values) {
            int NValues = Values.Length;
            if(NValues == 1) { return 0; }
            double Sum = 0.0, SumSquares = 0.0;

            for (int i = 0; i < NValues; i++)
            {
                Sum += Values[i];
                SumSquares += Math.Pow(Values[i], 2);
            }
            return ((NValues * SumSquares - Math.Pow(Sum, 2)) / (NValues * (NValues - 1)));
        }

        /// <summary>
		/// Returns the varianza Muestral of array "Values".
		/// If FirstP is less than 0, returns -1
		/// If FirstP is greater than NValues, returns -2
		/// FirstP starts to count on 0.
		/// </summary>
		/// <param name="Values">Values = Array of values from hich to calculate the mean.</param>
		/// <param name="NValues">NValues = Number of values passed in the "Values" array.</param>
		/// <param name="UseValue">UseValue = if 1, the value is used. If 0, the values is not used.</param>
		/// <param name="FirstP">FirstP = First period to consider of the values, 0 all periods.</param>
        /// <returns> calculated variance </returns>
        internal double VarMuestral(double[] Values, int NValues, int[] UseValue, int FirstP) {
			int values = 0;
			double Sum = (double) 0, SumSquares = (double) 0;

			if (FirstP < 0) return (-1);
			if (FirstP > NValues) return (-2);

			for (int i = FirstP; i < NValues; i++) 
				if (UseValue [i] == 1) 
				{
					Sum += Values [i];
					SumSquares += Math.Pow (Values [i], 2);
					values ++;
				}
			return ((values * SumSquares -  Math.Pow (Sum, 2)) / (values * (values - 1)));
		}

		/// <summary>
		/// Returns the varianza Muestral of array "Values1" and "Values2".
		/// If FirstP is less than 0, returns -1
		/// If FirstP is greater than NValues, returns -2
		/// FirstP starts to count on 0.
		/// </summary>
		/// <param name="Values1">Values1 = First Array of values from which to calculate the mean.</param>
		/// <param name="Values2">Values1 = Second Array of values from which to calculate the mean.</param>
		/// <param name="NValues">NValues = Number of values passed in the "Values" array.</param>
		/// <param name="FirstP">FirstP = First period to consider of the values, 0 all periods.</param>
        /// <returns> co variance value </returns>
        internal double CoVarMuestral(double[] Values1, double[] Values2, int NValues, int FirstP) {
			double Mean1 = Mean (Values1, NValues, FirstP), Mean2 = Mean (Values2, NValues, FirstP);
			double Sum = (double) 0;

			if (FirstP < 0) return (-1);
			if (FirstP > NValues) return (-2);

			for (int i = FirstP; i < NValues; i++) Sum += (Values1 [i] - Mean1) * (Values2 [i] - Mean2);
			return (Sum / (NValues - FirstP));
		}

        /// <summary>
        /// Returns the varianza Muestral of array "Values1" and "Values2".
        /// </summary>
        /// <param name="Values1">Values1 = First Array of values from which to calculate the mean.</param>
        /// <param name="Values2">Values1 = Second Array of values from which to calculate the mean.</param>
        /// <returns> co variance value </returns>
        internal double CoVarMuestral(double[] Values1, double[] Values2) {
            if (Values1.Length != Values2.Length)
                throw new ArgumentException("Strings.Both_vectors_must_have_the_same_number_of_values");

            double Mean1 = Mean(Values1), Mean2 = Mean(Values2), Sum = (double)0;
            int NValues = Values1.Length;

            for (int i = 0; i < NValues; i++) Sum += (Values1[i] - Mean1) * (Values2[i] - Mean2);
            return (Sum / NValues);
        }
        
        /// <summary>
		/// Returns the varianza Muestral of array "Values1" and "Values2".
		/// If FirstP is less than 0, returns -1
		/// If FirstP is greater than NValues, returns -2
		/// FirstP starts to count on 0.
		/// </summary>
		/// <param name="Values1">Values1 = First Array of values from which to calculate the mean.</param>
		/// <param name="Values2">Values2 = Second Array of values from which to calculate the mean.</param>
		/// <param name="NValues">NValues = Number of values passed in the "Values" array.</param>
		/// <param name="UseValue">UseValue = if 1, the value is used. If 0, the values is not used.</param>
		/// <param name="FirstP">FirstP = First period to consider of the values, 0 all periods.</param>
        /// <returns> co variance value </returns>
        internal double CoVarMuestral(double[] Values1, double[] Values2, int NValues, int[] UseValue, int FirstP) {
			double Mean1 = Mean (Values1, NValues, UseValue, FirstP), Mean2 = Mean (Values2, NValues, UseValue, FirstP);
			double Sum = (double) 0;
			int values = 0;

			if (FirstP < 0) return (-1);
			if (FirstP > NValues) return (-2);

			for (int i = FirstP; i < NValues; i++) 
				if (UseValue [i] == 1) 
				{
					Sum += (Values1 [i] - Mean1) * (Values2 [i] - Mean2);
					values ++;
				}
			return (Sum / values);
		}

		/// <summary>
		/// Returns the Correlation COeficient of array "Values1" and "Values2".
		/// If FirstP is less than 0, returns -1
		/// If FirstP is greater than NValues, returns -2
		/// FirstP starts to count on 0.
		/// </summary>
		/// <param name="Values1">Values1 = First Array of values from which to calculate the correlation coeficient.</param>
		/// <param name="Values2">Values2 = Second Array of values from which to calculate the correlation coeficient.</param>
		/// <param name="NValues">NValues = Number of values passed in the "Values" array.</param>
		/// <param name="UseValue">UseValue = if 1, the value is used. If 0, the values is not used.</param>
		/// <param name="FirstP">FirstP = First period to consider of the values, 0 all periods.</param>
        /// <returns> correlation coefficent value </returns>
        internal double CoefCorrelation(double[] Values1, double[] Values2, int NValues, int[] UseValue, int FirstP) {
			int NVal = 0;
			double DNVal;

			if (FirstP < 0) return (-1);
			if (FirstP > NValues) return (-2);

			for (int i = FirstP; i < NValues; i++) 
				if (UseValue [i] == 1) NVal++;
			DNVal = (Double) NVal;
            return ((DNVal / (DNVal - (double) 1.0)) * CoVarMuestral (Values1, Values2, NValues, UseValue, FirstP) /
				Math.Sqrt (VarMuestral (Values1, NValues, UseValue, FirstP) * VarMuestral (Values2, NValues, UseValue, FirstP)));
		}

        /// <summary>
        /// Returns the Correlation COeficient of array "Values1" and "Values2".
        /// </summary>
        /// <param name="Values1">Values1 = First Array of values from which to calculate the correlation coeficient.</param>
        /// <param name="Values2">Values2 = Second Array of values from which to calculate the correlation coeficient.</param>
        /// <returns> correlation coefficent value </returns>
        internal double CoefCorrelation(double[] Values1, double[] Values2) {
            if (Values1.Length != Values2.Length) {
                string msg =
                    string.Format("Strings.Both_Vectors_must_have_the_same_length,Values1.Length, Values2.Length");
                throw new ArgumentException(msg);
            }

            double DNVal = (Double)Values1.Length;
            return ((DNVal / (DNVal - 1.0)) * CoVarMuestral(Values1, Values2) /
                Math.Sqrt(VarMuestral(Values1) * VarMuestral(Values2)));
        }

        /// <summary>
		/// Returns if Values1 is correlated with Values2, being Value the % of 
		/// probability to achieve.
		/// If FirstP is less than 0, Exception detected
		/// If FirstP is greater than NValues, Exception detected
		/// FirstP starts to count on 0.
		/// </summary>
		/// <param name="Values1">Values1c= First Array of values with which calculate correlation.</param>
		/// <param name="Values2">Values2 = Second Array of values with which calculate correlation.</param>
		/// <param name="NValues">NValues = Number of values passed in the "Values" arrays.</param>
		/// <param name="UseValue">UseValue = if 1, the value is used. If 0, the values is not used.</param>
		/// <param name="MinProb">Percentage of probability with which the regressor must be selected.</param>
		/// <param name="FirstP">FirstP = First period to consider of the values, 0 all periods.</param>
		/// <returns> if the series are correlated </returns>
        internal bool Test_Correlation(double[] Values1, double[] Values2, int NValues, int[] UseValue, double MinProb, int FirstP) {
			double coefcorr, contraststat, limit;
			int nperiods = 0, NObs;
			if (FirstP < 0) 
			{
				throw new ArgumentException("Strings.Starting_Period_cannot_be_less_than_0");
			}

			if (FirstP > NValues) 
			{
				throw new ArgumentException("Strings.Starting_Period_cannot_greater_than_the_number_of_periods_in_the_Time_Series");
			}

			NObs = NValues - FirstP;
			for (int i = FirstP; i < NValues; i++)
				if (UseValue [i] == 1) nperiods++;

			limit = TStudent_quantil (MinProb / 100.0, nperiods - 2, true);
			coefcorr = CoefCorrelation (Values1, Values2, NValues, UseValue, FirstP);
			contraststat = coefcorr / Math.Sqrt ((1.0 - Math.Pow (coefcorr, 2.0)) / (NValues - 2.0));
			if (Double.IsNaN (contraststat)) return (true);
			// This is the contrast which appears on point 5 of the document.
			return (Math.Abs (contraststat) > limit );

			// This is the contrast which appears on point 6 of the document.
			// We need a function to convert the probability into the value of d*
			// double val = - (this.nobservations / 2) * Math.Log (1 - Math.Pow (coefcorr, 2)) + 0.5;
		}

        /// <summary>
        /// Returns if Values1 is correlated with Values2, being MinProb the % of 
        /// probability to achieve.
        /// </summary>
        /// <param name="Values1">Values1c= First Array of values with which calculate correlation.</param>
        /// <param name="Values2">Values2 = Second Array of values with which calculate correlation.</param>
        /// <param name="MinProb">Percentage of probability with which the regressor must be selected.</param>
        /// <returns> if series are correlated </returns>
        internal bool Test_Correlation(double[] Values1, double[] Values2, double MinProb) {
            double coefcorr, contraststat, limit;
            if (Values1.Length != Values2.Length) {
                string msg =
                    string.Format("Strings.Both_Vectors_must_have_the_same_length,Values1.Length, Values2.Length");
                throw new ArgumentException(msg);
            }

            limit = TStudent_quantil(MinProb / 100.0, Values1.Length - 2, true);
            coefcorr = CoefCorrelation(Values1, Values2);
            contraststat = coefcorr / Math.Sqrt((1.0 - Math.Pow(coefcorr, 2.0)) / (Values1.Length - 2.0));
            if (Double.IsNaN(contraststat)) return (true);
            // This is the contrast which appears on point 5 of the document.
            return (Math.Abs(contraststat) > limit);
        }

        /// <summary>
        /// Returns the value of the correlation contrast of Values1 being with Values2, being MinProb the % of 
        /// probability to achieve.
        /// </summary>
        /// <param name="Values1">Values1c= First Array of values with which calculate correlation.</param>
        /// <param name="Values2">Values2 = Second Array of values with which calculate correlation.</param>
        /// <param name="MinProb">Percentage of probability with which the regressor must be selected.</param>
        /// <returns> result of contrast </returns>
        internal double CorrelationContrast(double[] Values1, double[] Values2, double MinProb) {
            double coefcorr, contraststat, limit;
            if (Values1.Length != Values2.Length) {
                string msg = "Strings.Both_Vectors_must_have_the_same_length,Values1.Length, Values2.Length)";
                throw new ArgumentException(msg);
            }

            limit = TStudent_quantil(MinProb / 100.0, Values1.Length - 2, true);
            coefcorr = CoefCorrelation(Values1, Values2);
            contraststat = coefcorr / Math.Sqrt((1.0 - Math.Pow(coefcorr, 2.0)) / (Values1.Length - 2.0));
            if (Double.IsNaN(contraststat)) return (100.0);
            // This is the contrast which appears on point 5 of the document.
            return (Math.Abs(contraststat) / limit);
        }

        /// <summary>Rounds trigonometric function to get the closest number to c.</summary>
		/// <param name="c">c: Is the value to be rounded</param>
        /// <returns> result value </returns>
        internal double RoundTrigFunc(double c) {
			double Val = c;

			Val = (c < 0.00000001 && c > 0) ? (double) 0 : c;
			Val = (c > 0.99999999 && c < 1) ? (double) 1 : c;
			Val = (c > 0.49999999 && c < 0.5) ? (double) 0.5 : c;
			Val = (c > 0.5 && c < 0.50000001) ? (double) 0.5 : c;

			Val = (c > -0.00000001 && c < 0) ? (double) 0 : c;
			Val = (c < -0.99999999 && c > -1) ? (double) -1 : c;
			Val = (c < -0.49999999 && c > -0.5) ? (double) -0.5 : c;
			Val = (c < -0.5 && c > -0.50000001) ? (double) -0.5 : c;

			return (Val);
		}

        /** Returns the Correlation COeficient of array "Values1" and "Values2" */
        internal double StdError(double[] Values1, double[] Values2) {
            if (Values1.Length != Values2.Length)
                throw new ArgumentException("Both vectors must have the same size");
            double Err = 0.0;

            for(int i = 0;i < Values1.Length;i++)
                if(Values1[i] != 0.0)
                    Err += (Math.Abs(Values1[i] - Values2[i])) / Values1[i];
                else
                    Err += Math.Abs(Values2[i]);
            return Err / Values1.Length;
        }

		#endregion Descriptive statistical function

        #endregion    

        #region Standardize Methods

        /** Bias standardizing of one value 
        X  list of data for standardize 
        res  bias 
        index index of value */
        internal double StandardizedRes(double[] X, double[] res, int index) {
            int n = X.Length;
            double Xmean = Mean(X, n, 0);
            double hi = 1.0 / n + Math.Pow((X[index] - Xmean), 2) / VarMuestral(X, n, 0);
            double Sxy = 0.0;
            for (int i = 0; i < n; i++) {
                Sxy += Math.Pow(res[i], 2);
            }
            Sxy = Math.Sqrt(Sxy / n - 2);

            double SRi = res[index] / Sxy * Math.Sqrt(1 - hi);
            return SRi;
        }

        /* Values standardizing */
        internal List<double> StandarizeValues(List<double> data, double InflationFactor) {
            double mean = Mean(data.ToArray(), data.Count, 0);
            double stddev = Math.Sqrt(VarMuestral(data.ToArray(), data.Count, 0));
            List<double> v = new List<double>();

            for (int i = 0; i < data.Count; i++)
                v.Add((data[i] - mean / stddev) * InflationFactor);
            return v;
        }

        #endregion
        
        #region Moving Calculations

        internal List<double> MovingAverageCentered(List<double> values, int mw) {
            List<double> movingAverage = new List<double>();
            int ini = (int)mw / 2;
            int fin;
            double average = 0.0;
            //inicio
            for (int j = 0; j < mw; j++) {
                average += values[j];
            }
            for (int i = 0; i < ini; i++) {
                movingAverage.Add(average / mw);
            }
            //medio
            for (int i = ini; i < values.Count - ini; i++) {
                average = 0.0;
                fin = (mw % 2 == 0) ? i + ini : i + ini + 1;
                for (int j = i - ini; j < fin; j++) {
                    average += values[j];
                }
                movingAverage.Add(average / mw);
            }
            //final
            average = 0.0;
            for (int j = values.Count - mw; j < values.Count; j++) {
                average += values[j];
            }
            for (int i = values.Count - (mw - ini) - 1; i < values.Count; i++) {
                movingAverage.Add(average / mw);
            }
            return movingAverage;
        }


        internal List<double> MovingStDevCentered(List<double> values, int mw) {
            List<double> movingStDev = new List<double>();
            int ini = (int)mw / 2;
            int fin;
            double stDev;
            //inicio
            for (int i = 0; i < ini; i++) {
                stDev = StDev(values, 0, mw - 1);
                movingStDev.Add(stDev);
            }
            //medio
            for (int i = ini; i < values.Count - ini; i++) {
                fin = (mw % 2 == 0) ? i + ini : i + ini + 1;
                stDev = StDev(values, ini, fin);
                movingStDev.Add(stDev);
            }
            //final
            stDev = StDev(values, values.Count-mw, values.Count-1);
            for (int i = values.Count - (mw - ini) - 1; i < values.Count; i++) {
                movingStDev.Add(stDev);
            }
            return movingStDev;
        }


        #endregion
       

        #endregion
       
    }
}

       
       