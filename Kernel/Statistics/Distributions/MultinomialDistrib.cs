#region Imports

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Maths;

#endregion

namespace Statistics
{

    internal class MultinomialDistrib
    {

        #region Fields

        private List<double> P;
        private Combinatory comb;

        #endregion

        #region Constructor

        internal MultinomialDistrib()
        {
            P = new List<double>();
            comb = new Combinatory();
        }

        #endregion

        #region internal Methods

        internal void SetP(List<double> P)
        {
            double sum = 0;
            foreach (double p in P) { sum += p; }
            if (sum != 1) { throw new Exception("Error. Probabilities should sum to 1"); }
            this.P = P;
        }

        internal double Probability(IList<int> X)
        {
            int n = 0;
            foreach (int x in X) { n += x; }
            double factNum = comb.Factorial(n);
            double prodPExp = 1;
            double prodXFact = 1;
            for (int i = 0; i < P.Count; i++)
            {
                prodPExp *= Math.Pow(P[i], X[i]);
                prodXFact *= comb.Factorial(X[i]);
            }
            return (factNum / prodXFact) * prodPExp;
        }

        #endregion

    }
}
