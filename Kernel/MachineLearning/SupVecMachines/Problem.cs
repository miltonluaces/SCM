#region Imports

using System;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using System.Globalization;

#endregion

namespace MachineLearning  {
    
    //Problem : set of vectors which must be classified.
    [Serializable]
	internal class Problem  {

        #region Fields

        private int count;
        private double[] y;
        private Node[][] x;
        private int maxIndex;

        #endregion

        #region Constructors

        internal Problem()  {
        }

        // Constructor. "count" : Number of vectors ; "y" : The class labels ; "x" : Vector data. ; "maxIndex" : Maximum index for a vector
        internal Problem(int count, double[] y, Node[][] x, int maxIndex)  {
            this.count = count;
            this.y = y;
            this.x = x;
            this.maxIndex = maxIndex;
        }

        #endregion

        #region Properties

        // Number of vectors.
        internal int Count  {
            get { return count;  }
            set { count = value; }
        }
        
        // Class labels.
        internal double[] Y  {
            get { return y; }
            set { y = value; }
        }

        // Vector data.
        internal Node[][] X  {
            get  {  return x;  }
            set  { x = value;  }
        }
        
        // Maximum index for a vector.
        internal int MaxIndex {
            get { return maxIndex;  }
            set { maxIndex = value; }
        }

        #endregion

        #region Internal Methods

        // Reads a problem from a stream.
        internal static Problem Read(Stream stream)  {
            TemporaryCulture.Start();

            StreamReader input = new StreamReader(stream);
            List<double> vy = new List<double>();
            List<Node[]> vx = new List<Node[]>();
            int max_index = 0;

            while (input.Peek() > -1)
            {
                string[] parts = input.ReadLine().Trim().Split();

                vy.Add(double.Parse(parts[0]));
                int m = parts.Length - 1;
                Node[] x = new Node[m];
                for (int j = 0; j < m; j++)
                {
                    x[j] = new Node();
                    string[] nodeParts = parts[j + 1].Split(':');
                    x[j].Index = int.Parse(nodeParts[0]);
                    x[j].Value = double.Parse(nodeParts[1]);
                }
                if (m > 0)
                    max_index = Math.Max(max_index, x[m - 1].Index);
                vx.Add(x);
            }

            TemporaryCulture.Stop();

            return new Problem(vy.Count, vy.ToArray(), vx.ToArray(), max_index);
        }

        // Writes a problem to a stream.
        internal static void Write(Stream stream, Problem problem)   {
            TemporaryCulture.Start();

            StreamWriter output = new StreamWriter(stream);
            for (int i = 0; i < problem.Count; i++)
            {
                output.Write(problem.Y[i]);
                for (int j = 0; j < problem.X[i].Length; j++)
                    output.Write(" {0}:{1}", problem.X[i][j].Index, problem.X[i][j].Value);
                output.WriteLine();
            }
            output.Flush();

            TemporaryCulture.Stop();
        }

        // Reads a Problem from a file.
        internal static Problem Read(string filename)  {
            FileStream input = File.OpenRead(filename);
            try
            {
                return Read(input);
            }
            finally
            {
                input.Close();
            }
        }

        // Writes a problem to a file.   This will overwrite any previous data in the file.
        internal static void Write(string filename, Problem problem)  {
            FileStream output = File.Open(filename, FileMode.Create);
            try
            {
                Write(output, problem);
            }
            finally
            {
                output.Close();
            }
        }

        #endregion
    }
}