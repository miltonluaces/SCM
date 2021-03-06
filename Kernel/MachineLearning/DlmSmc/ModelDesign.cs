#region Imports

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Statistics;

#endregion

namespace MachineLearning {

    internal class ModelDesign {

        #region Fields

        private StatFunctions stat;
        private HypoTest ht;
        private int minTestForHarms;
    
        #endregion

        #region Constructor

        internal ModelDesign() {
            stat = new StatFunctions();
            double significance = 0;
            int minCount = 0;
            double minHarmContrib = 0.02;
            ht = new HypoTest(minHarmContrib / 100.0, 0);
            this.minTestForHarms = 6;
        }

        #endregion

        #region Internal Methods

        #region Build Model Matrices

        internal double[] BuildF(int nSeasons) { 
            if(nSeasons == 0) { 
                double[] F = { 1, 0 }; return F;
            }
            else {
                double[] F = new double[2 * nSeasons + 3];
                for (int i = 0; i < F.Length; i=i+2) { F[i] = 1; }
                return F;
            }
        }

        internal double[,] BuildG(List<Harm> seasons) {
            int p = 2;
            if (seasons.Count > 0) { p = 2 * seasons.Count + 3; } //poly y Niquist
            //polynomial
            double[,] G = new double[p, p];
            G[0, 0] = 1;  G[0, 1] = 1;
            G[1, 0] = 0;  G[1, 1] = 1;
            //seasonal
            int dia = 2;
            for (int i = 0; i < seasons.Count; i++) { 
                double w = (2 *Math.PI)/seasons[i].period;
                G[dia, dia] = Math.Cos(w);
                G[dia, dia+1] = Math.Sin(w);
                G[dia + 1, dia] = -G[dia, dia+1]; 
                G[dia+1, dia+1] = G[dia, dia];
                dia = dia+2;
            }
            //niquist
            if (seasons.Count > 0) { G[p - 1, p - 1] = -1; }
            return G;
        }

        internal double GetV(IList<double> hist) {
            return stat.StDev(hist);
        }

        internal double[,] GetW(IList<double> hist, int p) {
            double[,] W = new double[p, p];
            for (int i = 0; i < p; i++) {
                for (int j = 0; j < p; j++) {
                    W[i, j] = 0.1;  //TODO: replace 
                }
            }
                return W;
        }

        #region Initial Distribution 

        internal double[] GetM0(IList<double> hist, int p) {
            double[] m0 = new double[p];   //TODO: replace 
            return m0;
        }

        internal double[,] GetC0(IList<double> hist, int p) {
            double[,] c0 = new double[p, p];   //TODO: replace 
            return c0;
        }

        #endregion

        #region HarmonicSelection

        internal List<Harm> SelectHarmonics(List<double> hist, int testSetSize, double minImprov) {
            DLM dlm;
            List<Harm> harms = new List<Harm>();
            dlm = BuildModel(hist, harms);
            dlm.Iterate(hist);
            double bestMae = dlm.GetCrossValMae(testSetSize);
            int maxHarm = Math.Min(12, testSetSize - 1);
            for (int p = 2; p <= 12; maxHarm++) { 
                Harm harm = new Harm(1,p, false);
                harms.Add(harm);
                dlm = BuildModel(hist, harms);
                dlm.Iterate(hist);
                double mae = dlm.GetCrossValMae(testSetSize);
                if ((bestMae - mae) / bestMae > minImprov) { bestMae = mae; }
                else { harms.RemoveAt(harms.Count - 1); }
            }
            return harms;
        }

        internal List<Harm> SelectHarmonicsForwBack(IList<double> hist, int testSetSize, double alpha) {
            List<Harm> tryHarms = new List<Harm>();
            if (testSetSize < minTestForHarms) { return tryHarms; }
            List<int> harmIndexes = new List<int>();
            List<double> testSet = new List<double>();
            for (int i = hist.Count - testSetSize; i < hist.Count; i++) { testSet.Add(hist[i]); }
            DLM dlm = BuildModel(hist, tryHarms);
            dlm.Iterate(hist);
            double[] reducedModelFcst = dlm.GetFcstsMean(testSetSize);
            int prm = 2;
            int maxHarm = Math.Min(12, testSetSize - 1);
       
            List<Harm> harms = new List<Harm>();
            for (int p = 0; p <= maxHarm; p++) { harms.Add(new Harm(p, p, false)); }
            
            int it = 1;
            while (it <= 1) {
                //forward step
                harmIndexes.Clear();
                for (int p = 2; p <= maxHarm; p++) {
                    harms[p].active = true;
                    tryHarms.Clear();
                    foreach (Harm h in harms) { if (h.active) { tryHarms.Add(h); } }
                    harms[p].active = false;
                    if (tryHarms.Count == 0) { continue; } 
              
                    dlm = BuildModel(hist, tryHarms);
                    dlm.Iterate(hist);
                    double[] harmModelFcst = dlm.GetFcstsMean(testSetSize);
                    bool isRelevant = ht.AnovaNestedModels(testSet, reducedModelFcst, harmModelFcst, prm, prm + tryHarms.Count, alpha);
                    if (isRelevant) { harmIndexes.Add(p); }
                }
                foreach (int index in harmIndexes) { harms[index].active = true; }

                //backward step
                harmIndexes.Clear();
                for (int p = 2; p < harms.Count; p++) {
                    if (!harms[p].active) { continue; }
                    harms[p].active = false;
                    tryHarms.Clear();
                    foreach (Harm h in harms) { if (h.active) { tryHarms.Add(h); } }
                    harms[p].active = true;
                    if (tryHarms.Count == 0) { continue; } 
                    
                    dlm = BuildModel(hist, tryHarms);
                    dlm.Iterate(hist);
                    double[] harmModelFcst = dlm.GetFcstsMean(testSetSize);
                    bool isRelevant = ht.AnovaNestedModels(testSet, reducedModelFcst, harmModelFcst, prm, prm + tryHarms.Count, alpha);
                    if (!isRelevant) { harmIndexes.Add(p); }
                }
                foreach (int index in harmIndexes) { harms[index].active = false; }
                it++;
            }

            tryHarms.Clear();
            foreach (Harm h in harms) { if (h.active) { tryHarms.Add(h); } }
            return tryHarms;
        }

        #endregion

        #region Model Building

        internal DLM BuildModel(IList<double> hist, List<ModelDesign.Harm> harms) {
            double[] F = BuildF(harms.Count);
            double[,] G = BuildG(harms);

            int p = 2;
            if (harms.Count > 0) { p = 2 * harms.Count + 3; }
            double v = GetV(hist);
            double[,] W = GetW(hist, p);

            DLM dlm = new DLM(hist.Count, p);
            dlm.LoadModel(F, G, v, W);

            double[] m0 = GetM0(hist, p);
            double[,] C0 = GetC0(hist, p);
            dlm.Initalize(m0, C0);
            return dlm;
        }

        #endregion


        #endregion

        #region Private Methods
        #endregion

        #region Inner Classes

        internal class Harm {
            internal int index;
            internal double period;
            internal bool active;

            internal Harm(int index, int period, bool active) {
                this.index = index;
                this.period = period;
                this.active = active;
            }
        }
        
        #endregion

        #endregion

    }
}
