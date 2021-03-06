#region Imports

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Planning;

#endregion

namespace Testing {

    [TestClass]
    //Tests FcstPol = max
    public class TestOrderGenMax {


        #region Tests 1 : OSSZ (OnlyStock Sim Zero)

        [TestCategory("1 OSSZ (OnlyStock SimZero)"), TestMethod]
        public void Test01_OSSZ_LotNone1() {
            TestGroup(OrderGen.TriggerFcstPolType.max, OrderGen.ReplenFcstPolType.simAsMin, simFcst = 0, lotSize = 1, lotIncs = 1);
            Test(hoy, end, stock = 0, trOrders = 0, 0, 10, desQty = 0);
        }

        [TestCategory("1 OSSZ (OnlyStock SimZero)"), TestMethod]
        public void Test02_OSSZ_LotNone2() {
            TestGroup(OrderGen.TriggerFcstPolType.max, OrderGen.ReplenFcstPolType.simAsMin, simFcst = 0, lotSize = 1, lotIncs = 1);
            Test(hoy, end, stock = 0, trOrders = 0, 0, 10, desQty = 3);
        }
        
        [TestCategory("1 OSSZ (OnlyStock SimZero)"), TestMethod]
        public void Test02_OSSZ_LotNone() {
            TestGroup(OrderGen.TriggerFcstPolType.max, OrderGen.ReplenFcstPolType.simAsMin, simFcst = 0, lotSize = 1, lotIncs = 1);
            Test(hoy, end, stock = 5, trOrders = 0, 0, 10, desQty = 3);
        }

        [TestCategory("1 OSSZ (OnlyStock SimZero)"), TestMethod]
        public void Test03_OSSZ_LotSize() {
            TestGroup(OrderGen.TriggerFcstPolType.max, OrderGen.ReplenFcstPolType.simAsMin, simFcst = 0, lotSize = 3, lotIncs = 1);
            Test(hoy, end, stock = 2, trOrders = 0, 0, 10, desQty = 3);
        }

        [TestCategory("1 OSSZ (OnlyStock SimZero)"), TestMethod]
        public void Test04_OSSZ_LotSize() {
            TestGroup(OrderGen.TriggerFcstPolType.max, OrderGen.ReplenFcstPolType.simAsMin, simFcst = 0, lotSize = 10, lotIncs = 1);
            Test(hoy, end, stock = 12, trOrders = 0, 0, 10, desQty = 3);
        }

        [TestCategory("1 OSSZ (OnlyStock SimZero)"), TestMethod]
        public void Test05_OSSZ_LotIncs() {
            TestGroup(OrderGen.TriggerFcstPolType.max, OrderGen.ReplenFcstPolType.simAsMin, simFcst = 0, lotSize = 3, lotIncs = 3);
            Test(hoy, end, stock = 3, trOrders = 0, 0, 10, desQty = 3);
        }

        [TestCategory("1 OSSZ (OnlyStock SimZero)"), TestMethod]
        public void Test06_OSSZ_LotIncs() {
            TestGroup(OrderGen.TriggerFcstPolType.max, OrderGen.ReplenFcstPolType.simAsMin, simFcst = 0, lotSize = 12, lotIncs = 3);
            Test(hoy, end, stock = 4, trOrders = 0, 0, 10, desQty = 3);
        }
        
        #endregion

        #region Tests 2 : OSSV (OnlyStock Sim Value)

        [TestCategory("2 OSSV (OnlyStock SimValue)"), TestMethod]
        public void Test07_OS_LotNone() {
            TestGroup(OrderGen.TriggerFcstPolType.max, OrderGen.ReplenFcstPolType.simAsMin, simFcst = 3, lotSize = 1, lotIncs = 1);
            Test(hoy, end, stock = 0, trOrders = 0, 0, 10, desQty = 3);
        }

        [TestCategory("2 OSSV (OnlyStock SimValue)"), TestMethod]
        public void Test08_OS_LotNone() {
            TestGroup(OrderGen.TriggerFcstPolType.max, OrderGen.ReplenFcstPolType.simAsMin, simFcst = 10, lotSize = 1, lotIncs = 1);
            Test(hoy, end, stock = 5, trOrders = 0, 0, 10, desQty = 3);
        }

