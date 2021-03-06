#region Imports

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Statistics;
using Maths;

#endregion


namespace MachineLearning {

    internal class CandidatePars {

        #region Fields

        private DLModel structure;
        private DLMDistribution dist;
        private Dictionary<int, ModelValue> model_hist;
        private Dictionary<int, ModelValue> model_fcst;
        private int zeroerrorperiods;
        private double mse;
        private Dictionary<int, double[,]> ListOfPostM;

        internal struct Weight {
            internal String Name;
            internal double Value;
            internal double AbsImportance;
            internal double RelImportance;
        }

        List<Weight> CompWeights;

      
        private double[] dfNormal;
        private Dictionary<int, double[]> dfAlters;
        private int nalters;
        
        private int LastDegPN;
        private DLMDistribution degdist;
        private Frequencies residualfrq;
        private List<double> LastDegResiduals;

        private double varMean;
        private double varIndex;
        private double stderror;

        #endregion 

        #region Constructors

        internal CandidatePars(DLModel structure, double pol, double sea, double reg, double var) {
            this.structure = structure;
            this.dfNormal = new double[4];
            this.dfNormal[0] = pol;
            this.dfNormal[1] = sea;
            this.dfNormal[2] = reg;
            this.dfNormal[3] = var;
            this.dfAlters = new Dictionary<int, double[]>();
            this.residualfrq = new Frequencies();
            this.dist = new DLMDistribution();
            model_hist = new Dictionary<int, ModelValue>();
            model_fcst = new Dictionary<int, ModelValue>();
            ListOfPostM = new Dictionary<int, double[,]>();
            this.zeroerrorperiods = 0;
            this.mse = -1;
        }

        internal CandidatePars(DLModel structure, double[] dfNormal, Dictionary<int, double[]> dfAlters) {
            this.dfNormal = dfNormal;
            this.dfAlters = dfAlters;
            this.dfAlters = new Dictionary<int, double[]>();
        }

        #endregion

        #region Properties

        internal DLModel Structure {
            get { return structure; }
            set { structure = value; }
        }

        internal int NAlters {
            get { return nalters; }
            set { nalters = value; }
        }

        internal Frequencies ResidualFrq {
            get { return residualfrq; }
            set { residualfrq = value; }
        }

        internal double VarMean {
            get { return varMean; }
            set { varMean = value; }
        }

        internal double VarIndex {
            get { return varIndex; }
            set { varIndex = value; }
        }

        internal double StdError {
            get { return stderror; }
            set { stderror = value; }
        }

        internal List<double> seovertime = new List<double>();
        
        #endregion

        #region Internal Methods

        internal int GetZeroErrorPeriods() {
            return this.zeroerrorperiods;
        }

        internal void IncrementZeroErrorPeriods() {
            this.zeroerrorperiods++;
        }

        internal void InitZeroErrorPeriods() {
            this.zeroerrorperiods = 0;
        }

        internal void SetZeroErrorPeriods(int v) {
            this.zeroerrorperiods = v;
        }

        internal ModelValue GetModelValue(int PN) {
            return model_hist[PN];
        }

        internal bool IsModelCalculated() {
            return (model_hist != null);
        }

        internal void SetModelValue(int PN, ModelValue M) {
            if (model_hist.ContainsKey(PN))
                model_hist[PN] = M;
            else
                model_hist.Add(PN, M);
        }

        internal void AddPostM(int PN, double[,] M) {
            if (ListOfPostM.ContainsKey(PN))
                ListOfPostM[PN] = M;
            else
                ListOfPostM.Add(PN, M);
        }

        internal double[,] GetPostM(int PN) {
            if (ListOfPostM.ContainsKey(PN))
                return ListOfPostM[PN];
            return null;
        }

        internal double[,] GetMean() {
            if (dist != null && dist.Mean != null) {
                return MatrixOp.Clone(this.dist.Mean);
            }
            else {
                return null;
            }
        }

        internal void SetMean(double[,] V) {
            if (dist == null) { dist = new DLMDistribution(); }
            this.dist.Mean = V;
        }

        internal double[,] GetVar() {
            if (dist == null || dist.Var == null) { return null; }
            return MatrixOp.Clone(this.dist.Var);
        }

