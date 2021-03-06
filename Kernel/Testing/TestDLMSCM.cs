#region Imports

using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MachineLearning;
using Maths;
using Statistics;

#endregion

namespace Testing {

    [TestClass]
    public class TestDLMSCM  {

        #region Polynomial Tests

        [TestMethod]
        public void Test01_DLMPoly() {
            double[]  m0 =  { 0, 1 };

            double[,] C0 = {{ 0, 1 }, 
                            { 1, 1 }};

            double    v =     0.1; 
            double[,] W =  {{ 0.3, 0.0 },
                            { 0.0, 0.3 }};

            double[] hist = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

            Test(n, p, m0, C0, F, G, v, W, hist);
        }

        [TestMethod]
        public void Test02_DLMPoly() {
            double[] m0 = { 3, 1 };

            double[,] C0 = {{ 0, 1 }, 
                            { 1, 1 }};

            double v = 0.1;
            double[,] W =  {{ 0.3, 0.0 },
                            { 0.0, 0.3 }};

            double[] hist = { 3, 4, 5, 6, 5, 4, 3, 4, 5, 6 };

            Test(n, p, m0, C0, F, G, v, W, hist);
        }

        [TestMethod]
        public void Test03_DLMPoly() {
            double[] m0 = { 1, 1 };

            double[,] C0 = {{ 0, 1 }, 
                            { 1, 1 }};

            double v = 0.1;
            double[,] W =  {{ 0.3, 0.0 },
                            { 0.0, 0.3 }};

            double[] hist = { 1.1, 1.3, 1.5, 1.3, 1.2, 1.4, 1.6, 2.4, 3.2, 4.5 };

            Test(n, p, m0, C0, F, G, v, W, hist);
        }
     

        #endregion 

        #region Seasonal Tests

        [TestMethod]
        public void Test04_DLMSeason() {
            int p = 5;
            int n = 30;
            double[] F = { 1, 0, 1, 0, 1 };
            double w = (2 * Math.PI) / 6;
            double s = Math.Sin(w);
            double c = Math.Cos(w);
            double[,] G = {{ 1, 1, 0, 0, 0 }, 
                           { 0, 1, 0, 0, 0 },
                           { 0, 0, c, s, 0 },
                           { 0, 0,-s, c, 0 },
                           { 0, 0, 0, 0,-1 }}; 

            double[] m0 = { 0, 0, 1, 1, 0  };

            double[,] C0 = {{ 0, 1, 0, 0, 0 }, 
                            { 0, 0, 0, 1, 0 }, 
                            { 1, 1, 0, 0, 0 },
                            { 0, 1, 0, 0, 0 }, 
                            { 0, 1, 0, 0, 0 }};

            double v = 0.1;
            double[,] W =  {{ 0.2, 0  , 0  , 0  , 0 },
                            { 0  , 0.2, 0  , 0  , 0 },
                            { 0  , 0  , 0.2, 0  , 0 },
                            { 0  , 0  , 0  , 0.2, 0 }, 
                            { 0  , 0  , 0  , 0  , 0 }};

            double[] hist = { 1, 2, 3, 4, 3, 2, 1, 2.2, 3.4, 4, 3, 2, 1.3, 2.1, 3, 4, 3, 2.3, 1, 2, 3, 4.4, 3.1, 2, 1.4, 2, 3.1, 4, 3.3, 2  };

            Test(n, p, m0, C0, F, G, v, W, hist);
        }

