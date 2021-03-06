#region Imports

using System;
using System.Collections.Generic;
using System.Text;
using Maths;

#endregion

namespace MachineLearning {

    internal abstract class SearchAlg : ISearchAlg {

        #region Fields

        protected int nGenes;
        protected int dim;
        protected double[] mins;
        protected double[] maxs;
        protected IMeritFunction fo;
        protected IValidation va;
        protected Crom cBest;
        protected bool debug;
        protected RndGenerator randGen;
        protected RandomType randomType = RandomType.Uniform;
        protected Crom initial;

        #endregion

        #region Implementacion ISearchAlgs

        bool ISearchAlg.GetResult() { return true; }
        double[][] ISearchAlg.Search(int maxIteraciones) { return null; }
        void ISearchAlg.Initialize(int nGenes, int dim, double[] mins, double[] maxs, IMeritFunction fo, IValidation va) {
            this.nGenes = nGenes;
            this.dim = dim;
            this.mins = mins;
            this.maxs = maxs;
            this.fo = fo;
            this.va = va;
            if (randGen == null) { randGen = new RndGenerator(); }
            randGen.Reset();
        }

        void ISearchAlg.SetDebug(bool debug) {
            this.debug = debug;
        }

        internal void SetRandomType(RandomType randomType, Crom initial) {
            this.randomType = randomType;
            this.initial = initial;
        }

        internal enum RandomType { Uniform, NormalFromBest }

        #endregion
    }
}
