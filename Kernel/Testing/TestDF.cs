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
    public class TestDF {

        #region Parameter fields

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
        static double[][] mat, mat1, mat2;
        static AR ts = new AR(arr1);

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

        #region Tests

        [TestMethod]
        public void Test1_Creation() {
            DF df1 = new DF(); Console.WriteLine(df1);
            DF df2 = new DF(mat); Console.WriteLine(df2);
            DF df3 = new DF(df2); Console.WriteLine(df3);
            DF df4 = new DF(4); Console.WriteLine(df4);
            DF df5 = new DF(arr1, true); Console.WriteLine(df5);
            DF df6 = new DF(arr1, false); Console.WriteLine(df6);
            DF df7 = new DF(ts, true); Console.WriteLine(df7);
            DF df8 = new DF(ts, false); Console.WriteLine(df8);
        }

        [TestMethod]
        public void Test2_Properties() {
            DF df1 = new DF(mat);
            Console.WriteLine(df1.NCol);
            Console.WriteLine(df1.NRow);
            Console.WriteLine(df1[2, 4]);
            Console.WriteLine(df1[1, 3]);
        }

        [TestMethod]
        public void Test3_Operators() {
            DF df1 = new DF(mat1);
            DF df2 = new DF(mat2);
            DF dfRes1 = df1 + 5; Console.WriteLine(dfRes1);
            DF dfRes2 = df1 - 6; Console.WriteLine(dfRes2);
            DF dfRes3 = df1 * 2; Console.WriteLine(dfRes3);
            DF dfRes4 = df1 / 3; Console.WriteLine(dfRes4);
            DF dfRes5 = df1 + df2; Console.WriteLine(dfRes5);
            DF dfRes6 = df1 - df2; Console.WriteLine(dfRes6);
            DF dfRes7 = df1 * df2; Console.WriteLine(dfRes7);
            DF dfRes8 = df1 / df2; Console.WriteLine(dfRes8);
        }

        [TestMethod]
        public void Test4_Statics() {
            double[] arr = {3,4,5};
            List<double> lst = new List<double>(arr);
            DF df1 = new DF(mat1);
            DF df2 = new DF(mat2);
            DF dfRes1 = DF.AddColumn(df1, lst4); Console.WriteLine(dfRes1);
            DF dfRes2 = DF.AddColumn(df1, new AR(arr2)); Console.WriteLine(dfRes2);
            DF dfRes3 = DF.AddRow(df1, lst); Console.WriteLine(dfRes3);
            DF dfRes4 = DF.AddRow(df1, new AR(arr)); Console.WriteLine(dfRes4);
            DF dfRes5 = DF.CBind(df1, df2); Console.WriteLine(dfRes5);
            DF dfRes6 = DF.RBind(df1, df2); Console.WriteLine(dfRes6);
        }

        [TestMethod]
        public void Test5_Instance() {
            double[] arr = { 3, 4, 5 };
            List<double> lst = new List<double>(arr);
            DF df1 = new DF(mat1); Console.WriteLine(df1);
            DF df2 = new DF(mat2);
            df1.AddColumn(lst1); Console.WriteLine(df1);
            df1.AddColumn(new AR(arr2)); Console.WriteLine(df1);
            df2.AddRow(lst); Console.WriteLine(df2);
            df2.AddRow(new AR(arr)); Console.WriteLine(df2);
            df1 = new DF(mat1);
            df2 = new DF(mat2);
            df1.CBind(df2); Console.WriteLine(df1);
            df1 = new DF(mat1);
            df1.RBind(df2); Console.WriteLine(df1);
            df1 = new DF(mat1);
            int[] sel = { 2, 3 };
            DF dfRes1 = df1.SubDfCols(1, 2); Console.WriteLine(dfRes1);
            DF dfRes2 = df1.SubDfCols(sel); Console.WriteLine(dfRes2);
            DF dfRes3 = df1.SubDfRows(2, 3); Console.WriteLine(dfRes3);
            DF dfRes4 = df1.SubDfRows(sel); Console.WriteLine(dfRes4);
        }

          
        #endregion

    }
}