        [TestMethod]
        public void Test05_DLMSeason() {
            int p = 5;
            int n = 60;
            double[] F = { 1, 0, 1, 0, 1 };
            double w = (2 * Math.PI) / 2;
            double s = Math.Sin(w);
            double c = Math.Cos(w);
            double[,] G = {{ 1, 1, 0, 0, 0 }, 
                           { 0, 1, 0, 0, 0 },
                           { 0, 0, c, s, 0 },
                           { 0, 0,-s, c, 0 },
                           { 0, 0, 0, 0,-1 }};

            double[] m0 = { 1, 1, 1, 1, 0 };

            double[,] C0 = {{ 0, 1, 0, 0, 0 }, 
                            { 0, 0, 0, 1, 0 }, 
                            { 1, 1, 0, 0, 0 },
                            { 0, 1, 0, 0, 0 }, 
                            { 0, 1, 0, 0, 0 }};

            double v = 0.1;
            double[,] W =  {{ 0.2, 0  , 0  , 0  , 0 },
                            { 0  , 0.2, 0  , 0  , 0 },
                            { 0  , 0  , 0.2, 0  , 0 },
                            { 0  , 0  , 0  , 0.2, 0 }, 
                            { 0  , 0  , 0  , 0  , 0 }};

            double[] hist = { 1, 2, 3, 4, 3, 2, 1, 2.2, 3.4, 4, 3, 2, 1.3, 2.1, 3, 4, 3, 2.3, 1, 2, 3, 4.4, 3.1, 2, 1.4, 2, 3.1, 4, 3.3, 2, 1, 2, 3, 4, 3, 2, 1, 2.2, 3.4, 4, 3, 2, 1.3, 2.1, 3, 4, 3, 2.3, 1, 2, 3, 4.4, 3.1, 2, 1.4, 2, 3.1, 4, 3.3, 2 };

            Test(n, p, m0, C0, F, G, v, W, hist);

        }

        #endregion

        #region ModelDesign Tests

        [TestMethod]
        public void Test06_DLMPoly() {
            double[] m0 = { 1, 1 };

            double[,] C0 = {{ 0, 1 }, 
                            { 1, 1 }};

            double v = 0.1;
            double[,] W =  {{ 0.3, 0.0 },
                            { 0.0, 0.3 }};

            double[] hist = { 1.1, 1.3, 1.5, 1.3, 1.2, 1.4, 1.6, 2.4, 3.2, 4.5 };

            ModelDesign md = new ModelDesign();
            F = md.BuildF(0);
            G = md.BuildG(new List<ModelDesign.Harm>());

            Test(n, p, m0, C0, F, G, v, W, hist);
        }

        [TestMethod]
        public void Test07_DLMSeason() {
            int p = 5;
            int n = 60;
            double[] F = { 1, 0, 1, 0, 1 };

            double[] m0 = { 1, 1, 1, 1, 0 };

            double[,] C0 = {{ 0, 1, 0, 0, 0 }, 
                            { 0, 0, 0, 1, 0 }, 
                            { 1, 1, 0, 0, 0 },
                            { 0, 1, 0, 0, 0 }, 
                            { 0, 1, 0, 0, 0 }};

            double v = 0.1;
            double[,] W =  {{ 0.2, 0  , 0  , 0  , 0 },
                            { 0  , 0.2, 0  , 0  , 0 },
                            { 0  , 0  , 0.2, 0  , 0 },
                            { 0  , 0  , 0  , 0.2, 0 }, 
                            { 0  , 0  , 0  , 0  , 0 }};

            double[] hist = { 1, 2, 3, 4, 3, 2, 1, 2.2, 3.4, 4, 3, 2, 1.3, 2.1, 3, 4, 3, 2.3, 1, 2, 3, 4.4, 3.1, 2, 1.4, 2, 3.1, 4, 3.3, 2, 1, 2, 3, 4, 3, 2, 1, 2.2, 3.4, 4, 3, 2, 1.3, 2.1, 3, 4, 3, 2.3, 1, 2, 3, 4.4, 3.1, 2, 1.4, 2, 3.1, 4, 3.3, 2 };

            ModelDesign md = new ModelDesign();
            List<ModelDesign.Harm> seasons = new List<ModelDesign.Harm>();
            ModelDesign.Harm season = new ModelDesign.Harm(1, 2, false);
            seasons.Add(season);
            F = md.BuildF(seasons.Count);
            G = md.BuildG(seasons);

            
            Test(n, p, m0, C0, F, G, v, W, hist);

        }
        
