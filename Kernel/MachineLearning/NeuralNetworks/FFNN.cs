#region Imports

using System;
using System.Diagnostics;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using Maths;

#endregion

namespace MachineLearning {
    
internal class FFNN {

        #region Fields

        private int nLayers;
        private int id;
        private double d;
        private int nHidden;
        private int nInput;
        private int nOutput;
        private double n;
        private double m;
        private Neuron[,] matrix;
        private Random rand;
        private Functions func;

        #endregion

        #region Constructor

        internal FFNN(int id, int nInput, int nHidden, int nOutput, double n, double m) {
            this.rand = new Random();
            this.func = new Functions();
            this.id = id;
            this.nLayers = 3;
            this.nInput = nInput;
            this.nHidden = nHidden;
            this.nOutput = nOutput;
            this.n = n;
            this.m = m;
            matrix = new Neuron[3, func.Max(nInput, nHidden, nOutput)];

            double w;
            Neuron ne;
            for (int i = 0; i < nInput; i++) {
                ne = new Neuron(id, 0, i, 1);
                matrix[0, i] = ne;
                ne.N = n;
                ne.M = m;
                if (i == 0) { ne.SetX(1, 0); }
            }

            for (int i = 0; i < nHidden; i++) {
                ne = new Neuron(id, 1, i, nInput + 1);
                matrix[1, i] = ne;
                ne.N = n;
                ne.M = m;
                for (int j = 0; j < ne.NConex; j++) {
                    w = (rand.NextDouble() < 0.5) ? -1 : 1 * rand.NextDouble();
                    ne.SetW(w, j);
                }
            }

            for (int i = 0; i < nOutput; i++) {
                ne = new Neuron(id, 2, i, nHidden + 1);
                matrix[2, i] = ne;
                ne.N = n;
                ne.M = m;
                for (int j = 0; j < ne.NConex; j++) {
                    w = (rand.NextDouble() < 0.5) ? -1 : 1 * rand.NextDouble();
                    ne.SetW(w, j);
                }
            }

            //interconex
            for (int i = 0; i < nInput; i++) {
                for (int j = 0; j < nHidden; j++) {
                    matrix[0, i].AddConex(matrix[1, j]);
                }
            }
            for (int i = 0; i < nHidden; i++) {
                for (int j = 0; j < nOutput; j++) {
                    matrix[1, i].AddConex(matrix[2, j]);
                }
            }
        }

        #endregion

        #region Properties

        internal int Id {
            get { return id; }
            set { id = value; }
        }

        internal int NInput {
            get { return nInput; }
            set { nInput = value; }
        }

        internal int NHidden {
            get { return nHidden; }
            set { nHidden  = value; }
        }

        internal int NOutput {
            get { return nOutput; }
            set { nOutput = value; }
        }

        internal double N {
            set {
                this.n = value;
                for (int i = 0; i < nInput; i++) {
                    matrix[0, i].N = this.n;
                }
                for (int i = 1; i < nHidden; i++) {
                    matrix[0, i].N = this.n;
                }
            }
        }

        internal double M {
            set {
                this.m = value;
                for (int l = 0; l < 3; l++) {
                    for (int i = 0; i < nInput; i++) {
                        matrix[l, i].M = this.m;
                    }
                }
            }
        }

        internal Neuron GetNeuron(int layer, int index) {
            return matrix[layer, index];
        }

        internal double D {
            set { d = value; }
        }

        #endregion

        #region Internal Methods

        internal bool Process(IList<double> data) {
            for (int i = 1; i < nInput; i++) {  matrix[0, i].SetX(data[i - 1], 0);  }
            for (int i = 0; i < nInput; i++) {  matrix[0, i].Transfer();  }
            for (int i = 0; i < nHidden; i++) {   matrix[1, i].Transfer();  }
            return true;
        }

        internal void Learn(double value) {
            for (int i = 0; i < nHidden; i++) {
                matrix[1, i].Learn(value);
            }
            for (int i = 0; i < nOutput; i++) {
                matrix[2, i].Learn(value);
            }
        }

        internal void Learn(double[] data) {
            for (int i = 0; i < nHidden; i++) {
                matrix[1, i].Learn(data);
            }
            for (int i = 0; i < nOutput; i++) {
                matrix[2, i].Learn(data);
            }
        }

