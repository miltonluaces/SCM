#region Imports

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Maths;
using RDotNet;

#endregion

namespace MachineLearning {
    
    internal class NNForecasting : TsForecast, RNetter {

    #region Fields

    private RFunctions rf;
    private Norm tsNor;
    private RNet rnet;
    private RNNet rnn;
     
    private int mw=0;
    private double dec=0;
    private double range=0;
    private int it = 5;
    private int maxIt=0;
    private int nHidden=0;
    private double normFactor = 1;
    private double normMinFactor = 0;
    private double normMaxFactor = 1;
    private bool negAllowed = false;
    private int horizon=0;
    private int minDecExp=4;
    private double maxVarRatio=1.3;
    private int histForVR=6;
    private bool trace=false;
    
    private AR origTs;  
    private AR ts;
    private AR cal;
    private AR calFcst;
  
    private DF X;
    private AR Y;
    
    #endregion
    
    #region Constructor
    
    internal NNForecasting(string path, string version, int mw, int nHidden, double dec, int minDecExp, double range, int it, int maxIt, double normFactor, double normMinFactor, double normMaxFactor, bool negAllowed, int[] cal, double maxVarRatio, int histForVR, bool trace) {
      this.fcstMethod = FcstMethodType.NNFcsting;
      this.rf = new RFunctions(path, version);
      this.rnet = new RNet(path, version);
      this.rnn = new RNNet(path, version);
      if(cal != null) this.cal = new AR(cal);
      
      this.trace=trace;
    
      //init
      this.dec = dec;
      this.range = range;
      this.maxIt = maxIt;
      this.it = it;
      this.nHidden = nHidden;
      this.mw = mw;
      this.negAllowed = negAllowed;
      this.minDecExp = minDecExp;
      this.normMinFactor = normMinFactor;
      this.normMaxFactor = normMaxFactor;
      
      //var control
      this.maxVarRatio=maxVarRatio;
      this.histForVR=histForVR;
    
      //this.rNet.RequireLibrary("nnet");
    }
  
    #endregion

    #region Public Methods
    
    public double[] Forecast(int horizon, int it) {
      this.horizon = horizon;
      double bestMae = double.MaxValue;
      AR bestFcst = null; double bestDec=this.dec;
      for(int i=1;i<it;i++) {
        if(this.dec < 0 & i <= 4*this.minDecExp) { dec = rf.GetExponentBy5(this.minDecExp, i);  } else { dec = bestDec; }
        rnn.CreateNN(X, Y, nHidden, dec, maxIt);
        AR fcst = this.FcstFrom(this.ts.Length-this.mw+1, horizon);
        double mae = this.CrossValidation(horizon);
        double ratio = rf.CalcSdRatio(this.ts, this.histForVR, fcst);
        if (ratio <= this.maxVarRatio & mae < bestMae) { bestMae = mae; bestDec = dec; bestFcst = fcst; }
      }
      this.dec=bestDec;
      if(this.negAllowed==false) { bestFcst.ElimNegatives(); }
      AR forecast = this.tsNor.UnNormalize(bestFcst);
      if(this.cal != null && this.calFcst != null && this.calFcst.Length>= horizon) { forecast = forecast * this.calFcst.SubAr(1,horizon); }
      return forecast.ToArray();
    }
    
    public AR FcstFrom(int start, int horizon) {
      AR fcst = new AR(horizon);
      AR x = ts.SubAr(start, (start+this.mw-1));
      for(int i=1; i<=horizon; i++) {
        double y = rnn.Predict(x);
        fcst[i] = y;
        AR subTs = x.SubAr(2, this.mw);
        x = AR.C(subTs, y); 
      }
      return fcst;
    }
    
    public AR GetOneStepFcstTsfunction() {
      AR Osf = new AR(ts.Length-this.mw);
      for(int i=1;i<=ts.Length-this.mw;i++) {
        double osf = FcstFrom(i, 1)[1];
        osf = this.tsNor.UnNormalize(osf);
        Osf[i] = osf;
      }
      AR NaNs = AR.Rep(double.NaN, this.mw);
      Osf = AR.C(NaNs, Osf);
      return Osf;
    }
    
    #endregion

    #region TsForecast Implementation

    public override void Calculate() {
        this.ts = new AR(this.hist.ToArray());
    
        this.ts.ElimNegatives();
        this.tsNor = new Norm(ts, normFactor, normMinFactor, normMaxFactor);
        if (this.cal != null && this.cal.Length >= ts.Length) { ts = rf.EliminateHolidays(ts, this.cal); this.calFcst = this.cal.SubAr(ts.Length + 1, this.cal.Length); }
        if (normFactor <= 0) { this.tsNor.XMin = 0; this.tsNor.XMax = 1; this.tsNor.XRange = 1; }
        this.origTs = ts;
        this.ts = this.tsNor.Normalize(ts);

        this.X = this.GetX(this.ts, this.mw);
        this.Y = this.GetY(this.ts, this.mw);
    }

    public override double[] GetFcst(int horizon) {
        double[] forecast = Forecast(horizon, this.it); 
        return forecast;
    }

    public override void SetModel(object modelObj) {
    }

    public override object GetModel() {
        return null;
    }

    #endregion

    #region RNetter Implementation

    bool RNetter.JITEnabled() { return rnet.JITEnabled(); }
    string RNetter.GetWorkingDirectory() { return rnet.GetWorkingDirectory(); }
    void RNetter.SetSeed(int seed) { rnet.SetRSeed(seed); }
    string RNetter.GetPackageVersions() { return rnet.GetPackageVersions(); }

    #endregion

    #region Private Methods

    private double CrossValidation(int horizon) {
      double aec=0;
      for(int i=1;i<=ts.Length-mw-horizon+1;i++) {
        AR ouT = this.FcstFrom(i, horizon);
        ouT.ElimNegatives();
        AR exp = ts.SubAr(i+this.mw, i+this.mw+horizon-1);
        AR err = AR.Abs(ouT-exp);
        aec = aec + err.Sum()/horizon;
      }
      double mae = Math.Round((aec/ts.Sum(1, this.ts.Length-this.mw-horizon+1)*100),2);
      return mae;    
    }
    
    private double Mae(double ouT, double exp) {
      return Math.Abs(ouT-exp);
    }
    
    private DF GetX(AR ts, int mw) {
        DF X = new DF();
        for (int i = 1; i <= ts.Length - mw; i++) {
            AR x = ts.SubAr(i, mw + i - 1);
            X.AddColumn(x);
        }
        return X;
    }
    
    public AR GetY(AR ts, int mw) {
      return ts.SubAr(mw+1, ts.Length);
    }
    
    public AR GetLast(AR ts, int n) {
      return( ts.SubAr(ts.Length-n+1, ts.Length));
    }
    
    public void SetNames() {
      string[] xNames = new string[this.mw];
      for(int i=1;i<=this.mw;i++) { xNames[i] = "Tn_" + (mw-i+1); }
      IList<RNet.Params> pars = new List<RNet.Params>(); 
      pars.Add(new RNet.Params("X", RNet.ParamType.Dataframe, this.X));
      pars.Add(new RNet.Params("xNames", RNet.ParamType.String, X));
      rnet.Execute("names(X) = xNames", pars);
      rnet.Execute("colnames(this.Y) = 'Tn'");
    }

    #endregion

  }
}