        #endregion

        #region K Step Fcst Tests

        [TestMethod]
        public void Test08_DLMPoly() {
            double[] m0 = { 1, 1 };

            double[,] C0 = {{ 0, 1 }, 
                            { 1, 1 }};

            double v = 0.1;
            double[,] W =  {{ 0.3, 0.0 },
                            { 0.0, 0.3 }};

            double[] hist = { 1.1, 1.3, 1.5, 1.3, 1.2, 1.4, 1.6, 2.4, 3.2, 4.5 };

            ModelDesign md = new ModelDesign();
            F = md.BuildF(0);
            G = md.BuildG(new List<ModelDesign.Harm>());

            int k = 5;
            TestFcst(k, n, p, m0, C0, F, G, v, W, hist);
                   
        }

        [TestMethod]
        public void Test09_DLMSeason() {
            int p = 5;
            int n = 60;
            double[] F = { 1, 0, 1, 0, 1 };

            double[] m0 = { 1, 1, 1, 1, 0 };

            double[,] C0 = {{ 0, 1, 0, 0, 0 }, 
                            { 0, 0, 0, 1, 0 }, 
                            { 1, 1, 0, 0, 0 },
                            { 0, 1, 0, 0, 0 }, 
                            { 0, 1, 0, 0, 0 }};

            double v = 0.1;
            double[,] W =  {{ 0.2, 0  , 0  , 0  , 0 },
                            { 0  , 0.2, 0  , 0  , 0 },
                            { 0  , 0  , 0.2, 0  , 0 },
                            { 0  , 0  , 0  , 0.2, 0 }, 
                            { 0  , 0  , 0  , 0  , 0 }};

            double[] hist = { 1, 2, 3, 4, 3, 2, 1, 2.2, 3.4, 4, 3, 2, 1.3, 2.1, 3, 4, 3, 2.3, 1, 2, 3, 4.4, 3.1, 2, 1.4, 2, 3.1, 4, 3.3, 2, 1, 2, 3, 4, 3, 2, 1, 2.2, 3.4, 4, 3, 2, 1.3, 2.1, 3, 4, 3, 2.3, 1, 2, 3, 4.4, 3.1, 2, 1.4, 2, 3.1, 4, 3.3, 2 };

            ModelDesign md = new ModelDesign();
            List<ModelDesign.Harm> seasons = new List<ModelDesign.Harm>();
            ModelDesign.Harm season = new ModelDesign.Harm(1, 12, false);
            seasons.Add(season);
            F = md.BuildF(seasons.Count);
            G = md.BuildG(seasons);

            int k = 20;
            TestFcst(k, n, p, m0, C0, F, G, v, W, hist);

        }

        #endregion

        #region K Step Fcst Retro Tests

        [TestMethod]
        public void Test10_DLMPoly() {
            double[] m0 = { 1, 1 };

            double[,] C0 = {{ 0, 1 }, 
                            { 1, 1 }};

            double v = 0.1;
            double[,] W =  {{ 0.3, 0.0 },
                            { 0.0, 0.3 }};

            double[] hist = { 1.1, 1.3, 1.5, 1.3, 1.2, 1.4, 1.6, 2.4, 3.2, 4.5 };

            ModelDesign md = new ModelDesign();
            F = md.BuildF(0);
            G = md.BuildG(new List<ModelDesign.Harm>());

            int k = 5;
            TestRetroFcst(k, n, p, m0, C0, F, G, v, W, hist);

        }

