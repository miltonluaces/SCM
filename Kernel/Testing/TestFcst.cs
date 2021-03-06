#region Imports

using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MachineLearning;
using Statistics;
using Maths;
using System.Data.OleDb;

#endregion

namespace Testing {

    [TestClass]
    public class TestFcst  {

        #region Parameter fields

        private int k = 12;
        private FcstMethodType method;
       
        private enum MethodType { Naive, Regression, ExpSmooth, DLMKalm, TDNN };
        
        #endregion

        #region Test context

        [ClassInitialize()]
        public static void InitSuite(TestContext testContext) {}

        [ClassCleanup()]
        public static void EndSuite() {}

        [TestInitialize()] 
        public void InitTest() {
            //method = FcstMethodType.Naive;
            //method = FcstMethodType.Regression;
            //method = FcstMethodType.ZChart;
            //method = FcstMethodType.HoltWinters;
            method = FcstMethodType.ARIMA;
            //method = FcstMethodType.DLMKalman;
            //method = FcstMethodType.NeuNet;
        }

        [TestCleanup()] 
        public void EndTest() {}

        #endregion

        #region Fcst Tests

        #region Tests 01-04 Constant, Trend

        [TestMethod]
        public void Test01_B_Constant()    {
            double[] hist = { 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30 };
            double[] fcst = CalcFcst(hist, method);
            WriteOutcome(hist, fcst);                
            WriteExcel("B", hist, fcst);
        }

        [TestMethod]
        public void Test02_C_TrendAsc() {
            double[] hist = { 1, 1.5, 2, 2.5, 3, 3.5, 4, 4.5, 5, 5.5, 6, 6.5, 7, 7.5, 8, 8.5, 9, 10, 10.5, 11, 11.5, 12, 12.5, 13, 13.5, 14, 14.5, 15, 15.5, 16, 16.5, 17, 17.5, 18, 18.5, 19, 20, 20.5, 21, 21.5, 22, 22.5, 23, 23.5, 24, 24.5 };
            double[] fcst = CalcFcst(hist, method);
            WriteOutcome(hist, fcst);
            WriteExcel("C", hist, fcst);
        }

        [TestMethod]
        public void Test03_D_TrendDesc()  {
            double[] hist = { 90, 89, 88, 87, 86, 85, 84, 83, 82, 81, 80, 79, 78, 77, 76, 75, 75, 74, 73, 72, 71, 70, 69, 68, 67, 66, 65, 64, 63, 62, 61, 60, 59, 58, 57, 56, 55, 54, 53, 52, 51, 50, 49, 48, 47, 46 };
            double[] fcst = CalcFcst(hist, method);
            WriteOutcome(hist, fcst);
            WriteExcel("D", hist, fcst);
        }

        [TestMethod]
        public void Test04_E_TrendAscDesc()  {
            double[] hist = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0, 1, 2, 3, 4, 5, 6 };
            double[] fcst = CalcFcst(hist, method);
            WriteOutcome(hist, fcst);
            WriteExcel("E", hist, fcst);
        }
        
        #endregion

        #region Tests 05-08 Perturb, Discont

        [TestMethod]
        public void Test05_F_ConstantPerturb() {
            double[] hist = { 30, 30, 30, 29, 30, 27, 30, 30, 33, 30, 32, 30, 32, 30, 28, 30, 27, 30, 31, 33, 30, 31, 30, 29, 30, 27, 30, 31, 30, 29, 29, 30, 30, 31, 30, 31, 30, 28, 30, 31, 32, 30, 28, 30, 29, 30 };
            double[] fcst = CalcFcst(hist, method);
            WriteOutcome(hist, fcst);
            WriteExcel("F", hist, fcst);
        }

