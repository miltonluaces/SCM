#region Imports

using System;
using System.IO;
using System.Threading;
using System.Globalization;

#endregion

namespace MachineLearning  {
    
    internal class RangeTransform : IRangeTransform  {

        #region Fields

        //Bounds for scaling
        internal const int DEFAULT_LOWER_BOUND = -1;
        internal const int DEFAULT_UPPER_BOUND = 1;

        private double[] inputStart;
        private double[] inputScale;
        private double outputStart;
        private double outputScale;
        private int length;
        
        #endregion

        #region Constructors

        // Constructor. minValues, maxValues for each dimesion, lower and upper bound for all dimensions
        internal RangeTransform(double[] minValues, double[] maxValues, double lowerBound, double upperBound)  {
            length = minValues.Length;
            if(maxValues.Length != length)
                throw new Exception("Number of max and min values must be equal.");
            inputStart = new double[length];
            inputScale = new double[length];
            for (int i = 0; i < length; i++)
            {
                inputStart[i] = minValues[i];
                inputScale[i] = maxValues[i] - minValues[i];
            }
            outputStart = lowerBound;
            outputScale = upperBound - lowerBound;
        }

        internal RangeTransform(double[] inputStart, double[] inputScale, double outputStart, double outputScale, int length)   {
            this.inputStart = inputStart;
            this.inputScale = inputScale;
            this.outputStart = outputStart;
            this.outputScale = outputScale;
            this.length = length;
        }

        #endregion

        #region Internal Methods

        // Determines the Range transform for the provided problem.  Uses the default lower and upper bounds.
        internal static RangeTransform Compute(Problem prob)  {
            return Compute(prob, DEFAULT_LOWER_BOUND, DEFAULT_UPPER_BOUND);
        }

        // Determines the Range transform for the provided problem.
        // <param name="lowerBound">The lower bound for scaling</param>
        // <param name="upperBound">The upper bound for scaling</param>
        // <returns>The Range transform for the problem</returns>
        internal static RangeTransform Compute(Problem prob, double lowerBound, double upperBound)  {
            double[] minVals = new double[prob.MaxIndex];
            double[] maxVals = new double[prob.MaxIndex];
            for (int i = 0; i < prob.MaxIndex; i++)
            {
                minVals[i] = double.MaxValue;
                maxVals[i] = double.MinValue;
            }
            for (int i = 0; i < prob.Count; i++)
            {
                for (int j = 0; j < prob.X[i].Length; j++)
                {
                    int index = prob.X[i][j].Index - 1;
                    double value = prob.X[i][j].Value;
                    minVals[index] = Math.Min(minVals[index], value);
                    maxVals[index] = Math.Max(maxVals[index], value);
                }
            }
            for (int i = 0; i < prob.MaxIndex; i++)
            {
                if (minVals[i] == double.MaxValue || maxVals[i] == double.MinValue)
                {
                    minVals[i] = 0;
                    maxVals[i] = 0;
                }
            }
            return new RangeTransform(minVals, maxVals, lowerBound, upperBound);
        }
        
        // Transforms the input array based upon the values provided. input array to scaled array
        public Node[] Transform(Node[] input)
        {
            Node[] output = new Node[input.Length];
            for (int i = 0; i < output.Length; i++)
            {
                int index = input[i].Index;
                double value = input[i].Value;
                output[i] = new Node(index, Transform(value, index));
            }
            return output;
        }

        // Transforms this an input value using the scaling transform for the provided dimension.
        public double Transform(double input, int index)  {
            index--;
            double tmp = input - inputStart[index];
            if (inputScale[index] == 0)
                return 0;
            tmp /= inputScale[index];
            tmp *= outputScale;
            return tmp + outputStart;
        }
        
        // Writes this Range transform to a stream.
        internal static void Write(Stream stream, RangeTransform r)  {
            TemporaryCulture.Start();

            StreamWriter output = new StreamWriter(stream);
            output.WriteLine(r.length);
            output.Write(r.inputStart[0]);
            for(int i=1; i<r.inputStart.Length; i++)
                output.Write(" " + r.inputStart[i]);
            output.WriteLine();
            output.Write(r.inputScale[0]);
            for (int i = 1; i < r.inputScale.Length; i++)
                output.Write(" " + r.inputScale[i]);
            output.WriteLine();
            output.WriteLine("{0} {1}", r.outputStart, r.outputScale);
            output.Flush();

            TemporaryCulture.Stop();
        }

        // Writes this Range transform to a file.    This will overwrite any previous data in the file.
        internal static void Write(string outputFile, RangeTransform r)   {
            FileStream s = File.Open(outputFile, FileMode.Create);
            try  { Write(s, r); }
            finally  {  s.Close();  }
        }

        // Reads a Range transform from a file.
        internal static RangeTransform Read(string inputFile)  {
            FileStream s = File.OpenRead(inputFile);
            try { return Read(s); }
            finally { s.Close(); }
        }

        // Reads a Range transform from a stream.
        internal static RangeTransform Read(Stream stream)  {
            TemporaryCulture.Start();

            StreamReader input = new StreamReader(stream);
            int length = int.Parse(input.ReadLine());
            double[] inputStart = new double[length];
            double[] inputScale = new double[length];
            string[] parts = input.ReadLine().Split();
            for (int i = 0; i < length; i++)
                inputStart[i] = double.Parse(parts[i]);
            parts = input.ReadLine().Split();
            for (int i = 0; i < length; i++)
                inputScale[i] = double.Parse(parts[i]);
            parts = input.ReadLine().Split();
            double outputStart = double.Parse(parts[0]);
            double outputScale = double.Parse(parts[1]);

            TemporaryCulture.Stop();

            return new RangeTransform(inputStart, inputScale, outputStart, outputScale, length);
        }

        #endregion
    }
}
