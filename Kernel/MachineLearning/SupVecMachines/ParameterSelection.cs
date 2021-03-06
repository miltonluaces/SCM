#region Imports

using System;
using System.Collections.Generic;
using System.IO;

#endregion

namespace MachineLearning  {
    
    //Parameter selection for a model which uses C-SVC and an RBF kernel.
    internal static class ParameterSelection  {

        #region Fields

        // Default number of times to divide the data.
        internal const int NFOLD = 5;
        // Default minimum power of 2 for the C value (-5)
        internal const int MIN_C = -5;
        // Default maximum power of 2 for the C value (15)
        internal const int MAX_C = 15;
        // Default power iteration step for the C value (2)
        internal const int C_STEP = 2;
        // Default minimum power of 2 for the Gamma value (-15)
        internal const int MIN_G = -15;
        // Default maximum power of 2 for the Gamma Value (3)
        internal const int MAX_G = 3;
        // Default power iteration step for the Gamma value (2)
        internal const int G_STEP = 2;

        #endregion

        #region Static Methods

        // Returns a logarithmic list of values from minimum power of 2 to the maximum power of 2 using the provided iteration size. MinPower The minimum power of 2, maxPower the maximum, iteration size in powers
        internal static List<double> GetList(double minPower, double maxPower, double iteration) {
            List<double> list = new List<double>();
            for (double d = minPower; d <= maxPower; d += iteration) { list.Add(Math.Pow(2, d)); }
            return list;
        }

        // Performs a Grid parameter selection, trying all possible combinations of the two lists and returning the combination which performed best.  The default ranges of C and Gamma values are used.  
        // Use this method if there is no validation data available, and it will divide it 5 times to allow 5-fold validation (training on 4/5 and validating on 1/5, 5 times).
        //"problem" The training data, parameters : The parameters to use when optimizing, "outputFile" : file for the parameter results."C" "Gamma": The optimal C Gamma value will be put into this variable
        internal static void Grid(Problem problem, Parameter parameters, string outputFile, out double C, out double Gamma) {
            Grid(problem, parameters, GetList(MIN_C, MAX_C, C_STEP), GetList(MIN_G, MAX_G, G_STEP), outputFile, NFOLD, out C, out Gamma);
        }
        
        // Performs a Grid parameter selection, trying all possible combinations of the two lists and returning the combination which performed best.  
        // Use this method if there is no validation data available, and it will divide it 5 times to allow 5-fold validation (training on 4/5 and validating on 1/5, 5 times).
        //"problem": The training data; "parameters" : The parameters to use when optimizing ; "CValues": The set of C values to use ; "GammaValues" : The set of Gamma values to use ; "outputFile" : Output file for the parameter results.; "C" : The optimal C value will be put into this variable ; "Gamma" : The optimal Gamma value will be put into this variable
        internal static void Grid(Problem problem, Parameter parameters, List<double> CValues, List<double> GammaValues, string outputFile, out double C, out double Gamma) {
            Grid(problem, parameters, CValues, GammaValues, outputFile, NFOLD, out C, out Gamma);
        }
        
        // Performs a Grid parameter selection, trying all possible combinations of the two lists and returning the combination which performed best.  Use this method if validation data isn't available, as it will divide the training data and train on a portion of it and test on the rest.
        //"problem": The training data; "parameters" : The parameters to use when optimizing ; "CValues": The set of C values to use ; "GammaValues" : The set of Gamma values to use ; "outputFile" : Output file for the parameter results.; "C" : The optimal C value will be put into this variable ; "Gamma" : The optimal Gamma value will be put into this variable ; "nrfold" : The number of times the data should be divided for validation ; 
        internal static void Grid(Problem problem, Parameter parameters, List<double> CValues, List<double> GammaValues, string outputFile, int nrfold, out double C, out double Gamma)  {
            C = 0;
            Gamma = 0;
            double crossValidation = double.MinValue;
            StreamWriter output = null;
            if (outputFile != null) { output = new StreamWriter(outputFile); }
            for (int i = 0; i < CValues.Count; i++)  {
                for (int j = 0; j < GammaValues.Count; j++) {
                    parameters.C = CValues[i];
                    parameters.Gamma = GammaValues[j];
                    double test = Training.PerformCrossValidation(problem, parameters, nrfold);
                    Console.Write("{0} {1} {2}", parameters.C, parameters.Gamma, test);
                    if (output != null) { output.WriteLine("{0} {1} {2}", parameters.C, parameters.Gamma, test); }
                    if (test > crossValidation) {
                        C = parameters.C;
                        Gamma = parameters.Gamma;
                        crossValidation = test;
                        Console.WriteLine(" New Maximum!");
                    }
                    else Console.WriteLine();
                }
            }
            if(output != null) output.Close();
        }
        
        // Performs a Grid parameter selection, trying all possible combinations of the two lists and returning the combination which performed best.  Uses the default values of C and Gamma.
        //"problem" : The training data ; "validation" : The validation data
        internal static void Grid(Problem problem, Problem validation, Parameter parameters, string outputFile, out double C, out double Gamma)  {
            Grid(problem, validation, parameters, GetList(MIN_C, MAX_C, C_STEP), GetList(MIN_G, MAX_G, G_STEP), outputFile, out C, out Gamma);
        }
        
        // Performs a Grid parameter selection, trying all possible combinations of the two lists and returning the combination which performed best.
        internal static void Grid(Problem problem, Problem validation, Parameter parameters, List<double> CValues, List<double> GammaValues, string outputFile, out double C, out double Gamma)  {
            C = 0;
            Gamma = 0;
            double maxScore = double.MinValue;
            StreamWriter output = null;
            if(outputFile != null)  output = new StreamWriter(outputFile);
            for (int i = 0; i < CValues.Count; i++)  {
                for (int j = 0; j < GammaValues.Count; j++)  {
                    parameters.C = CValues[i];
                    parameters.Gamma = GammaValues[j];
                    SVMModel model = Training.Train(problem, parameters);
                    double test = Prediction.Predict(validation, "tmp.txt", model, false);
                    Console.Write("{0} {1} {2}", parameters.C, parameters.Gamma, test);
                    if (output != null)  output.WriteLine("{0} {1} {2}", parameters.C, parameters.Gamma, test);
                    if (test > maxScore) {
                        C = parameters.C;
                        Gamma = parameters.Gamma;
                        maxScore = test;
                        Console.WriteLine(" New Maximum!");
                    }
                    else Console.WriteLine();
                }
            }
            if(output != null) output.Close();
        }

        #endregion
    }
}
