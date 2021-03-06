#region ImporAR

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
    public class TestAR {

        #region Parameter fields

        private int k = 12;
        private FcstMethodType method;

        private enum MethodType { Naive, Regression, ExpSmooth, DLMKalm, TDNN };

        double[] arr1 = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0, 1, 2, 3, 4, 5, 6 };
        double[] arr2 = { 1, 1.5, 2, 2.5, 3, 3.5, 4, 4.5, 5, 5.5, 6, 6.5, 7, 7.5, 8, 8.5, 9, 10, 10.5, 11, 11.5, 12, 12.5, 13, 13.5, 14, 14.5, 15, 15.5, 16, 16.5, 17, 17.5, 18, 18.5, 19, 20, 20.5, 21, 21.5, 22, 22.5, 23, 23.5, 24, 24.5 };
        double[] arr3 = { 90, 89, 88, 87, 86, 85, 84, 83, 82, 81, 80, 79, 78, 77, 76, 75, 75, 74, 73, 72, 71, 70, 69, 68, 67, 66, 65, 64, 63, 62, 61, 60, 59, 58, 57, 56, 55, 54, 53, 52, 51, 50, 49, 48, 47, 46 };
        double[] arr4 = { 3, 3, 1, 0, 0, 0, 0, 0, 0, 1, 0, 1, 0, 0, 1, 0, 0, 0, 1, 0, 1, 0, 0, 0, 0, 2, 0, 0, 2, 0, 1, 2, 0, 0, 0, 0, 1, 1, 0, 0, 0, 1, 0, 0, 1, 0 };
               
        #endregion

        #region Test context

        [ClassInitialize()]
        public static void InitSuite(TestContext testContext) { }

        [ClassCleanup()]
        public static void EndSuite() { }

        [TestInitialize()]
        public void InitTest() {
        }

        [TestCleanup()]
        public void EndTest() { }

        #endregion

        #region TesAR

        [TestMethod]
        public void Test1_Creation() {
            AR ar0 = new AR(12); Console.WriteLine(ar0);
            AR ar1 = new AR(arr1); Console.WriteLine(ar1);
        }

        [TestMethod]
        public void Test2_Properties() {
            AR ar1 = new AR(arr1); Console.WriteLine(ar1);
            Console.WriteLine(ar1[3]);
            Console.WriteLine(ar1[5]);
            Console.WriteLine(ar1[ar1.Length]); 
            Console.WriteLine(ar1.Length); 
        }

        [TestMethod]
        public void Test3_Operators() {
            double[] arrX = { 1, 2, 3, 4, 5, 6, 7 };
            double[] arrY = { 2, 2, 2, 2, 2, 2, 2 }; 
            AR ar0 = new AR(arrX); Console.WriteLine(ar0);
            AR ar1 = new AR(arrY); Console.WriteLine(ar1);

            AR arSum1 = ar0 + 5; Console.WriteLine(arSum1);
            AR ArSub1 = ar0 - 3; Console.WriteLine(ArSub1);
            AR ArPro1 = ar0 * 3; Console.WriteLine(ArPro1);
            AR ArDiv1 = ar0 / 2; Console.WriteLine(ArDiv1);
        
            AR arSum2 = ar0 + ar1; Console.WriteLine(arSum2);
            AR ArSub2 = ar0 - ar1; Console.WriteLine(ArSub2);
            AR ArPro2 = ar0 * ar1; Console.WriteLine(ArPro2);
            AR ArDiv2 = ar0 / ar1; Console.WriteLine(ArDiv2);
        }

        [TestMethod]
        public void Test4_Statics() {
            double[] arrX = { 1, 2, 3, 4, 5, 6, 7 };
            double[] arrY = { 4, 2, 3, 2, 1, 0, 9 };
            AR ar0 = new AR(arrX); Console.WriteLine(ar0);
            AR ar1 = new AR(arrY); Console.WriteLine(ar1);

            AR ArAbs = AR.Abs(ar0 - ar1); Console.WriteLine(ArAbs);
            AR ArC1 = AR.C(ar0, 28); Console.WriteLine(ArC1);
            AR ArC2 = AR.C(ar0, ar1); Console.WriteLine(ArC2);
            AR.Copy(ar0, ar1, 2, 4, 1, 3); Console.WriteLine(ar1);
            AR ArRep = AR.Rep(7, 5); Console.WriteLine(ArRep);
            double sum = AR.Sum(ar0, ar1); Console.WriteLine(sum);
        }

        [TestMethod]
        public void Test5_Instance() {
            double[] arrX = { 1, 2, 3, 4, 5, 6, 7 };
            double[] arrY = { 4, 2, 3, 2, 1, 0, 9 };
            AR ar0 = new AR(arrX); Console.WriteLine(ar0);
            AR ar1 = new AR(arrY); Console.WriteLine(ar1);

            double[] arr = ar0.ToArray(); Console.WriteLine(arr);
            AR ArClo = ar0.Clone(); Console.WriteLine(ArClo);
            AR ArSub = ar0.SubAr(3, 5); Console.WriteLine(ArSub);
            double sum1 = ar0.Sum(); Console.WriteLine(sum1);
            double sum2 = ar0.Sum(3, 6); Console.WriteLine(sum2);
            double min = ar0.Min(); Console.WriteLine(min);
            double max = ar0.Max(); Console.WriteLine(max);
            AR ArNeg = ar0 * -2;
            ArNeg.ElimNegatives(); Console.WriteLine(ArNeg);
        }

        [TestMethod]
        public void Test6_Statistic() {
            double[] arrX = { 1, 2, 3, 4, 5, 6, 7 };
            AR ar0 = new AR(arrX); Console.WriteLine(ar0);
            double min = ar0.Min(); Console.WriteLine(min);
            double max = ar0.Max(); Console.WriteLine(max);
            double mean = ar0.Mean(); Console.WriteLine(mean);
            double var = ar0.Variance(); Console.WriteLine(var);
            double sd = ar0.Sd(); Console.WriteLine(sd);
        }
        
        #endregion

    }
}
