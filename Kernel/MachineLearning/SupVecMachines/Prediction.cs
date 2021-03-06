#region Imports

using System;
using System.IO;
using System.Diagnostics;

#endregion

namespace MachineLearning {
    
    /// Class containing the routines to perform class membership prediction using a trained SVM.
    internal static class Prediction  {
        
        #region Static Methods

        // Predicts the class memberships of all the vectors in the problem. "predict_probability" : Whether to output a distribution over the classes
        internal static double Predict(Problem problem, string outputFile, SVMModel model, bool predict_probability)   {
            int correct = 0;
            int total = 0;
            double error = 0;
            double sumv = 0, sumy = 0, sumvv = 0, sumyy = 0, sumvy = 0;
            StreamWriter output = outputFile != null ? new StreamWriter(outputFile) : null;

            SvmType svm_type = Procedures.svm_get_svm_type(model);
            int nr_class = Procedures.svm_get_nr_class(model);
            int[] labels = new int[nr_class];
            double[] prob_estimates = null;

            if (predict_probability)
            {
                if (svm_type == SvmType.EPSILON_SVR || svm_type == SvmType.NU_SVR)
                {
                    Console.WriteLine("Prob. model for test data: target value = predicted value + z,\nz: Laplace distribution e^(-|z|/sigma)/(2sigma),sigma=" + Procedures.svm_get_svr_probability(model));
                }
                else
                {
                    Procedures.svm_get_labels(model, labels);
                    prob_estimates = new double[nr_class];
                    if (output != null)
                    {
                        output.Write("labels");
                        for (int j = 0; j < nr_class; j++)
                        {
                            output.Write(" " + labels[j]);
                        }
                        output.Write("\n");
                    }
                }
            }
            for (int i = 0; i < problem.Count; i++)
            {
                double target = problem.Y[i];
                Node[] x = problem.X[i];

                double v;
                if (predict_probability && (svm_type == SvmType.C_SVC || svm_type == SvmType.NU_SVC))
                {
                    v = Procedures.svm_predict_probability(model, x, prob_estimates);
                    if (output != null)
                    {
                        output.Write(v + " ");
                        for (int j = 0; j < nr_class; j++)
                        {
                            output.Write(prob_estimates[j] + " ");
                        }
                        output.Write("\n");
                    }
                }
                else
                {
                    v = Procedures.svm_predict(model, x);
                    if(output != null)
                        output.Write(v + "\n");
                }

                if (v == target)
                    ++correct;
                error += (v - target) * (v - target);
                sumv += v;
                sumy += target;
                sumvv += v * v;
                sumyy += target * target;
                sumvy += v * target;
                ++total;
            }
            if(output != null)
                output.Close();
            return (double)correct / total;
        }

        // Predict the class for a single input vector. "x" : The vector for which to predict class
        internal static double Predict(SVMModel model, Node[] x)  {
            return Procedures.svm_predict(model, x);
        }

        // Predicts a class distribution for the single input vector.
        // <param name="model">Model to use for prediction</param>
        // <param name="x">The vector for which to predict the class distribution</param>
        // <returns>A probability distribtion over classes</returns>
        internal static double[] PredictProbability(SVMModel model, Node[] x)
        {
            SvmType svm_type = Procedures.svm_get_svm_type(model);
            if (svm_type != SvmType.C_SVC && svm_type != SvmType.NU_SVC)
                throw new Exception("Model type " + svm_type + " unable to predict probabilities.");
            int nr_class = Procedures.svm_get_nr_class(model);
            double[] probEstimates = new double[nr_class];
            Procedures.svm_predict_probability(model, x, probEstimates);
            return probEstimates;
        }

        internal static void exit_with_help()  {
            Debug.Write("usage: svm_predict [options] test_file model_file output_file\n" + "options:\n" + "-b probability_estimates: whether to predict probability estimates, 0 or 1 (default 0); one-class SVM not supported yet\n");
            Environment.Exit(1);
        }

        #endregion
    }
}