        [TestMethod]
        public void Test06_G_AscDiscont()  {
            double[] hist = { 0, 0, 1, 0, 0, 0, 0, 0, 0, 1, 0, 1, 0, 0, 1, 0, 0, 0, 1, 0, 1, 0, 0, 0, 0, 2, 0, 0, 2, 0, 1, 2, 0, 0, 3, 3, 1, 2, 3, 4, 4, 3, 4, 3, 4, 5, 6 };								
            double[] fcst = CalcFcst(hist, method);
            WriteOutcome(hist, fcst);
            WriteExcel("G", hist, fcst);
        }

        [TestMethod]
        public void Test07_H_DescDiscont() {
            double[] hist = { 3,	3,	1,	0,	0,	0,	0,	0,	0,	1,	0,	1,	0,	0,	1,	0,	0,	0,	1,	0,	1,	0,	0,	0,	0,	2,	0,	0,	2,	0,	1,	2,	0,	0,	0,	0, 1, 1, 0, 0, 0, 1, 0, 0, 1, 0  };
            double[] fcst = CalcFcst(hist, method);
            WriteOutcome(hist, fcst);
            WriteExcel("H", hist, fcst);
        }

        [TestMethod]
        public void Test08_I_TrendAscDesc() {
            double[] hist = { 0, 0, 1, 0, 0, 0, 0, 0, 0, 1, 0, 1, 1, 2, 1, 3, 3, 3, 1, 2, 1, 1, 0, 1, 2, 1, 0, 1, 0, 0, 0, 0, 0, 0,	0, 0, 1, 2, 0, 1, 2, 1, 2, 3, 3, 2 };
            double[] fcst = CalcFcst(hist, method);
            WriteOutcome(hist, fcst);
            WriteExcel("I", hist, fcst);
        }

        #endregion

        #region Tests 09-12 Model Change

        [TestMethod]
        public void Test09_J_ModChangeContLevTrend() {
            double[] hist = { 0, 1, 0, 0, 0, 1, 0, 0, 1, 1, 0, 0, 1, 0, 0, 0, 0, 0, 0, 1, 0, 1, 0, 0, 1, 0, 0, 0, 1, 0,	1,	0,	0,	1,	3,	2,	1,	2,	2,	2,	1,	2,	1,	0.2,	3,	3,  };
            double[] fcst = CalcFcst(hist, method);
            WriteOutcome(hist, fcst);
            WriteExcel("J", hist, fcst);
        }

        [TestMethod]
        public void Test10_K_ModChangeIncrAmplitude() {
            double[] hist = { 0, 0, 0.50, 0, 0.5 ,0 , 0, 0.5, 0, 0, 0, 0, 1, 0, 0,	0,	0,	0,	0,	1,	0,	1,	0,	0,	1,	0,	0,	0,	1,	0,	1,	0,	0,	3,	0,	3,	0,	3,	0,	3,	0,	3,	0,	3,	0,	3 };
            double[] fcst = CalcFcst(hist, method);
            WriteOutcome(hist, fcst);
            WriteExcel("K", hist, fcst);
        }

        [TestMethod]
        public void Test11_L_ModChangeContToDisc()  {
            double[] hist = { 4.1, 3, 3.4, 3, 2.8,3, 4, 3.2, 3.5, 3, 2.50,	2.51, 2.62,	2.57,	2.52,	2.51,	2.54,	2.62,	2.73,	2.84,	2,92,	2.89,	2.67,	2.21,	1.55,	0.94,	0.74,	1.26,	2.22,	2.64,	1.79, 0.67,	1.20,	2.53,	1.64,	0.59,	2.27,	1.56,	0.83,	2.45,	0.48,	2.45,	0.48,	2.45,	0.55,	1.88 };
            double[] fcst = CalcFcst(hist, method);
            WriteOutcome(hist, fcst);
            WriteExcel("L", hist, fcst);
        }

