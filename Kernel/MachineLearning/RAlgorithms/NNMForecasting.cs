#region Imports

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Maths;
using System.Dynamic;

#endregion

namespace MachineLearning {
    
    internal class NNMForecasting {

        #region Fields

        private string path;
        private string version;
        private AR ts;
        private int mw = 6;
        private int nHidden = 6;
        private double decay = -1;
        private int maxIt = 100;
        private DF data;
        private double alpha = 0.05;
        private int n = 1;

        private AR pValues;
        private double prop = -1;

        private AR tsRec;
        private AR tsReg;
        private AR reSt;

        private AR recurrFcst;
        private AR regeffFcst;
        private AR totalFcst;

        private NNForecasting nnf;
        private NNMultiRegression nnmr;
        private RFunctions rf;
        private AR calFcst;
 
        #endregion

        #region Constructor
        
        internal NNMForecasting(string path, string version, int mw, int nHidden, double dec, int maxIt, double alpha, int n) {
          this.path = path;
          this.version = version;
          this.nHidden=nHidden;
          this.decay=dec;
          this.mw=mw;
          this.maxIt=maxIt;
          this.alpha=alpha;
          this.n=n;
          this.rf = new RFunctions(path, version);
          this.nnmr = new NNMultiRegression(path, version);
          this.nnmr.LoadParamsNeuNet(nHidden, decay, maxIt);
        }

        #endregion
                
        #region Internal Methods

          internal void LoadData(AR ts, int[] cal, AR reCal, AR iniEff, AR endEff, double[][] regs) {
              this.ts=ts;
              if(cal !=  null && cal.Length >= ts.Length) {
                  AR tsCal = new AR(cal);
                this.ts = rf.EliminateHolidays(ts, tsCal); 
                this.calFcst = tsCal.SubAr(ts.Length+1,tsCal.Length);
              }
              dynamic res = rf.CalcRecurrentTs(this.ts, iniEff, endEff, this.alpha, this.n);
              this.tsRec = res.tsRec;
              this.pValues = res.pValues;
              this.prop = res.prop;
              this.tsReg = this.ts - this.tsRec;
              this.reSt = rf.GetPerStats(this.tsReg, reCal);
              regs[regs.Length] = this.reSt.ToArray(); //review
      
              this.nnf = new NNForecasting(this.path, this.version, this.mw, this.nHidden, this.decay, 4, 0.5, 5, this.maxIt, 1, 0.1, 0.1, false, cal, 1.3, 12, false);
              this.nnmr.LoadForCalculate(regs);
        }
    
        internal double[] Forecast(int horizon, int it, AR regsFcst, int[] iniEff, int[] endEff) {
          //if(horizon==0 | horizon > nrow(regsFcst)) { horizon=nrow(regsFcst) }
      
          this.recurrFcst = new AR(this.nnf.Forecast(horizon, 5));
          //this.regeffFcst = this.nnmr.Calculate(regsFcst.SubTs(1,horizon).ToArray());
          this.totalFcst = Join(this.recurrFcst, this.regeffFcst, iniEff, endEff);
          if(calFcst.Length >= horizon) { 
            this.recurrFcst = this.recurrFcst * calFcst.SubAr(1,horizon); 
            this.totalFcst = this.totalFcst * calFcst.SubAr(1,horizon); 
          }
          return this.totalFcst.ToArray();
        }


        #endregion

        #region Private Methods
        
        private AR Join(AR recurrFcst, AR regEffFcst, int[] reIniEff, int[] reEndEff) {
            int n = this.tsRec.Length;
            AR totalFcst = recurrFcst.Clone();
            int p = reIniEff.Length;
            for(int i=0;i<p;i++) {
                int ini = reIniEff[i] - n + 1;
                int end = reEndEff[i] - n + 1;
                for (int j = ini; j <= end; j++) { totalFcst[j] = totalFcst[j] + regEffFcst[i+1] / p; }
            }
            return totalFcst;
        }

        #endregion

    }
}
