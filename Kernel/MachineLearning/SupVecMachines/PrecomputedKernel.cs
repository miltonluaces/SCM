#region Imports

using System;
using System.Collections.Generic;

#endregion

namespace MachineLearning {

    // Class encapsulating a precomputed kernel, where each position indicates the similarity score for two items in the training data.
    [Serializable]
    internal class PrecomputedKernel  {

        #region Fields

        private float[,] _similarities;
        private int _rows;
        private int _columns;

        #endregion

        #region Constructors

        //"similarities" : The similarity scores between all items in the training data</param>
        internal PrecomputedKernel(float[,] similarities) {
            _similarities = similarities;
            _rows = _similarities.GetLength(0);
            _columns = _similarities.GetLength(1);
        }

        // Constructor. "nodes" : Nodes for self-similarity analysis ; 
        internal PrecomputedKernel(List<Node[]> nodes, Parameter param) {
            _rows = nodes.Count;
            _columns = _rows;
            _similarities = new float[_rows, _columns];
            for (int r = 0; r < _rows; r++)
            {
                for (int c = 0; c < r; c++)
                    _similarities[r, c] = _similarities[c, r];
                _similarities[r, r] = 1;
                for (int c = r + 1; c < _columns; c++)
                    _similarities[r, c] = (float)Kernel.KernelFunction(nodes[r], nodes[c], param);
            }
        }

        //"rows"  : Nodes to use as the rows of the matrix, idem colums
        internal PrecomputedKernel(List<Node[]> rows, List<Node[]> columns, Parameter param)  {
            _rows = rows.Count;
            _columns = columns.Count;
            _similarities = new float[_rows, _columns];
            for (int r = 0; r < _rows; r++)
                for (int c = 0; c < _columns; c++)
                    _similarities[r, c] = (float)Kernel.KernelFunction(rows[r], columns[c], param);
        }

        #endregion

        #region Internal Methods

        // Constructs a object using the labels provided.  If a label is set to "0" that item is ignored."rowLabels" : The labels for the row items ; "columnLabels" : The labels for the column items
        internal Problem Compute(double[] rowLabels, double[] columnLabels)  {
            List<Node[]> X = new List<Node[]>();
            List<double> Y = new List<double>();
            int maxIndex = 0;
            for (int i = 0; i < columnLabels.Length; i++)
                if (columnLabels[i] != 0)
                    maxIndex++;
            maxIndex++;
            for (int r = 0; r < _rows; r++)
            {
                if (rowLabels[r] == 0)
                    continue;
                List<Node> nodes = new List<Node>();
                nodes.Add(new Node(0, X.Count + 1));
                for (int c = 0; c < _columns; c++)
                {
                    if (columnLabels[c] == 0)
                        continue;
                    double value = _similarities[r, c];
                    nodes.Add(new Node(nodes.Count, value));
                }
                X.Add(nodes.ToArray());
                Y.Add(rowLabels[r]);
            }
            return new Problem(X.Count, Y.ToArray(), X.ToArray(), maxIndex);
        }

        #endregion
    }
}