        internal void SetVar(double[,] V) {
            if (dist == null) { dist = new DLMDistribution(); }
            this.dist.Var = V;
        }

        internal double GetLearnN() {
            if (dist == null) { return -1; }
            return this.dist.Dof;
        }

        internal void SetLearnN(double v) {
            if (dist == null) { dist = new DLMDistribution(); }
            this.dist.Dof = v;
        }

        internal double GetLearnS() {
            return this.dist.S;
        }

        internal void SetLearnS(double v) {
            if (dist == null) { dist = new DLMDistribution(); }
            this.dist.S = v;
        }

        internal DLModel GetStructure() {
            return (this.structure);
        }

        internal void SetModelFcst(int PN, ModelValue M) {
            if (model_fcst == null) { model_fcst = new Dictionary<int, ModelValue>(); }
            if (model_fcst.ContainsKey(PN)) {
                this.model_fcst[PN] = M;
            }
            else {
                this.model_fcst.Add(PN, M);
            }
        }

        internal void InitModelFcst() {
            this.model_fcst = new Dictionary<int, ModelValue>();
            if (this.structure.GetMatrixG() == null) this.structure.CalcGMatrix();
        }

        internal ModelValue GetModelFcst(int PN) {
            return this.model_fcst[PN];
        }

        internal Dictionary<int, ModelValue> GetModelFcst() {
            return this.model_fcst;
        }

        internal List<double> GetNMeanFcst() {
            List<double> F = new List<double>();

            foreach (int k in this.model_fcst.Keys)
                F.Add(this.model_fcst[k].NMean);
            return F;
        }

        internal List<double> GetNVarFcst() {
            List<double> F = new List<double>();

            foreach (int k in this.model_fcst.Keys)
                F.Add(this.model_fcst[k].NVar);
            return F;
        }

        internal double GetMSE() {
            if (this.model_hist == null) { return 0; }
            return this.mse;
        }

        internal double SetMSE(double v) {
            return this.mse = v;
        }

        /// <summary>Calculate sthe importance or weightings of each component</summary>
        internal void CalcWeightings() {
            Weight W;
            W.Name = "";
            W.Value = 0.0;
            List<Weight> L = new List<Weight>();
            for (int i = 1; i < this.structure.GetPolSeasonalComponents().Length; i++) {
                ArrayList ListOfParams = this.structure.GetPolSeasonalComponents()[i];
                DLMParameter p = ((DLMParameter)ListOfParams[0]);
                if (p.IsActive()) {
                    W.AbsImportance = 0.0;
                    W.RelImportance = 0.0;
                    int col = p.GetParamNumber();
                    switch (p.GetParamName()) {
                        case ("Level"):
                            W.Name = "Polinomial";
                            W.Value = Math.Abs(this.dist.Mean[col, 0]);
                            break;

                        case ("Cos"):
                            W.Name = "Armonic " + p.GetArmonicNumber();
                            if (ListOfParams.Count == 1)
                                W.Value = Math.Abs(this.dist.Mean[col, 0]);
                            else
                                W.Value = Math.Sqrt((Math.Pow(this.dist.Mean[col, 0], 2) + Math.Pow(this.dist.Mean[col + 1, 0], 2)));
                            break;
                        default:
                            break;
                    }
                    L.Add(W);
                }
            }
      
            double suma = 0.0;
            double Pol = 0.0;
            foreach (Weight w in L)
                if (w.Name == "Polinomial")
                    Pol = w.Value;
                else
                    suma += w.Value;
            for (int i = 0; i < L.Count; i++) {
                Weight auxw = L[i];
                auxw.AbsImportance = L[i].Value / (suma + Pol);
                auxw.RelImportance = L[i].Value / suma;
                L[i] = auxw;
            }
            this.CompWeights = L;
        }

        internal double[] DFNormal() {
            return dfNormal;
        }

        internal Dictionary<int, double[]> DFAlters() {
            return dfAlters;
        }

        internal double[] GetDFAlter(int time) {
            if (dfAlters.ContainsKey(time))
                return dfAlters[time];
            else
                return null;
        }