        [TestMethod]
        public void Test12_M_ModelChangePending() {
            double[] hist = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 19, 18, 17, 16, 15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0, 1, 2, 3, 4, 5, 6 };
            double[] fcst = CalcFcst(hist, method);
            WriteOutcome(hist, fcst);
            WriteExcel("M", hist, fcst);
        }

        #endregion

        #region Tests 13-16 Seasonality

        [TestMethod]
        public void Test13_N_BasicSeason() {
            double[] hist = { 0, 0, 1, 0, 0, 0, 0, 0, 0, 1, 0, 0, 1, 0, 0, 0, 0, 0, 0, 1, 0, 1, 2, 0, 1, 2, 0, 0, 3, 0, 1, 0, 0, 0, 0, 2, 0, 0, 2, 0, 1, 2, 0, 0, 3, 0 };
            double[] fcst = CalcFcst(hist, method);
            WriteOutcome(hist, fcst);
            WriteExcel("N", hist, fcst);
        }

        [TestMethod]
        public void Test14_O_AditiveHarmonic()  {
            double[] hist = { 1.14, 0.72, 1.41, 0.46, 1.65, 0.25, 1.84, 0.09, 1.96, 0.01, 1.14, 0.72, 1.41, 0.46, 1.65, 0.25, 1.84, 0.09, 1.96, 0.01, 2.00, 0.01, 1.96, 0.08, 1.85, 0.23, 1.67, 0.44, 1.44, 0.70, 1.17, 0.97, 0.89, 1.25, 0.61, 1.51, 0.37, 1.73, 0.18, 1.89, 0.05, 1.98, 0.0, 1.99, 0.03, 1.93 };
            double[] fcst = CalcFcst(hist, method);
            WriteOutcome(hist, fcst);
            WriteExcel("O", hist, fcst);
        }

        [TestMethod]
        public void Test15_P_MultipHarmonic()  {
            double[] hist = { 1.01, 1.04, 1.09, 1.16, 1.25, 1.35, 1.47, 1.60, 1.72, 1.84, 1.01, 1.04, 1.09, 1.16, 1.25, 1.35, 1.47, 1.60, 1.72, 1.84, 1.94, 1.99, 1.99, 1.93, 1.78, 1.55, 1.25, 0.90, 0.55, 0.24, 0.05, 0.01, 0.16, 0.50, 0.97, 1.46, 1.85, 2.00, 1.85, 1.41, 0.82, 0.27, 0.01, 0.16, 0.69, 1.38 };
            double[] fcst = CalcFcst(hist, method);
            WriteOutcome(hist, fcst);
            WriteExcel("P", hist, fcst);
        }

        [TestMethod]
        public void Test16_Q_PendingHarmonic() {
            double[] hist = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 19, 18, 17, 16, 15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0, 1, 2, 3, 4, 5, 6 };
            double[] fcst = CalcFcst(hist, method);
            WriteOutcome(hist, fcst);
            WriteExcel("Q", hist, fcst);
        }

        #endregion

        #region Tests 17-20 ARIMA

        [TestMethod]
        public void Test17_R_BasicArima()  {
            double[] hist = {2.11,	2.18,	1.53,	1.54,	0.36,	0.68,	3.83,	3.82,	3.00,	3.32, 2.11,	2.18,	1.53,	1.54,	0.36,	0.68,	3.83,	3.82,	3.00,	3.32, 2.42,	2.45,	5.42,	5.14,	3.80,	3.99,	2.66,	2.33,	4.67,	4.63,	3.20,	3.20,	2.36,	2.89,	6.32,	6.48,	5.96,	6.02,	5.25,	4.46,	7.10,	6.68,	5.11,	5.35,	4.87,	5.46 };
            double[] fcst = CalcFcst(hist, method);
            WriteOutcome(hist, fcst);
            WriteExcel("R", hist, fcst);
        }

        [TestMethod]
        public void Test18_S() {
            double[] hist = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 19, 18, 17, 16, 15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0, 1, 2, 3, 4, 5, 6 };
            double[] fcst = CalcFcst(hist, method);
            WriteOutcome(hist, fcst);
            WriteExcel("S", hist, fcst);
        }

        [TestMethod]
        public void Test19_T() {
            double[] hist = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 19, 18, 17, 16, 15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0, 1, 2, 3, 4, 5, 6 };
            double[] fcst = CalcFcst(hist, method);
            WriteOutcome(hist, fcst);
            WriteExcel("T", hist, fcst);
        }

        [TestMethod]
        public void Test20_U()  {
            double[] hist = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 19, 18, 17, 16, 15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0, 1, 2, 3, 4, 5, 6 };
            double[] fcst = CalcFcst(hist, method);
            WriteOutcome(hist, fcst);
            WriteExcel("U", hist, fcst);
        }

        #endregion

        #region Tests 21-24 Real Data

        [TestMethod]
        public void Test21_V_RealCont() {
            double[] hist = { 3804,	2643,	4230,	3810,	3767,	3399,	3693,	2625,	2674,	3766, 3804,	2643,	4230,	3810,	3767,	3399,	3693,	2625,	2674,	3766,	3029,	3010,	4094,	2386,	2911,	1983,	3434,	2733,	3229,	3280,	2830,	3843,	2150, 2339,	3455,	2843,	1859,	3631,	2864,	3019,	4141,	3585,	2565,	2284,	1498,	1871,	3006,	2391,	2721,	2130,	2231,	2168,	2483,	1853,	1349 };
            double[] fcst = CalcFcst(hist, method);
            WriteOutcome(hist, fcst);
            WriteExcel("V", hist, fcst);
        }

        [TestMethod]
        public void Test22_W() {
            double[] hist = {  17, 18, 31, 33 ,42.10, 34.2, 100.8, 81.6, 66.5, 34.8, 30.6, 7, 19.8, 92.5, 154.4, 125.9, 84.8, 68.1, 38.5, 22.8, 10.2, 24.1, 82.9, 132, 130.9, 118.1, 89.9, 66.6, 60, 46.9, 41, 21.3, 16, 6.4, 4.1, 6.8, 14.5, 34, 45, 43.1, 47.5, 42.2, 28.1, 10.1, 8.1, 2.5 };
            double[] fcst = CalcFcst(hist, method);
            WriteOutcome(hist, fcst);
            WriteExcel("W", hist, fcst);
        }

        [TestMethod]
        public void Test23_X() {
            double[] hist = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 19, 18, 17, 16, 15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0, 1, 2, 3, 4, 5, 6 };
            double[] fcst = CalcFcst(hist, method);
            WriteOutcome(hist, fcst);
            WriteExcel("X", hist, fcst);
        }

        [TestMethod]
        public void Test24_Y() {
            double[] hist = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 19, 18, 17, 16, 15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0, 1, 2, 3, 4, 5, 6 };
            double[] fcst = CalcFcst(hist, method);
            WriteOutcome(hist, fcst);
            WriteExcel("Y", hist, fcst);
        }

        #endregion

        #region Tests 25-28

        [TestMethod]
        public void Test25_Z() {
            double[] hist = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 19, 18, 17, 16, 15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0, 1, 2, 3, 4, 5, 6 };
            double[] fcst = CalcFcst(hist, method);
            WriteOutcome(hist, fcst);
            WriteExcel("Z", hist, fcst);
        }

        [TestMethod]
        public void Test26_AA()  {
            double[] hist = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 19, 18, 17, 16, 15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0, 1, 2, 3, 4, 5, 6 };
            double[] fcst = CalcFcst(hist, method);
            WriteOutcome(hist, fcst);
            WriteExcel("AA", hist, fcst);
        }

        [TestMethod]
        public void Test27_AB() {
            double[] hist = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 19, 18, 17, 16, 15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0, 1, 2, 3, 4, 5, 6 };
            double[] fcst = CalcFcst(hist, method);
            WriteOutcome(hist, fcst);
            WriteExcel("AB", hist, fcst);
        }

        [TestMethod]
        public void Test28_AC() {
            double[] hist = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 19, 18, 17, 16, 15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0, 1, 2, 3, 4, 5, 6 };
            double[] fcst = CalcFcst(hist, method);
            WriteOutcome(hist, fcst);
            WriteExcel("AC", hist, fcst);
        }

        #endregion

        #region Tests 29-32

        [TestMethod]
        public void Test29_AD() {
            double[] hist = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 19, 18, 17, 16, 15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0, 1, 2, 3, 4, 5, 6 };
            double[] fcst = CalcFcst(hist, method);
            WriteOutcome(hist, fcst);
            WriteExcel("AD", hist, fcst);
        }

        [TestMethod]
        public void Test30_AE() {
            double[] hist = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 19, 18, 17, 16, 15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0, 1, 2, 3, 4, 5, 6 };
            double[] fcst = CalcFcst(hist, method);
            WriteOutcome(hist, fcst);
            WriteExcel("AE", hist, fcst);
        }

        [TestMethod]
        public void Test31_AF()  {
            double[] hist = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 19, 18, 17, 16, 15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0, 1, 2, 3, 4, 5, 6 };
            double[] fcst = CalcFcst(hist, method);
            WriteOutcome(hist, fcst);
            WriteExcel("AF", hist, fcst);
        }

        [TestMethod]
        public void Test32_AG()  {
            double[] hist = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 19, 18, 17, 16, 15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0, 1, 2, 3, 4, 5, 6 };
            double[] fcst = CalcFcst(hist, method);
            WriteOutcome(hist, fcst);
            WriteExcel("AG", hist, fcst);
        }

        #endregion

        #region Tests 33-36

        [TestMethod]
        public void Test33_AH()  {
            double[] hist = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 19, 18, 17, 16, 15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0, 1, 2, 3, 4, 5, 6 };
            double[] fcst = CalcFcst(hist, method);
            WriteOutcome(hist, fcst);
            WriteExcel("AH", hist, fcst);
        }

        [TestMethod]
        public void Test34_AI()  {
            double[] hist = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 19, 18, 17, 16, 15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0, 1, 2, 3, 4, 5, 6 };
            double[] fcst = CalcFcst(hist, method);
            WriteOutcome(hist, fcst);
            WriteExcel("AI", hist, fcst);
        }

        [TestMethod]
        public void Test35_AJ()  {
            double[] hist = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 19, 18, 17, 16, 15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0, 1, 2, 3, 4, 5, 6 };
            double[] fcst = CalcFcst(hist, method);
            WriteOutcome(hist, fcst);
            WriteExcel("AJ", hist, fcst);
        }

        [TestMethod]
        public void Test36_AK() {
            double[] hist = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 19, 18, 17, 16, 15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0, 1, 2, 3, 4, 5, 6 };
            double[] fcst = CalcFcst(hist, method);
            WriteOutcome(hist, fcst);
            WriteExcel("AK", hist, fcst);
        }

        #endregion

        #region Tests 37-40

        [TestMethod]
        public void Test37_AL()  {
            double[] hist = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 19, 18, 17, 16, 15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0, 1, 2, 3, 4, 5, 6 };
            double[] fcst = CalcFcst(hist, method);
            WriteOutcome(hist, fcst);
            WriteExcel("AL", hist, fcst);
        }

        [TestMethod]
        public void Test38_AM() {
            double[] hist = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 19, 18, 17, 16, 15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0, 1, 2, 3, 4, 5, 6 };
            double[] fcst = CalcFcst(hist, method);
            WriteOutcome(hist, fcst);
            WriteExcel("AM", hist, fcst);
        }

        [TestMethod]
        public void Test39_AN() {
            double[] hist = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 19, 18, 17, 16, 15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0, 1, 2, 3, 4, 5, 6 };
            double[] fcst = CalcFcst(hist, method);
            WriteOutcome(hist, fcst);
            WriteExcel("AN", hist, fcst);
        }

        [TestMethod]
        public void Test40_AO() {
            double[] hist = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 19, 18, 17, 16, 15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0, 1, 2, 3, 4, 5, 6 };
            double[] fcst = CalcFcst(hist, method);
            WriteOutcome(hist, fcst);
            WriteExcel("AO", hist, fcst);
        }

        #endregion

        #endregion

        #region Calculation Method

        private double[] CalcFcst(double[] hist, FcstMethodType method) {
            List<double> histList = new List<double>(hist);
            ITsForecast tsFcst = null;
            
            switch (method) {
                case FcstMethodType.Naive:
                    int movingWindow = 6;
                    NaiveFcst.AverageType avg = NaiveFcst.AverageType.simple;
                    tsFcst = new NaiveFcst(movingWindow,avg); 
                    break;
                case FcstMethodType.Regression:
                    int grade = 1;
                    tsFcst = new RegressionFcst(grade);
                    break;
                case FcstMethodType.HoltWinters:
                    double a = 0.3;
                    double b = 0.3;
                    tsFcst = new HoltWintersFcst(a,b);
                    break;
                case FcstMethodType.ZChart:
                    int rollingPrd = 6;
                    tsFcst = new ZChartFcst(rollingPrd);
                    break;
                case FcstMethodType.ARIMA:
                    int arOrder = 4;
                    int maOrder = 3;
                    tsFcst = new ARIMAFcst(arOrder, maOrder);
                    break;
                case FcstMethodType.DLMKalman:
                    int testSetSize = 6;
                    tsFcst = new StatFcst(testSetSize); 
                    break;
                case FcstMethodType.NeuNet:
                    movingWindow = 6;
                    int epochs = 20;
                    tsFcst = new TDNNFcst(movingWindow, epochs);
                    break;
            }

            tsFcst.LoadData(histList, 0);
            tsFcst.Calculate();
            if (tsFcst.GetFcstRes() != FcstResType.Ok) { Console.WriteLine(tsFcst.GetFcstRes().ToString()); }
            double[] fcst = tsFcst.GetFcst(k);
            return fcst;        
        }

        #endregion

        #region Auxiliar Methods

        private void WriteOutcome(double[] hist, double[] fcst)    {
            Console.WriteLine("t\tHist\tMean");
            Console.WriteLine("--\t------\t------\t------\t------");
            for (int t = 0; t < hist.Length; t++) { Console.WriteLine((t + 1) + "\t" + hist[t].ToString("0.00") + "\t"); }
            for (int t = 0; t < fcst.Length; t++) { Console.WriteLine((t + 1) + "\t" + fcst[t].ToString("0.00") + "\t"); }
        }

        private void WriteExcel(string serie, double[] hist, double[] fcst) {
            string strConnnectionOle = @"Provider=Microsoft.Jet.OLEDB.4.0;" + @"Data Source=..\..\TestFcst.xls;" + @"Extended Properties=" + '"' + "Excel 8.0;HDR=NO" + '"';
            OleDbConnection oleConn = new OleDbConnection(strConnnectionOle);
            oleConn.Open();
            OleDbCommand cmd = new OleDbCommand();
            cmd.Connection = oleConn;
            for (int i = 2; i <= 47; i++) { 
                cmd.CommandText = "UPDATE [Data$" + serie + i + ":" + serie + i + "] SET F1=" + hist[i-2];
                cmd.CommandText = cmd.CommandText.Replace(",", ".");
                cmd.ExecuteNonQuery();
            }
            for (int i = 0; i < 12; i++)  {
                int index = 48 + i;
                cmd.CommandText = "UPDATE [Data$" + serie + index + ":" + serie + index + "] SET F1=" + Math.Round(fcst[i]);
                cmd.CommandText = cmd.CommandText.Replace(",", ".");
                cmd.ExecuteNonQuery();
            }
            oleConn.Close(); 
        }

        #endregion

    }
}