        internal double Train(IList<double> data) {
            try {
                List<double> input = new List<double>(data);
                double desOutput = data[data.Count - 1];
                Process(input);
                double error = desOutput - GetOutput()[0];
                double squareError = error * error;
                Learn(desOutput);
                return squareError;
            } 
	        catch { return 0.00; }
        }

        internal double Train(IList<double> input, double desOutput) {
            try {
                Process(input);
                double error = desOutput - GetOutput()[0];
                double squareError = error * error;
                Learn(desOutput);
                return squareError;
            } 
	        catch { return 0.00; }
        }

        internal double Train(IList<double> input, double[] desOutput) {
            try {
                Process(input);
                double sqError = 0;
                for (int i = 0; i < input.Count; i++) {
                    sqError += Math.Pow(desOutput[i] - GetOutput()[i], 2);
                }
                Learn(desOutput);
                return sqError;
            } 
            catch {
                return 0.00;
            }
        }

        internal double[] GetOutput() {
            double[] output = new double[nOutput];
            for (int i = 0; i < nOutput; i++) {
                output[i] = GetNeuron(nLayers - 1, i).Output();
            }
            return output;
        }

        #endregion

        #region Persistence

        internal void SetWeights(List<List<List<double>>> weights)  {
            for (int i = 0; i < nHidden; i++) {
                for (int j = 0; j < matrix[0, i].NConex; j++) { matrix[0, i].SetW(weights[0][i][j], j); }
            }
            for (int i = 0; i < nOutput; i++)  {
                for (int j = 0; j < matrix[0, i].NConex; j++) { matrix[0, i].SetW(weights[1][i][j], j); }
            }
        }

        internal List<List<List<double>>> GetWeights()  {
            List<List<List<double>>> weights = new List<List<List<double>>>();
            List<List<double>> hiddenWeights = new List<List<double>>(); weights.Add(hiddenWeights);
            List<double> neuWeights;
            for (int i = 0; i < nHidden; i++)  {
                neuWeights = new List<double>(); hiddenWeights.Add(neuWeights);
                for (int j = 0; j < matrix[0, i].NConex; j++) { neuWeights.Add(matrix[0, i].GetW(j)); }
            }
            List<List<double>> outputWeights = new List<List<double>>(); weights.Add(outputWeights);
            for (int i = 0; i < nOutput; i++)  {
                neuWeights = new List<double>(); outputWeights.Add(neuWeights);
                for (int j = 0; j < matrix[0, i].NConex; j++) { neuWeights.Add(matrix[0, i].GetW(j)); }
            }
            return weights;
        }
      
        #endregion

        #region Debug

        internal void PrintWeights()
        {

            ResetTrace();
            Trace("INPUT LAYER VALUES");
            for (int i = 0; i < nInput; i++) {
                Trace("[" + i + "] = " + matrix[0, i].GetX(0));
            }

            Trace("HIDDEN LAYER VALUES");
            for (int i = 0; i < nHidden; i++) {
                for (int j = 0; j < nInput; j++) {
                    Trace("[" + i + "," + j + "] = " + matrix[1, i].GetX(j));
                }
            }

            Trace("HIDDEN LAYER WEIGHTS");
            for (int i = 0; i < nHidden; i++) {
                for (int j = 0; j < nInput; j++) {
                    Trace("[" + i + "," + j + "] = " + matrix[1, i].GetW(j));
                }
            }

            Trace("OUTPUT LAYER VALUES");
            for (int i = 0; i < nOutput; i++) {
                for (int j = 0; j < nHidden; j++) {
                    Trace("[" + i + "," + j + "] = " + matrix[2, i].GetX(j));
                }
            }

            Trace("OUTPUT LAYER WEIGHTS");
            for (int i = 0; i < nOutput; i++) {
                for (int j = 0; j < nHidden; j++) {
                    Trace("[" + i + "," + j + "] = " + matrix[2, i].GetW(j));
                }
            }
        }

            private void ResetTrace() {
                StreamWriter trace = new StreamWriter("..\\..\\..\\trace.txt", true);
                trace.Close();
            }

            private void Trace(string msg) {
                StreamWriter trace = new StreamWriter("..\\..\\..\\trace.txt", true);
                trace.WriteLine(msg);
                trace.Close();
            }


        #endregion
    }
}
