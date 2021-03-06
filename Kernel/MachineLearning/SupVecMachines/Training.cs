#region Imports

using System;
using System.Collections.Generic;

#endregion

namespace MachineLearning  {

    // SVM models training
    internal static class Training   {

        #region Properties

        // Whether the system will output information to the console during the training process.
        internal static bool IsVerbose  {
            get { return Procedures.IsVerbose; }
            set { Procedures.IsVerbose = value; }
        }

        #endregion

        #region Static Methods

        private static double doCrossValidation(Problem problem, Parameter parameters, int nr_fold)  {
            int i;
            double[] target = new double[problem.Count];
            Procedures.svm_cross_validation(problem, parameters, nr_fold, target);
            int total_correct = 0;
            double total_error = 0;
            double sumv = 0, sumy = 0, sumvv = 0, sumyy = 0, sumvy = 0;
            if (parameters.SvmType == SvmType.EPSILON_SVR || parameters.SvmType == SvmType.NU_SVR)  {
                for (i = 0; i < problem.Count; i++) {
                    double y = problem.Y[i];
                    double v = target[i];
                    total_error += (v - y) * (v - y);
                    sumv += v;
                    sumy += y;
                    sumvv += v * v;
                    sumyy += y * y;
                    sumvy += v * y;
                }
                return(problem.Count * sumvy - sumv * sumy) / (Math.Sqrt(problem.Count * sumvv - sumv * sumv) * Math.Sqrt(problem.Count * sumyy - sumy * sumy));
            }
            else
                for (i = 0; i < problem.Count; i++)
                    if (target[i] == problem.Y[i]) ++total_correct;
            return (double)total_correct / problem.Count;
        }
        
        // Performs cross validation. returns The cross validation score
        internal static double PerformCrossValidation(Problem problem, Parameter parameters, int nrfold)  {
            string error = Procedures.svm_check_parameter(problem, parameters);
            if (error == null)  return doCrossValidation(problem, parameters, nrfold);
            else throw new Exception(error);
        }

        // Trains a model using the provided training data and parameters.returns A trained SVM Model
        internal static SVMModel Train(Problem problem, Parameter parameters) {
            string error = Procedures.svm_check_parameter(problem, parameters);
            if (error == null) return Procedures.svm_train(problem, parameters);
            else throw new Exception(error);
        }

        private static void parseCommandLine(string[] args, out Parameter parameters, out Problem problem, out bool crossValidation, out int nrfold, out string modelFilename)  {
            int i;

            parameters = new Parameter();
            // default values

            crossValidation = false;
            nrfold = 0;

            // parse options
            for (i = 0; i < args.Length; i++)
            {
                if (args[i][0] != '-')
                    break;
                ++i;
                switch (args[i - 1][1])
                {

                    case 's': parameters.SvmType = (SvmType)int.Parse(args[i]);  break;
                    case 't': parameters.KernelType = (KernelType)int.Parse(args[i]);  break;
                    case 'd': parameters.Degree = int.Parse(args[i]); break;
                    case 'g': parameters.Gamma = double.Parse(args[i]); break;
                    case 'r': parameters.Coefficient0 = double.Parse(args[i]); break;
                    case 'n': parameters.Nu = double.Parse(args[i]); break;
                    case 'm': parameters.CacheSize = double.Parse(args[i]); break;
                    case 'c': parameters.C = double.Parse(args[i]); break;
                    case 'e': parameters.EPS = double.Parse(args[i]); break;
                    case 'p': parameters.P = double.Parse(args[i]); break;
                    case 'h': parameters.Shrinking = int.Parse(args[i]) == 1; break;
                    case 'b': parameters.Probability = int.Parse(args[i]) == 1; break;
                    case 'v': crossValidation = true; nrfold = int.Parse(args[i]); if (nrfold < 2) { throw new ArgumentException("n-fold cross validation: n must >= 2"); } break;
                    case 'w': parameters.Weights[int.Parse(args[i - 1].Substring(2))] = double.Parse(args[1]); break;
                    default: throw new ArgumentException("Unknown Parameter");
                }
            }

            // determine filenames
            if (i >= args.Length)  throw new ArgumentException("No input file specified");
            problem = Problem.Read(args[i]);
            if (parameters.Gamma == 0)  parameters.Gamma = 1.0 / problem.MaxIndex;
            if (i < args.Length - 1)  modelFilename = args[i + 1];
            else  {
                int p = args[i].LastIndexOf('/') + 1;
                modelFilename = args[i].Substring(p) + ".model";
            }
        }

        #endregion
    }
}