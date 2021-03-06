#region Imports

using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using System.Threading;

#endregion

namespace MachineLearning {
    
    //A transform which learns the mean and variance of a sample set and uses these to transform new data so that it has zero mean and unit variance.
    public class GaussianTransform : IRangeTransform  {

        #region Fields

        private double[] means;
        private double[] stDevs;

        #endregion

        #region Constructor

        // Means in each dimension , Standard deviation in each dimension
        public GaussianTransform(double[] means, double[] stddevs)  {
            this.means = means;
            this.stDevs = stddevs;
        }

        #endregion
        
        #region Internal Methods

        // Determines the Gaussian transform for the provided problem.prob
        internal static GaussianTransform Compute(Problem prob) {
            int[] counts = new int[prob.MaxIndex];
            double[] means = new double[prob.MaxIndex];
            foreach (Node[] sample in prob.X)  {
                for (int i = 0; i < sample.Length; i++)  {
                    means[sample[i].Index-1] += sample[i].Value;
                    counts[sample[i].Index-1]++;
                }
            }
            for (int i = 0; i < prob.MaxIndex; i++) {
                if (counts[i] == 0)
                    counts[i] = 2;
                means[i] /= counts[i];
            }

            double[] stddevs = new double[prob.MaxIndex];
            foreach (Node[] sample in prob.X) {
                for (int i = 0; i < sample.Length; i++) {
                    double diff = sample[i].Value - means[sample[i].Index - 1];
                    stddevs[sample[i].Index - 1] += diff * diff;
                }
            }
            for (int i = 0; i < prob.MaxIndex; i++)  {
                if (stddevs[i] == 0)
                    continue;
                stddevs[i] /= (counts[i] - 1);
                stddevs[i] = Math.Sqrt(stddevs[i]);
            }

            return new GaussianTransform(means, stddevs);
        }

        #endregion

        #region IRangeTransform Members

        // Transform the input value using the transform stored for the provided index.
        public double Transform(double input, int index)    {
            index--;
            if (stDevs[index] == 0)
                return 0;
            double diff = input - means[index];
            diff /= stDevs[index];
            return diff;
        }

        // Transforms the input array.
        public Node[] Transform(Node[] input)   {
            Node[] output = new Node[input.Length];
            for (int i = 0; i < output.Length; i++)  {
                int index = input[i].Index;
                double value = input[i].Value;
                output[i] = new Node(index, Transform(value, index));
            }
            return output;
        }

        #endregion

        #region IO Static Methods

        //Saves the transform to the disk.  The samples are not stored, only the statistics.
        public static void Write(Stream stream, GaussianTransform transform)  {
            TemporaryCulture.Start();

            StreamWriter output = new StreamWriter(stream);
            output.WriteLine(transform.means.Length);
            for (int i = 0; i < transform.means.Length; i++)
                output.WriteLine("{0} {1}", transform.means[i], transform.stDevs[i]);
            output.Flush();

            TemporaryCulture.Stop();
        }

        // Reads a GaussianTransform from the provided stream.
        public static GaussianTransform Read(Stream stream)  {
            TemporaryCulture.Start();

            StreamReader input = new StreamReader(stream);
            int length = int.Parse(input.ReadLine(), CultureInfo.InvariantCulture);
            double[] means = new double[length];
            double[] stddevs = new double[length];
            for (int i = 0; i < length; i++)
            {
                string[] parts = input.ReadLine().Split();
                means[i] = double.Parse(parts[0], CultureInfo.InvariantCulture);
                stddevs[i] = double.Parse(parts[1], CultureInfo.InvariantCulture);
            }

            TemporaryCulture.Stop();

            return new GaussianTransform(means, stddevs);
        }

        // Saves the transform to the disk.  The samples are not stored, only the statistics.
        public static void Write(string filename, GaussianTransform transform)  {
            FileStream output = File.Open(filename, FileMode.Create);
            try { Write(output, transform); }
            finally { output.Close(); }
        }

        //Reads a GaussianTransform from the provided stream.
        public static GaussianTransform Read(string filename)  {
            FileStream input = File.Open(filename, FileMode.Open);
            try { return Read(input); }
            finally { input.Close(); }
        }

        #endregion

    }
}
