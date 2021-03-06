#region Imports

using System;
using System.IO;
using System.Threading;
using System.Globalization;

#endregion

namespace MachineLearning  {
    
    //SVM Model.
    [Serializable]
	internal class SVMModel  {

        #region Fields

        private Parameter parameter;
        private int nClasses;
        private int vectorsCount;
        private Node[][] vectors;
        private double[][] vectorsCoeffs;
        private double[] rho;
        private double[] pairwiseProbabilityA;
        private double[] pairwiseProbabilityB;

        private int[] classLabels;
        private int[] numberOfSVPerClass;

        #endregion

        #region Constructor

        internal SVMModel() {
        }

        #endregion

        #region Properties

        internal Parameter Parameter  {
            get  { return parameter; }
            set  { parameter = value; }
        }

        internal int NumberOfClasses {
            get { return nClasses; }
            set { nClasses = value; }
        }

        internal int SupportVectorCount {
            get  {  return vectorsCount;  }
            set  {  vectorsCount = value; }
        }

        internal Node[][] SupportVectors {
            get {  return vectors; }
            set { vectors = value; }
        }

        internal double[][] SupportVectorCoefficients {
            get { return vectorsCoeffs; }
            set {vectorsCoeffs = value; }
        }

        internal double[] Rho {
            get  { return rho; }
            set  {rho = value; }
        }

        internal double[] PairwiseProbabilityA {
            get { return pairwiseProbabilityA;  }
            set { pairwiseProbabilityA = value;  }
        }

        internal double[] PairwiseProbabilityB  {
            get  { return pairwiseProbabilityB; }
            set  { pairwiseProbabilityB = value; }
        }
		
		// for classification only
        internal int[] ClassLabels {
            get  {  return classLabels;   }
            set  { classLabels = value; }
        }

        internal int[] NumberOfSVPerClass  {
            get  { return numberOfSVPerClass; }
            set  { numberOfSVPerClass = value;  }
        }

        #endregion

        #region Methods

        /// Reads a Model from the provided file.
        internal static SVMModel Read(string filename)  {
            FileStream input = File.OpenRead(filename);
            try  { return Read(input);  }
            finally { input.Close(); }
        }

        /// Reads a Model from the provided stream.
        internal static SVMModel Read(Stream stream)  {
            TemporaryCulture.Start();

            StreamReader input = new StreamReader(stream);

            // read parameters

            SVMModel model = new SVMModel();
            Parameter param = new Parameter();
            model.Parameter = param;
            model.Rho = null;
            model.PairwiseProbabilityA = null;
            model.PairwiseProbabilityB = null;
            model.ClassLabels = null;
            model.NumberOfSVPerClass = null;

            bool headerFinished = false;
            while (!headerFinished)  {
                string line = input.ReadLine();
                string cmd, arg;
                int splitIndex = line.IndexOf(' ');
                if (splitIndex >= 0)
                {
                    cmd = line.Substring(0, splitIndex);
                    arg = line.Substring(splitIndex + 1);
                }
                else
                {
                    cmd = line;
                    arg = "";
                }
                arg = arg.ToLower();

                int i,n;
                switch(cmd){
                    case "svm_type":
                        param.SvmType = (SvmType)Enum.Parse(typeof(SvmType), arg.ToUpper());
                        break;
                        
                    case "kernel_type":
                        param.KernelType = (KernelType)Enum.Parse(typeof(KernelType), arg.ToUpper());
                        break;

                    case "degree":
                        param.Degree = int.Parse(arg);
                        break;

                    case "gamma":
                        param.Gamma = double.Parse(arg);
                        break;

                    case "coef0":
                        param.Coefficient0 = double.Parse(arg);
                        break;

                    case "nr_class":
                        model.NumberOfClasses = int.Parse(arg);
                        break;

                    case "total_sv":
                        model.SupportVectorCount = int.Parse(arg);
                        break;

                    case "rho":
                        n = model.NumberOfClasses * (model.NumberOfClasses - 1) / 2;
                        model.Rho = new double[n];
                        string[] rhoParts = arg.Split();
                        for(i=0; i<n; i++)
                            model.Rho[i] = double.Parse(rhoParts[i]);
                        break;

                    case "label":
                        n = model.NumberOfClasses;
                        model.ClassLabels = new int[n];
                        string[] labelParts = arg.Split();
                        for (i = 0; i < n; i++)
                            model.ClassLabels[i] = int.Parse(labelParts[i]);
                        break;

                    case "probA":
                        n = model.NumberOfClasses * (model.NumberOfClasses - 1) / 2;
                        model.PairwiseProbabilityA = new double[n];
                            string[] probAParts = arg.Split();
                        for (i = 0; i < n; i++)
                            model.PairwiseProbabilityA[i] = double.Parse(probAParts[i]);
                        break;

                    case "probB":
                        n = model.NumberOfClasses * (model.NumberOfClasses - 1) / 2;
                        model.PairwiseProbabilityB = new double[n];
                        string[] probBParts = arg.Split();
                        for (i = 0; i < n; i++)
                            model.PairwiseProbabilityB[i] = double.Parse(probBParts[i]);
                        break;

                    case "nr_sv":
                        n = model.NumberOfClasses;
                        model.NumberOfSVPerClass = new int[n];
                        string[] nrsvParts = arg.Split();
                        for (i = 0; i < n; i++)
                            model.NumberOfSVPerClass[i] = int.Parse(nrsvParts[i]);
                        break;

                    case "SV":
                        headerFinished = true;
                        break;

                    default:
                        throw new Exception("Unknown text in model file");  
                }
            }

            // read sv_coef and SV

            int m = model.NumberOfClasses - 1;
            int l = model.SupportVectorCount;
            model.SupportVectorCoefficients = new double[m][];
            for (int i = 0; i < m; i++)
            {
                model.SupportVectorCoefficients[i] = new double[l];
            }
            model.SupportVectors = new Node[l][];

            for (int i = 0; i < l; i++)
            {
                string[] parts = input.ReadLine().Trim().Split();

                for (int k = 0; k < m; k++)
                    model.SupportVectorCoefficients[k][i] = double.Parse(parts[k]);
                int n = parts.Length-m;
                model.SupportVectors[i] = new Node[n];
                for (int j = 0; j < n; j++)
                {
                    string[] nodeParts = parts[m + j].Split(':');
                    model.SupportVectors[i][j] = new Node();
                    model.SupportVectors[i][j].Index = int.Parse(nodeParts[0]);
                    model.SupportVectors[i][j].Value = double.Parse(nodeParts[1]);
                }
            }

            TemporaryCulture.Stop();

            return model;
        }

