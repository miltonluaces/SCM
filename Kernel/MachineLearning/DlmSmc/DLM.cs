#region Imports

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Statistics;
using Maths;
using System.IO;
using System.Diagnostics;
using System.Runtime.CompilerServices;

#endregion

namespace MachineLearning {

    internal class DLM {
		
        #region Fields

        private NormalDistrib normal;
        private RndGenerator rnd;

        private int n;       //number of observed data
        private int p;       //number of known coefficients

        private List<double> y;  //observed data vector
        private Matrix Yt;   //observed datum matrix
        private Matrix Tt;   //state vector (Theta) 
        private Matrix F;    //design vector
        private Matrix G;    //evolution matrix 
        private Matrix V;    //observation variance  (Nu)
        private Matrix W;    //evolution covariance matrix (Omega)
        private Matrix w;    //error term par (omega)

        private List<Matrix> m;    //parameter mean vector
        private List<Matrix> C;    //parameter Covariance matrix

        private int t;             //iteration time
        private Matrix at;
        private Matrix Rt;
        private Matrix Qt;
        private Matrix Qti;
        private Matrix At;
        private Matrix ft;

        private double mse;
     
        private bool writeLog;
        private StreamWriter log;
        private string logPath;
        
        #endregion

        #region Constructor

        internal DLM(int n, int p) {
            this.n = n;
            this.p = p;
            this.y = new List<double>();
 
            F  = new Matrix(p, 1);
            G  = new Matrix(p, p);
            V  = new Matrix(1, 1);
            W  = new Matrix(p, p);
            Yt = new Matrix(1, 1);
            Tt = new Matrix(p, 1);
            w = new Matrix(p,1);

            m = new List<Matrix>();
            Matrix m0 = new Matrix(p,1);
            m.Add(m0);
            C = new List<Matrix>();
            Matrix C0 = new Matrix(p,p);
            C.Add(C0);

            normal = new NormalDistrib();
            rnd = new RndGenerator();

            mse = 0;
       
            logPath = @"..\Files\ailsLog.txt";
            //try { log = new StreamWriter(logPath); }
            //catch { Console.WriteLine("Error. Log not found"); }
            writeLog = false;
        }

        #endregion

        #region Internal Properties

        internal List<double> Y {
            get { return y; }
        }

        internal List<Matrix> M {
            get { return m; }
        }

        internal List<Matrix> Ct {
            get { return C;}
        }

        internal bool WriteLog {
            get { return writeLog; }
            set { writeLog = value; }
        }

        #endregion

        #region Internal Methods

        #region Main Methods

        internal void LoadModel(double[] F, double[,] G, double v, double[,] W) {
            this.F.Set(F, false);
            this.G.Set(G);
            ((IMatrix)this.V)[0, 0] = v;
            this.W.Set(W);
        }

        internal void Initalize(double[] m0, double[,] C0) {
            this.m[0].Set(m0, false);
            this.C[0].Set(C0);
            this.w.Set(Getw(), false);
            this.Tt.SetNormal(m0, C0);
            this.t = 0;
        }

        internal void Iteration(double yt) {
            y.Add(yt);
            ((IMatrix)Yt)[0, 0] = y[t];
            at = Calca(t);
            ft = Calcf(at);
            Rt = CalcR(t);
            Qt = CalcQ(Rt);
            Qti = (Matrix)((IMatrix)Qt).Inverse;
            At = CalcA(Rt, Qti);

            Matrix mt = CalcM(Yt, ft, at, At);  m.Add(mt);
            Matrix Ct = CalcC(Rt, At, Qti); C.Add(Ct);

            t++;
        }

        internal void Iterate(IList<double> y) {
            for (int t = 0; t < y.Count; t++) { Iteration(y[t]); }
        }

        //one step forecast mean
        internal double GetFcstMean() {
            return ((IMatrix)ft).ToScalar();
        }

        //one step forecast var
        internal double GetFcstVar() {
            return ((IMatrix)Qt).ToScalar();
        }

        //k step forecast mean
        internal double GetFcstMean(int k) {
            Matrix mk = F.t() * G.Pow(k) * m[t];
            return ((IMatrix)mk).ToScalar();
        }

        //k steps forecast mean
        internal double[] GetFcstsMean(int k) {
            double[] fcst = new double[k];
            for (int i = 0; i < k; i++) {
                fcst[i] = GetFcstMean(i+1);
            }
            return fcst;
        }
        
        //k step forecast var
        internal double GetFcstVar(int k) {
            Matrix Gk = G.Pow(k);
            Matrix vk = F.t() * Gk * C[t] * Gk.t() * F + /*sum i=0 to k*/ F.t() * G.Pow(k).t() * F + V;
            return ((IMatrix)vk).ToScalar();
        }
        
        #endregion

        #region Monitoring

        internal double GetMse() {
            return mse / n; 
        }

