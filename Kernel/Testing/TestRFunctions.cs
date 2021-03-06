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
    public class TestRFunctions {

        #region Parameter fields

        string path = "C:/Rlang/";
        string version = "3.3.1";
        RFunctions rf = new RFunctions("C:/Rlang/", "3.3.1");
        static double[] arr1 = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }; 
        static double[] arr2 = { 10, 9, 8, 7, 6, 5, 4, 3, 2, 1 };
        static double[] arr3 = { 90, 89, 88, 87, 86, 85, 84, 83, 82, 81 };
        static double[] arr4 = { 2, 3, 4, 6, 8, 10, 12, 14, 16, 18 };
        static double[] arr5 = { 3, 6, 9, 12, 15, 18, 21, 24, 27, 30 };
        static double[] arr6 = { 3, 1, 4, 1, 5, 9, 2, 6, 5, 3 };
        static List<double> lst1 = new List<double>(arr1);
        static List<double> lst2 = new List<double>(arr2);
        static List<double> lst3 = new List<double>(arr3);
        static List<double> lst4 = new List<double>(arr4);
        static double[] calArr = { 1, 1, 1, 1, 1, 0, 0, 1, 1, 1 };
        static double[] reArr = { 0, 0, 0, 1, 1, 0, 0, 1, 1, 0 };
        static double[][] mat, mat1, mat2;
        static AR ts = new AR(arr1);
        static AR re = new AR(reArr);
        static AR fcst = new AR(arr2);
        static AR cal = new AR(calArr);

        #endregion

        #region Test context

        [ClassInitialize()]
        public static void InitSuite(TestContext testContext) {
            mat = new double[4][];
            mat[0] = arr1;
            mat[1] = arr2;
            mat[2] = arr3;
            mat[3] = arr4;
            mat1 = new double[3][];
            mat1[0] = arr1;
            mat1[1] = arr2;
            mat1[2] = arr3;
            mat2 = new double[3][];
            mat2[0] = arr4;
            mat2[1] = arr5;
            mat2[2] = arr6;
        }

        [ClassCleanup()]
        public static void EndSuite() { }

        [TestInitialize()]
        public void InitTest() {
        }

        [TestCleanup()]
        public void EndTest() { }

        #endregion

        #region Tests Math Functions

        [TestMethod]
        public void Test11_LineEq() {
            double x = 2, x1 = 1, y1 = 2, x2 = 4, y2 = 8; 
            double y = rf.LineEq(x, x1, y1, x2, y2);
            Console.WriteLine(y);   
        }
          
        [TestMethod]
        public void Test12_ExpByFive() {
            double exp;
            int minDecExp=4;
            for(int i=0;i<12;i++) {
                exp = rf.GetExponentBy5(minDecExp, i);
                Console.WriteLine(exp);   
            }
        }
     
        #endregion

        #region Tests Ts PreProcess

        [TestMethod]
        public void Test21_EliminateHolidays() {
            ts[6] = 0; ts[7] = 0;
            AR newTs = rf.EliminateHolidays(ts, cal);
            Console.WriteLine(newTs);   
        }

        [TestMethod]
        public void Test22_CalcSdRatio() {
            int mw = 3;
            double ratio = rf.CalcSdRatio(ts, mw, fcst);
            Console.WriteLine(ratio);
        }

          [TestMethod]
          public void Test23_GetPerStats() {
              AR perStats = rf.GetPerStats(ts, re);
              Console.WriteLine(perStats);   
        }

        #endregion
          
        #region Tests Hypothesis

        [TestMethod]
        public void Test31_CalcRecurrentTs() {
            int[] iniEffArr = {4, 8}; int[] endEffArr = {5, 10};
            AR iniEff = new AR(iniEffArr);
            AR endEff = new AR(endEffArr);
            double alpha = 0.05;
            double prop = -1;
            int n=1;
            AR tsRec = new AR(arr1);
            AR pValues = new AR(iniEff.Length);
            dynamic res = rf.CalcRecurrentTs(ts, iniEff, endEff, alpha, n);
            Console.WriteLine(res.tsRec);
            Console.WriteLine(res.pValues);
            Console.WriteLine(res.prop); 
        }

        #endregion

    }
}