        [TestMethod]
        public void Test11_DLMSeason() {
            int p = 5;
            int n = 60;
            double[] F = { 1, 0, 1, 0, 1 };

            double[] m0 = { 1, 1, 1, 1, 0 };

            double[,] C0 = {{ 0, 1, 0, 0, 0 }, 
                            { 0, 0, 0, 1, 0 }, 
                            { 1, 1, 0, 0, 0 },
                            { 0, 1, 0, 0, 0 }, 
                            { 0, 1, 0, 0, 0 }};

            double v = 0.1;
            double[,] W =  {{ 0.2, 0  , 0  , 0  , 0 },
                            { 0  , 0.2, 0  , 0  , 0 },
                            { 0  , 0  , 0.2, 0  , 0 },
                            { 0  , 0  , 0  , 0.2, 0 }, 
                            { 0  , 0  , 0  , 0  , 0 }};

            double[] hist = { 1, 2, 3, 4, 3, 2, 1, 2.2, 3.4, 4, 3, 2, 1.3, 2.1, 3, 4, 3, 2.3, 1, 2, 3, 4.4, 3.1, 2, 1.4, 2, 3.1, 4, 3.3, 2, 1, 2, 3, 4, 3, 2, 1, 2.2, 3.4, 4, 3, 2, 1.3, 2.1, 3, 4, 3, 2.3, 1, 2, 3, 4.4, 3.1, 2, 1.4, 2, 3.1, 4, 3.3, 2 };

            ModelDesign md = new ModelDesign();
            List<ModelDesign.Harm> seasons = new List<ModelDesign.Harm>();
            ModelDesign.Harm season = new ModelDesign.Harm(1, 12, false);
            seasons.Add(season);
            F = md.BuildF(seasons.Count);
            G = md.BuildG(seasons);

            int k = 20;
            TestRetroFcst(k, n, p, m0, C0, F, G, v, W, hist);

        }

        #endregion

        #region Retro Error & Mse Tests

        [TestMethod]
        public void Test12_DLMPoly() {
            double[] m0 = { 1, 1 };

            double[,] C0 = {{ 0, 1 }, 
                            { 1, 1 }};

            double v = 0.1;
            double[,] W =  {{ 0.3, 0.0 },
                            { 0.0, 0.3 }};

            double[] hist = { 1.1, 1.3, 1.5, 1.3, 1.2, 1.4, 1.6, 2.4, 3.2, 4.5 };

            ModelDesign md = new ModelDesign();
            F = md.BuildF(0);
            G = md.BuildG(new List<ModelDesign.Harm>());

            int k = 5;
            TestRetroError(k, n, p, m0, C0, F, G, v, W, hist);

        }

        [TestMethod]
        public void Test13_DLMSeason() {
            int p = 5;
            int n = 60;
            double[] F = { 1, 0, 1, 0, 1 };

            double[] m0 = { 1, 1, 1, 1, 0 };

            double[,] C0 = {{ 0, 1, 0, 0, 0 }, 
                            { 0, 0, 0, 1, 0 }, 
                            { 1, 1, 0, 0, 0 },
                            { 0, 1, 0, 0, 0 }, 
                            { 0, 1, 0, 0, 0 }};

            double v = 0.1;
            double[,] W =  {{ 0.2, 0  , 0  , 0  , 0 },
                            { 0  , 0.2, 0  , 0  , 0 },
                            { 0  , 0  , 0.2, 0  , 0 },
                            { 0  , 0  , 0  , 0.2, 0 }, 
                            { 0  , 0  , 0  , 0  , 0 }};

            double[] hist = { 1, 2, 3, 4, 3, 2, 1, 2.2, 3.4, 4, 3, 2, 1.3, 2.1, 3, 4, 3, 2.3, 1, 2, 3, 4.4, 3.1, 2, 1.4, 2, 3.1, 4, 3.3, 2, 1, 2, 3, 4, 3, 2, 1, 2.2, 3.4, 4, 3, 2, 1.3, 2.1, 3, 4, 3, 2.3, 1, 2, 3, 4.4, 3.1, 2, 1.4, 2, 3.1, 4, 3.3, 2 };

            ModelDesign md = new ModelDesign();
            List<ModelDesign.Harm> seasons = new List<ModelDesign.Harm>();
            ModelDesign.Harm season = new ModelDesign.Harm(1, 12,false);
            seasons.Add(season);
            F = md.BuildF(seasons.Count);
            G = md.BuildG(seasons);

            int k = 20;
            TestRetroError(k, n, p, m0, C0, F, G, v, W, hist);

        }