        //one step forecast mean
        internal double GetOneStepRetroFcstMean(int t) {
            Matrix a_t = Calca(t);
            Matrix f_t = Calcf(a_t);
            return ((IMatrix)f_t).ToScalar();
        }

        //k step forecast mean
        internal double GetRetroFcstMean(int k) {
            Matrix mk = F.t() * G.Pow(k) * m[t-k];
            return ((IMatrix)mk).ToScalar();
        }

        //k steps forecast mean
        internal double[] GetRetroFcstsMean(int k) {
            double[] fcst = new double[k];
            for (int i = k; i > 0; i--) {
                fcst[k-i] = GetRetroFcstMean(i);
            }
            return fcst;
        }

        internal double[] GetRetroFcstsError(int t) {
            double[] fcst = GetRetroFcstsMean(t);
            double[] error = new double[t];
            for (int i = y.Count - t; i < y.Count; i++) {
                error[i - (y.Count - t)] = y[i] - fcst[i - (y.Count - t)];
            }
            return error;
        }

        internal double GetCrossValMae(int t) {
            double[] error = GetRetroFcstsError(t);
            double sumAb = 0;
            for (int i = 0; i < error.Length; i++) { sumAb += Math.Abs(error[i]); }
            return sumAb / error.Length;
        }
        
        #endregion

        #region Trace

        internal void WriteLogMsg(string msg) {
            if (writeLog) { log.WriteLine(msg); }
        }

        internal void WriteLogMatrix(string title) {
            if (!writeLog) { return; }
            log.WriteLine(title);
            log.WriteLine("");
            log.WriteLine("T:\tMatrix F:\t\tMatrix G:");
            log.WriteLine("-----\t-----\t--------------------\t--------------------");
            for (int i = 0; i < n; i++) {
                if (i < p) { log.Write(((IMatrix)Tt)[i, 0].ToString("0.00")); }
                log.Write("\t");
                for (int j = 0; j < p; j++) {
                    log.Write(((IMatrix)F)[i, j].ToString("0.00") + "\t");
                    if (i < p) { log.Write(((IMatrix)G)[i, j].ToString("0.00") + "\t"); }
                }
                log.WriteLine("");
            }
            log.WriteLine("");
        }

        internal void CloseLog() {
            if (writeLog) { log.Close(); }
        }

        #endregion
        
        #endregion
         
        #region Private Methods

        #region Dlm Equations

        //Observation equation: (1x1) = (1xp) * (px1) + (1x1)
        private void ObservationEquation() { 
            Matrix Yt = F.t() * Tt + V;
        }
        
        //Evolution equation: (px1) = (pxp) * (px1) + (px1)
        private void EvolutionEquation() { 
            Tt = G * Tt + w;
        }

        #endregion

        #region Error terms

        private double[] Getv() {
            double[] v = new double[n];
            for (int i = 0; i < n; i++) { v[i] = rnd.NextNormal(0, ((IMatrix)V)[i, 0]); } //TODO sustituir V[i,0]
            return v;
        }

        private double[] Getw() {
            double[] w = new double[p];
            for (int i = 0; i < p; i++) { w[i] = rnd.NextNormal(0, ((IMatrix)W)[i,0]); }  //TODO: W[i,0] sustituir
            return w;
        }

        #endregion

        #region Get Parameters

        //a: (px1) = (pxp) * (px1) 
        private Matrix Calca(int t) { 
            return G * m[t]; 
        }

        //f: (1x1) = (1xp) * (px1)
        private Matrix Calcf(Matrix at) {
            return F.t() * at;
        }
        
        //R: (pxp) = (pxp) * (pxp) * (pxp) + (pxp)
        private Matrix CalcR(int t) { 
            return G * C[t] * G.t() + W;  
        }
        
        //Q: (1x1) = (1xp) * (pxp) * (px1) + (1x1)
        private Matrix CalcQ(Matrix Rt) { 
            return F.t() * Rt * F + V;  
        }

        //A: (px1) = (pxp) * (px1) * (1x1)
        private Matrix CalcA(Matrix Rt, Matrix Qti) {
            return Rt * F * Qti;
        }
        
        //W: Wt = (d / (1-d)) * Ct-1
        private Matrix CalcW(Matrix C, double d) { 
            return (Matrix)((IMatrix)C).Multiply(d/(1-d));
        }

        #endregion

        #region Kalman filter
         
        //M: (px1) = (px1) + (px1) * [(1x1) - (1x1)]
        private Matrix CalcM(Matrix Yt, Matrix ft, Matrix at, Matrix At) {
            return at + At * (Yt - ft);
        }

        //C: (pxp) = (pxp) - (px1) * (1x1) * (1xp)
        private Matrix CalcC(Matrix Rt, Matrix At, Matrix Qti) {
            return Rt - At * Qti * At.t();
        }
        
        #endregion

        #endregion
    }
}