        internal void AddDFAlter(int PNumber, double[] df) {
            if (dfAlters.ContainsKey(PNumber))
                dfAlters[PNumber] = df;
            else {
                this.dfAlters.Add(PNumber, df);
                this.NAlters++;
            }
        }

        internal void SetDFAlter(int PNumber, double[] df) {
            if (dfAlters.ContainsKey(PNumber))
                dfAlters[PNumber] = df;
        }

        internal double[] GetDF(int PN) {
            if (dfAlters.ContainsKey(PN)) { return dfAlters[PN]; } else { return dfNormal; }
        }

        internal bool IsDegenerated(int PN) {
            return (dfAlters.ContainsKey(PN));
        }

        internal double[,] GetMeanDeg() {
            if (degdist == null || degdist.Mean == null) {
                return null;
            }
            return MatrixOp.Clone(this.degdist.Mean);
        }

        internal void SetMeanDeg(double[,] v) {
            if (degdist == null) {
                degdist = new DLMDistribution();
            }
            this.degdist.Mean = MatrixOp.Clone(v);
        }

        internal double[,] GetVarDeg() {
            if (degdist == null || degdist.Var == null) {
                return null;
            }
            return MatrixOp.Clone(this.degdist.Var);
        }

        internal void SetVarDeg(double[,] v) {
            if (degdist == null) {
                degdist = new DLMDistribution();
            }
            this.degdist.Var = MatrixOp.Clone(v);
        }

        internal double GetLearnNDeg() {
            if (degdist == null) { return -1; }
            return this.degdist.Dof;
        }

        internal void SetLearnNDeg(double v) {
            if (degdist == null) {
                degdist = new DLMDistribution();
            }
            this.degdist.Dof = v;
        }

        internal double GetLearnSDeg() {
            if (degdist == null) {
                return -1;
            }
            return this.degdist.S;
        }

        internal void SetLearnSDeg(double v) {
            if (degdist == null) {
                degdist = new DLMDistribution();
            }
            this.degdist.S = v;
        }

        internal int GetLastDegPN() {
            return this.LastDegPN;
        }

        internal void SetLastDegPN(int PN) {
            this.LastDegPN = PN;
        }

        internal void SetLastDegResiduals(List<double> l) {
            this.LastDegResiduals = l;
        }

        internal void AddLastDegResiduals(double v) {
            if (this.LastDegResiduals == null)
                this.LastDegResiduals = new List<double>();
            this.LastDegResiduals.Add(v);
        }

        internal void ClearLastDeg() {
            this.LastDegPN = 0;
            this.degdist = new DLMDistribution();
            this.LastDegResiduals = new List<double>();
        }

        internal List<double> GetLastDegResiduals() {
            return this.LastDegResiduals;
        }

        internal Frequencies GetResidualFrq() {
            return this.residualfrq;
        }

        internal void SetResidualFrq(Frequencies TF) {
            this.residualfrq = TF;
        }

        #endregion

        #region Private Methods

        internal void ResetModelHist(int firstPN, int lastPN) {
            model_hist = new Dictionary<int, ModelValue>();
            ModelValue mv;
            for (int i = firstPN; i <= lastPN; i++) {
                mv = new ModelValue();
                mv.NNMean = 0.0;
                mv.NNVar = 0.0;
                model_hist.Add(i, mv);
            }
        }

        #endregion

    }

    #region Class DLMDistribution

    internal class DLMDistribution {

        #region Fields

        private double[,] mean;
        private double[,] var;
        private double dof;
        private double s;

        #endregion

        #region Properties

        internal double[,] Mean {
            get { return mean; }
            set { mean = value; }
        }

        internal double[,] Var {
            get { return var; }
            set { var = value; }
        }

        internal double Dof {
            get { return dof; }
            set { dof = value; }
        }

        internal double S {
            get { return s; }
            set { s = value; }
        }

        #endregion

    }

    #endregion

    #region Struct Model Value

    internal struct ModelValue {
        internal double NNMean;
        internal double NNVar;
        internal double NMean;
        internal double NVar;
        internal double dof;
        internal double S;
    }

    #endregion

    
}