        [TestCategory("2 OSSV (OnlyStock SimValue)"), TestMethod]
        public void Test09_OS_LotSize() {
            TestGroup(OrderGen.TriggerFcstPolType.max, OrderGen.ReplenFcstPolType.simAsMin, simFcst = 12, lotSize = 3, lotIncs = 1);
            Test(hoy, end, stock = 2, trOrders = 0, 0, 10, desQty = 3);
        }

        [TestCategory("2 OSSV (OnlyStock SimValue)"), TestMethod]
        public void Test10_OS_LotSize() {
            TestGroup(OrderGen.TriggerFcstPolType.max, OrderGen.ReplenFcstPolType.simAsMin, simFcst = 4, lotSize = 1, lotIncs = 1);
            Test(hoy, end, stock = 3, trOrders = 0, 0, 10, desQty = 3);
        }

        [TestCategory("2 OSSV (OnlyStock SimValue)"), TestMethod]
        public void Test11_OS_LotIncs() {
            TestGroup(OrderGen.TriggerFcstPolType.max, OrderGen.ReplenFcstPolType.simAsMin, simFcst = 7, lotSize = 1, lotIncs = 1);
            Test(hoy, end, stock = 2, trOrders = 0, 0, 10, desQty = 3);
        }

        [TestCategory("2 OSSV (OnlyStock SimValue)"), TestMethod]
        public void Test12_OS_LotIncs() {
            TestGroup(OrderGen.TriggerFcstPolType.max, OrderGen.ReplenFcstPolType.simAsMin, simFcst = 1, lotSize = 1, lotIncs = 1);
            Test(hoy, end, stock = 2, trOrders = 0, 0, 10, desQty = 3);
        }

        #endregion

        #region Tests 3 : STSZ (StockTrans Sim Zero)

        [TestCategory("3 STSZ (StockTrans SimZero)"), TestMethod]
        public void Test13_STSZ_LotNone() {
            TestGroup(OrderGen.TriggerFcstPolType.max, OrderGen.ReplenFcstPolType.simAsMin, simFcst = 0, lotSize = 1, lotIncs = 1);
            Test(hoy, end, stock = 0, trOrders = 2, 0, 10, desQty = 3);
        }

        [TestCategory("3 STSZ (StockTrans SimZero)"), TestMethod]
        public void Test14_STSZ_LotNone() {
            TestGroup(OrderGen.TriggerFcstPolType.max, OrderGen.ReplenFcstPolType.simAsMin, simFcst = 0, lotSize = 1, lotIncs = 1);
            Test(hoy, end, stock = 5, trOrders = 4, 0, 10, desQty = 3);
        }

        [TestCategory("3 STSZ (StockTrans SimZero)"), TestMethod]
        public void Test15_STSZ_LotSize() {
            TestGroup(OrderGen.TriggerFcstPolType.max, OrderGen.ReplenFcstPolType.simAsMin, simFcst = 0, lotSize = 3, lotIncs = 1);
            Test(hoy, end, stock = 4, trOrders = 1, 0, 10, desQty = 3);
        }

        [TestCategory("3 STSZ (StockTrans SimZero)"), TestMethod]
        public void Test16_STSZ_LotSize() {
            TestGroup(OrderGen.TriggerFcstPolType.max, OrderGen.ReplenFcstPolType.simAsMin, simFcst = 0, lotSize = 1, lotIncs = 1);
            Test(hoy, end, stock = 7, trOrders = 2, 0, 10, desQty = 3);
        }

        [TestCategory("3 STSZ (StockTrans SimZero)"), TestMethod]
        public void Test17_STSZ_LotIncs() {
            TestGroup(OrderGen.TriggerFcstPolType.max, OrderGen.ReplenFcstPolType.simAsMin, simFcst = 0, lotSize = 1, lotIncs = 1);
            Test(hoy, end, stock = 8, trOrders = 3, 0, 10, desQty = 3);
        }

        [TestCategory("3 STSZ (StockTrans SimZero)"), TestMethod]
        public void Test18_STSZ_LotIncs() {
            TestGroup(OrderGen.TriggerFcstPolType.max, OrderGen.ReplenFcstPolType.simAsMin, simFcst = 0, lotSize = 1, lotIncs = 1);
            Test(hoy, end, stock = 12, trOrders = 2, 0, 10, desQty = 3);
        }