        /// Writes a model to the provided filename.  This will overwrite any previous data in the file.
        internal static void Write(string filename, SVMModel model)  {
            FileStream stream = File.Open(filename, FileMode.Create);
            try  { Write(stream, model); }
            finally { stream.Close(); }
        }

        /// Writes a model to the provided stream.
        internal static void Write(Stream stream, SVMModel model) {
            TemporaryCulture.Start();

            StreamWriter output = new StreamWriter(stream);

            Parameter param = model.Parameter;

            output.Write("svm_type " + param.SvmType + "\n");
            output.Write("kernel_type " + param.KernelType + "\n");

            if (param.KernelType == KernelType.POLY)
                output.Write("degree " + param.Degree + "\n");

            if (param.KernelType == KernelType.POLY || param.KernelType == KernelType.RBF || param.KernelType == KernelType.SIGMOID)
                output.Write("gamma " + param.Gamma + "\n");

            if (param.KernelType == KernelType.POLY || param.KernelType == KernelType.SIGMOID)
                output.Write("coef0 " + param.Coefficient0 + "\n");

            int nr_class = model.NumberOfClasses;
            int l = model.SupportVectorCount;
            output.Write("nr_class " + nr_class + "\n");
            output.Write("total_sv " + l + "\n");

            {
                output.Write("rho");
                for (int i = 0; i < nr_class * (nr_class - 1) / 2; i++)
                    output.Write(" " + model.Rho[i]);
                output.Write("\n");
            }

            if (model.ClassLabels != null)
            {
                output.Write("label");
                for (int i = 0; i < nr_class; i++)
                    output.Write(" " + model.ClassLabels[i]);
                output.Write("\n");
            }

            if (model.PairwiseProbabilityA != null)
            // regression has probA only
            {
                output.Write("probA");
                for (int i = 0; i < nr_class * (nr_class - 1) / 2; i++)
                    output.Write(" " + model.PairwiseProbabilityA[i]);
                output.Write("\n");
            }
            if (model.PairwiseProbabilityB != null)
            {
                output.Write("probB");
                for (int i = 0; i < nr_class * (nr_class - 1) / 2; i++)
                    output.Write(" " + model.PairwiseProbabilityB[i]);
                output.Write("\n");
            }

            if (model.NumberOfSVPerClass != null)
            {
                output.Write("nr_sv");
                for (int i = 0; i < nr_class; i++)
                    output.Write(" " + model.NumberOfSVPerClass[i]);
                output.Write("\n");
            }

            output.Write("SV\n");
            double[][] sv_coef = model.SupportVectorCoefficients;
            Node[][] SV = model.SupportVectors;

            for (int i = 0; i < l; i++)
            {
                for (int j = 0; j < nr_class - 1; j++)
                    output.Write(sv_coef[j][i] + " ");

                Node[] p = SV[i];
                if (p.Length == 0)
                {
                    output.WriteLine();
                    continue;
                }
                if (param.KernelType == KernelType.PRECOMPUTED)
                    output.Write("0:{0}", (int)p[0].Value);
                else
                {
                    output.Write("{0}:{1}", p[0].Index, p[0].Value);
                    for (int j = 1; j < p.Length; j++)
                        output.Write(" {0}:{1}", p[j].Index, p[j].Value);
                }
                output.WriteLine();
            }

            output.Flush();

            TemporaryCulture.Stop();
        }

        #endregion
    }
}