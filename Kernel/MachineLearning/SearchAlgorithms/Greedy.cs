#region Imports

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Maths;

#endregion

namespace MachineLearning {

    internal class Greedy : SearchAlg {

            #region Fields

            private int[] order;
            private double eps;
            private int iteracion;
            private int maxIteraciones;

            #endregion

            #region Constructor

            internal Greedy(int nGenes, int dim, double[] mins, double[] maxs, IMeritFunction fo, IValidation va, int[] order, double eps) {
                ((ISearchAlg)this).Initialize(nGenes, dim, mins, maxs, fo, va);
                this.order = order;
                this.eps = eps;
            }

            #endregion

            #region ISearchAlgs Implementation

            Crom Search(int maxIteraciones) {
                this.maxIteraciones = maxIteraciones;
                this.iteracion = 1;
                cBest = new Crom("best", nGenes, dim);
                cBest.Reset();
                Crom crom = new Crom("", nGenes, dim);
                crom.Reset();
                SearchVar(crom, mins[0], maxs[0], 0);
                return cBest;
            }

            internal void SearchVar(Crom crom, double min, double max, int d) {
                double[] vals = { min, max, (min + max) / 2.0 };
                double[] evals = new double[3];

                while (min <= max) {
                    for (int v = 0; v < 3; v++) {
                        if (iteracion > maxIteraciones) { return; }
                        crom.Genes[0][d] = vals[v];
                        if (d < 3) { SearchVar(crom, mins[d + 1], maxs[d + 1], d + 1); }
                        crom.Eval = fo.Evaluate(crom.Genes, null);
                        iteracion++;
                        if (crom.Eval < cBest.Eval) { SaveSolution(crom); }
                        evals[v] = crom.Eval;
                    }
                    if (evals[0] < evals[1]) { max = vals[2]; }
                    else if (evals[0] > evals[1]) { min = vals[2]; }
                    else { return; }
                    if (max - min < eps) { return; }

                    SearchVar(crom, min, max, d);
                }
            }

            bool GetResult() {
                return true;
            }

            #endregion

            #region Private Methods

            private void SaveSolution(Crom crom) {
                for (int g = 0; g < nGenes; g++) {
                    for (int d = 0; d < dim; d++) { cBest[g, d] = crom[g, d]; }
                }
                cBest.Eval = crom.Eval;
            }

            #endregion

        }
}