        #endregion
        
        #region Tests 4 : STSV (StockTrans SimValue)

        [TestCategory("4 STSV (StockTrans SimValue)"), TestMethod]
        public void Test19_STSV_LotNone() {
            TestGroup(OrderGen.TriggerFcstPolType.max, OrderGen.ReplenFcstPolType.simAsMin, simFcst = 10, lotSize = 1, lotIncs = 1);
            Test(hoy, end, stock = 0, trOrders = 4, 0, 10, desQty = 3);
        }

        [TestCategory("4 STSV (StockTrans SimValue)"), TestMethod]
        public void Test20_STSV_LotNone() {
            TestGroup(OrderGen.TriggerFcstPolType.max, OrderGen.ReplenFcstPolType.simAsMin, simFcst = 12, lotSize = 1, lotIncs = 1);
            Test(hoy, end, stock = 0, trOrders = 5, 0, 10, desQty = 3);
        }

        [TestCategory("4 STSV (StockTrans SimValue)"), TestMethod]
        public void Test21_STSV_LotSize() {
            TestGroup(OrderGen.TriggerFcstPolType.max, OrderGen.ReplenFcstPolType.simAsMin, simFcst = 13, lotSize = 3, lotIncs = 1);
            Test(hoy, end, stock = 0, trOrders = 2, 0, 10, desQty = 3);
        }

        [TestCategory("4 STSV (StockTrans SimValue)"), TestMethod]
        public void Test22_STSV_LotSize() {
            TestGroup(OrderGen.TriggerFcstPolType.max, OrderGen.ReplenFcstPolType.simAsMin, simFcst = 7, lotSize = 1, lotIncs = 1);
            Test(hoy, end, stock = 0, trOrders = 4, 0, 10, desQty = 3);
        }

        [TestCategory("4 STSV (StockTrans SimValue)"), TestMethod]
        public void Test23_STSV_LotIncs() {
            TestGroup(OrderGen.TriggerFcstPolType.max, OrderGen.ReplenFcstPolType.simAsMin, simFcst = 4, lotSize = 1, lotIncs = 1);
            Test(hoy, end, stock = 0, trOrders = 5, 0, 10, desQty = 3);
        }

        [TestCategory("4 STSV (StockTrans SimValue)"), TestMethod]
        public void Test24_STSV_LotIncs() {
            TestGroup(OrderGen.TriggerFcstPolType.max, OrderGen.ReplenFcstPolType.simAsMin, simFcst = 1, lotSize = 1, lotIncs = 1);
            Test(hoy, end, stock = 0, trOrders = 6, 0, 10, desQty = 3);
        }

        #endregion

        #region Test context

        private OrderGen orGen;
        private OrderGen.TriggerFcstPolType triggerFcstPol;
        private OrderGen.ReplenFcstPolType replenFcstPol;
        private Queue<Order> supplies;
        private int ldtime;
        private int rptime; 
        private double simFcst = 0;
        private double lotSize = 0;
        private double lotIncs = 0;
        private double stock = 10;
        private double trOrders = 0;
        private double[] statFcsts;
        private double desQty = 0;
        private DateTime hoy = new DateTime(2015, 1, 1);
        private DateTime end = new DateTime(2015, 1, 7);

        [ClassInitialize()]
        public static void InitSuite(TestContext testContext) { }

        [ClassCleanup()]
        public static void EndSuite() { }

        [TestInitialize()]
        public void InitTest() {
        }

        [TestCleanup()]
        public void EndTest() { 
        }

    #endregion

        #region Auxiliar Methods

        private void TestGroup(OrderGen.TriggerFcstPolType triggerFcstPol, OrderGen.ReplenFcstPolType replenFcstPol, double simFcst, double lotSize, double lotIncs) {
            orGen = new OrderGen(triggerFcstPol, replenFcstPol, supplies, simFcst, lotSize, lotIncs);
        }
        
        private void Test(DateTime date, DateTime endLdtime, double stock, double transOrders, double statLtFcst, double statRpFcst, double desQty) {
            double qty = orGen.Calculate(date, endLdtime, stock = 10, statLtFcst, statRpFcst);
            Assert.AreEqual(desQty, qty);
        }

        #endregion

    }
}
