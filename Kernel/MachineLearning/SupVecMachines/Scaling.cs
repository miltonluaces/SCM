#region Imports

using System;

#endregion

namespace MachineLearning {

    //Deals with the scaling of Problems so they have uniform ranges across all dimensions in order to result in better SVM performance.
    internal static class Scaling {
        
        /// Scales a problem using the provided range.  This will not affect the parameter. "range" : The Range transform to use in scaling; returns The Scaled problem
        internal static Problem Scale(this IRangeTransform range, Problem prob)  {
            Problem scaledProblem = new Problem(prob.Count, new double[prob.Count], new Node[prob.Count][], prob.MaxIndex);
            for (int i = 0; i < scaledProblem.Count; i++)  {
                scaledProblem.X[i] = new Node[prob.X[i].Length];
                for (int j = 0; j < scaledProblem.X[i].Length; j++) { scaledProblem.X[i][j] = new Node(prob.X[i][j].Index, range.Transform(prob.X[i][j].Value, prob.X[i][j].Index)); }
                scaledProblem.Y[i] = prob.Y[i];
            }
            return scaledProblem;
        }
    }
}
