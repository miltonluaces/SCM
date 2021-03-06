#region Imports

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace MachineLearning {

    internal class Crom : IComparable {

         #region Fields

            private string codigo;
            private double[][] genes;
            private double eval;

            #endregion

         #region Constructors

            internal Crom(string codigo, int nGenes, int dim) {
                this.codigo = codigo;
                this.genes = new double[nGenes][];
                for (int i = 0; i < nGenes; i++) { this.genes[i] = new double[dim]; }
            }

            internal Crom(string codigo, double[][] genes) {
                this.codigo = codigo;
                this.genes = genes;
            }

            /// <summary> Constructor </summary>
            /// <param name="genes"> matrix of genes </param>
            internal Crom(double[][] genes) {
                this.codigo = "";
                this.genes = genes;
            }

            #endregion

         #region Properties

            /// <summary> chromosome code </summary>
            internal string Codigo {
                get { return codigo; }
            }

            /// <summary> Matrix of genes </summary>
            internal double[][] Genes {
                get { return genes; }
            }

            /// <summary> Indexer by number of gen and dimension </summary>
            internal double this[int nGen, int nDim] {
                get { return genes[nGen][nDim]; }
                set { genes[nGen][nDim] = value; }
            }

            /// <summary> number of genes </summary>
            internal int nGenes {
                get { return genes.Length; }
            }

            /// <summary> number of dimensions </summary>
            internal int Dim {
                get { return genes[0].Length; }
            }

            /// <summary> Result of evaluation </summary>
            internal double Eval {
                get { return eval; }
                set { eval = value; }
            }

            #endregion

            #region Internal Methods

            /// <summary> Reset value of all genes, to re-evaluate  </summary>
            internal void Reset() {
                for (int i = 0; i < nGenes; i++) {
                    for (int j = 0; j < this.Genes[i].Length; j++) {
                        this.Genes[i][j] = double.MinValue;
                    }
                }
                this.Eval = double.MaxValue;
            }

            #endregion

         #region IComparable Implementation

            int System.IComparable.CompareTo(object obj) {
                return (this.Eval >= ((Crom)obj).Eval) ? 1 : -1;
            }

            #endregion
    
         #region ToString Override

            public override string ToString() {
                string str = "";
                for (int g = 0; g < genes.Length; g++) {
                    for (int d = 0; d < genes[0].Length; d++) {
                        str += genes[g][d] + "\t";
                    }
                    str += "\n";
                }
                return base.ToString();
            }

            #endregion

    }
 
}
