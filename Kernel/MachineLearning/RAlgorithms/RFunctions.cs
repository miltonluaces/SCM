#region Imports

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Maths;
using RDotNet;
using System.Dynamic;

#endregion

namespace MachineLearning {

    internal class RFunctions {

        #region Fields

        private RNetFunctions rnf;

        #endregion

        #region Constructor

        internal RFunctions(string path, string version) {
            rnf = new RNetFunctions(path, version);
        }

        #endregion

        #region Internal Methods

        #region MathFunctions

        internal double LineEq(double x, double x1, double y1, double x2, double y2) {
          if(x1 == x2) return(0);
          double a = (y2-y1) / (x2 - x1);
          double b =  -a * x1 + y1;
          double y = a * x + b;
          return y;
        }
        
        internal double GetExponentBy5(int minDecExp, int i) {
          int j=i/2;
          double e = Math.Pow(10, -(minDecExp-j % minDecExp));
          if (i % 2 == 0) { return e; } else { return 5 * e; }
        }

        #endregion

        #region TsPreprocess

        internal AR EliminateHolidays(AR ts, AR cal) {
          int ini, end;
          AR newTs = AR.Rep(0,ts.Length);
          if (cal[1] == 0) newTs[1] = ts[2]; else newTs[1] = ts[1]; 
          int i=2;
          while(i<=ts.Length-1) {
            if(cal[i]==0) {
              ini=i-1; end=i+1; int j=1;
                while(cal[i+j]==0) { j=j+1; end=i+j; } 
              if(end-ini==2) { 
                  newTs[i]=(ts[ini]+ts[end])/2; 
              }
              else {
                  for (int k = ini + 1; k <= end - 1; k++ ) {
                      newTs[k] = LineEq(k, ini, ts[ini], end, ts[end]);
                  }  
              }
              i=end;
            }  
            else{
              newTs[i]=ts[i]; i=i+1;
            }
          }
          if(cal[ts.Length]==0) newTs[ts.Length]=ts[(ts.Length-1)]; else newTs[ts.Length]=ts[ts.Length]; 
          return newTs;
        }

    
        internal double CalcSdRatio(AR hist, int mw, AR fcst) {
          if(fcst.Length <= 1) { return 1; }
          int n = hist.Length;
          mw = Math.Min(mw, n);
          AR last = hist.SubAr(n-mw,n);
          double ratio = fcst.Sd()/last.Sd();
          return Math.Round(ratio, 3);
        }

        internal AR GetPerStats(AR ts, AR re) {
          AR perStats = new AR(ts.Length);
          bool regEffPer = false;
          int ini = 0;
          for(int i=2;i<=ts.Length;i++) {
            if(re[i]==0 & regEffPer == true) { 
                double perStat = ts.SubAr(ini+1,i-1).Mean();
                for (int j = ini + 1; j < i; j++) { perStats[j] = perStat; }
                regEffPer = false; 
            }
            if(re[i]==1) {
                if (regEffPer == false) { ini = i - 1; regEffPer = true; }
            }
          }
          return perStats;
        }

        #endregion

        #region HypoTests

        internal dynamic CalcRecurrentTs(AR ts, AR iniEff, AR endEff, double alpha, int n) {
            AR tsRec = ts.Clone();
            AR pValues = new AR(iniEff.Length);
            if(iniEff.Length != endEff.Length) { throw new Exception("Error: different effect lengths"); }
            int hits=0;
            for(int i=1;i<=pValues.Length;i++) {
                int s = (int)(endEff[i]-iniEff[i]+1); 
                int iniCtrl = (int)iniEff[i]-s*n; if(iniCtrl<1) iniCtrl=1;
                int endCtrl = (int)iniEff[i]-1;
                AR tsEff = ts.SubAr((int)iniEff[i],(int)endEff[i]);
                AR tsCtrl = tsRec.SubAr(iniCtrl,endCtrl);
                double m = tsCtrl.Mean();
                AR.Copy(AR.Rep(m, s), tsRec, 1, s, (int)iniEff[i], (int)endEff[i]);
               
                pValues[i] = rnf.WicoxTest(tsEff, tsCtrl);
                if(pValues[i] <= alpha) hits=hits+1;
            }
            double prop = hits/pValues.Length;

            dynamic res = new ExpandoObject();
            res.tsRec = tsRec;
            res.pValues = pValues;
            res.prop = prop;
            return res;
        }
        
        #endregion

        #region Statistical Methods

        internal double Variance(IList<double> values) {
            int n = values.Count;
            if (n == 1) { return 0; }
            double Sum = 0.0, SumSquares = 0.0;

            for (int i = 0; i < n; i++) {
                Sum += values[i];
                SumSquares += values[i] * values[i];
            }
            return (n * SumSquares - (Sum * Sum)) / (n * n - 1);
        }

        internal double Sd(IList<double> values) {
            return Math.Sqrt(Variance(values));
        }


        #endregion

        #endregion

    }
}