        #endregion

        #region StatFcst Tests

        [TestMethod]
        public void Test14_StatFcst() {
            double[] histArr = { 1, 2, 3, 4, 3, 2, 1, 2.2, 3.4, 4, 3, 2, 1.3, 2.1, 3, 4, 3, 2.3, 1, 2, 3, 4.4, 3.1, 2, 1.4, 2, 3.1, 4, 3.3, 2, 1, 2, 3, 4, 3, 2, 1, 2.2, 3.4, 4, 3, 2, 1.3, 2.1, 3, 4, 3, 2.3, 1, 2, 3, 4.4, 3.1, 2, 1.4, 2, 3.1, 4, 3.3, 2 };
            List<double> hist = new List<double>(histArr);
           ITsForecast sf = new StatFcst(6);
            sf.LoadData(hist, 0);
            Console.WriteLine("t\tHist\tMean");
            Console.WriteLine("--\t------\t------\t------\t------");
            for (int t = 0; t < hist.Count; t++) {
                Console.WriteLine((t + 1) + "\t" + hist[t].ToString("0.00") + "\t");
            }
            int k = 5; 
            double[] fcst = sf.GetFcst(k);
            for (int t = 0; t < fcst.Length; t++) {
                Console.WriteLine((t + 1) + "\t" + fcst[t].ToString("0.00") + "\t");
            }
        }

        #endregion

        #region SVM Tests

        [TestMethod]
        public void TestSVM() {

            //First, read in the training data.
            Problem train = Problem.Read(@"..\..\a1a.train");
            Problem test = Problem.Read(@"..\..\a1a.test");


            //For this example (and indeed, many scenarios), the default parameters will suffice.
            Parameter parameters = new Parameter();
            double C;
            double Gamma;


            //This will do a grid optimization to find the best parameters and store them in C and Gamma, outputting the entire search to params.txt.
            ParameterSelection.Grid(train, parameters, "params.txt", out C, out Gamma);
            parameters.C = C;
            parameters.Gamma = Gamma;


            //Train the model using the optimal parameters.
            SVMModel model = Training.Train(train, parameters);


            //Perform classification on the test data, putting the results in results.txt.
            Prediction.Predict(test, "results.txt", model, false);

        }

        [TestMethod]
        public void TestSVMRegression()  {

            //First, read in the training data.
            Problem train = Problem.Read(@"..\..\tsSVM.train");
            Problem test = Problem.Read(@"..\..\a1a.test");


            //For this example (and indeed, many scenarios), the default parameters will suffice.
            Parameter parameters = new Parameter();
            parameters.SvmType = SvmType.EPSILON_SVR;
            double C;
            double Gamma;


            //This will do a grid optimization to find the best parameters and store them in C and Gamma, outputting the entire search to params.txt.
            ParameterSelection.Grid(train, parameters, "params.txt", out C, out Gamma);
            parameters.C = C;
            parameters.Gamma = Gamma;

            parameters.C = 1;
            //Train the model using the optimal parameters.
            SVMModel model = Training.Train(train, parameters);


            //Perform classification on the test data, putting the results in results.txt.
            Prediction.Predict(test, "results.txt", model, false);

        }
        
        #endregion

        #region Auxiliar Methods

        private void Test(int n, int p, double[] m0, double[,] C0, double[] F, double[,] G, double v, double[,] W, double[] hist) {
            DLM dlm = new DLM(n, p);
            dlm.LoadModel(F, G, v, W);
            dlm.Initalize(m0, C0);

            Console.WriteLine("t\tHist\tMean\tCILow\tCIHigh");
            Console.WriteLine("--\t------\t------\t------\t------");
            for (int t = 0; t < hist.Length; t++) {
                dlm.Iteration(hist[t]);
                double mean = dlm.GetFcstMean();
                double sd = Math.Sqrt(dlm.GetFcstVar());
                Console.WriteLine((t + 1) + "\t" + hist[t].ToString("0.00") + "\t" + mean.ToString("0.00") + "\t" + (mean - sd).ToString("0.00") + "\t" + (mean + sd).ToString("0.00"));
            }
        }

        private void TestFcst(int k, int n, int p, double[] m0, double[,] C0, double[] F, double[,] G, double v, double[,] W, double[] hist) {
            DLM dlm = new DLM(n, p);
            dlm.LoadModel(F, G, v, W);
            dlm.Initalize(m0, C0);
            for (int t = 0; t < hist.Length; t++) { dlm.Iteration(hist[t]); }


            Console.WriteLine("t\tHist\tFcst");
            Console.WriteLine("--\t----\t----");
            for (int t = 0; t < hist.Length; t++) {
                Console.WriteLine((t + 1) + "\t" + hist[t].ToString("0.00") + "\t" + hist[t].ToString("0.00"));
            }
        
            double[] fcst = dlm.GetFcstsMean(k);
            for (int t = 0; t < k; t++) {
                Console.WriteLine((t + 1) + "\t" + fcst[t].ToString("0.00"));
            }
        }

        private void TestRetroFcst(int k, int n, int p, double[] m0, double[,] C0, double[] F, double[,] G, double v, double[,] W, double[] hist) {
            DLM dlm = new DLM(n, p);
            dlm.LoadModel(F, G, v, W);
            dlm.Initalize(m0, C0);
            for (int t = 0; t < hist.Length; t++) { dlm.Iteration(hist[t]); }


            Console.WriteLine("t\tHist\tFcst");
            Console.WriteLine("--\t----\t----");
            for (int t = 0; t < hist.Length-k; t++) {
                Console.WriteLine((t + 1) + "\t" + hist[t].ToString("0.00") + "\t" + hist[t].ToString("0.00"));
            }

            double[] fcst = dlm.GetRetroFcstsMean(k);
            for (int t = 0; t < k; t++) {
                Console.WriteLine((t + 1) + "\t" + fcst[t].ToString("0.00") + "\t" + hist[hist.Length-k+t].ToString("0.00"));
            }
        }

        private void TestRetroError(int k, int n, int p, double[] m0, double[,] C0, double[] F, double[,] G, double v, double[,] W, double[] hist) {
            DLM dlm = new DLM(n, p);
            dlm.LoadModel(F, G, v, W);
            dlm.Initalize(m0, C0);
            for (int t = 0; t < hist.Length; t++) { dlm.Iteration(hist[t]); }


            Console.WriteLine("t\tError");
            Console.WriteLine("--\t----");
            double[] error = dlm.GetRetroFcstsError(k);
            for (int t = 0; t < k; t++) {
                Console.WriteLine((t + 1) + "\t" + error[t].ToString("0.00"));
            }
            double mse = dlm.GetCrossValMae(k);
            Console.WriteLine("");
            Console.WriteLine("mae = " + mse.ToString("0.00"));
        }

        #endregion

        #region Test context

        private int p = 2;
        private int n = 10;
        private double[] F = { 1, 0 };
        private double[,] G = { { 1, 1 }, { 0, 1 } }; 

        [ClassInitialize()] //Run code before running the first test in the class
        public static void InitSuite(TestContext testContext) {
        }

        [ClassCleanup()] //Run code aflter running the last test in the class
        public static void EndSuite() {
        }

        [TestInitialize()] //Run code before running each test
        public void InitTest() {
        }

        [TestCleanup()] //Run code after running each test
        public void EndTest() {
        }

        #endregion
    }
}
