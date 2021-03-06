#region Imports

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AibuSet;

#endregion

namespace AibuSet {

    internal class BrkrMgr {

        #region Singleton

        private static BrkrMgr bm;

        private BrkrMgr() {
            brkrProduct = new BrkrProduct();
            brkrCalendar = new BrkrCalendar(); 
            brkrCustomer = new BrkrCustomer();
            brkrDemand = new BrkrDemand();
            brkrNode = new BrkrNode();
            brkrOrder = new BrkrPuOrder();
            brkrTrOrder = new BrkrTrOrder();
            brkrProduct = new BrkrProduct();
            brkrRelation = new BrkrRelation();
            brkrSku = new BrkrSku();
            brkrSupplier = new BrkrSupplier();
            brkrSupply = new BrkrSupply();
            brkrParameter = new BrkrParameter();
            brkrProcess = new BrkrProcess(); 
            brkrTimeSeries = new BrkrTimeSeries();
            brkrBomRelation = new BrkrBomRelation();
            brkrBomDemand = new BrkrBomDemand();
            brkrFcstOverride = new BrkrFcstOverride();
            brkrSimFcstDist = new BrkrSimFcstDist();
            brkrPlan = new BrkrPlan();
            brkrBinLargeObj = new BrkrBinLargeObj();
            brkrUser = new BrkrUser();
            brkrFcstModel = new BrkrFcstModel();
            brkrSubSku = new BrkrSubSku();
            
            brkrSku.UFieldCount = 3;
        }

        internal static BrkrMgr GetInstance() {
            if (bm == null) { bm = new BrkrMgr(); }
            return bm;
        }

        internal static void Initialize() { 
            bm = new BrkrMgr(); 
        }

        #endregion

        #region Broker Fields

        private Broker broker;
        private Broker brkrCalendar;
        private Broker brkrCustomer;
        private Broker brkrDemand;
        private Broker brkrNode;
        private Broker brkrOrder;
        private Broker brkrOrderItem;
        private Broker brkrTrOrder;
        private Broker brkrTrOrderItem;
        private Broker brkrProduct;
        private Broker brkrRelation;
        private Broker brkrSku;
        private Broker brkrSupplier;
        private Broker brkrSupply;
        private Broker brkrSupplyItem;
        private Broker brkrParameter;
        private Broker brkrProcess;
        private Broker brkrTimeSeries;
        private Broker brkrBomRelation;
        private Broker brkrBomDemand;
        private Broker brkrFcstOverride;
        private Broker brkrSimFcstDist;
        private Broker brkrPlan;
        private Broker brkrBinLargeObj;
        private Broker brkrUser;
        private Broker brkrFcstModel;
        private Broker brkrSubSku;
  
   
        #endregion
      
        #region Broker accessors

        internal BrkrCalendar GetBroker(Calendar obj) { return (BrkrCalendar)brkrCalendar; }
        internal BrkrCustomer GetBroker(Customer obj) { return (BrkrCustomer)brkrCustomer; }
        internal BrkrDemand GetBroker(Demand obj) { return (BrkrDemand)brkrDemand; }
        internal BrkrBomDemand GetBroker(BomDemand obj) { return (BrkrBomDemand)brkrBomDemand; }
        internal BrkrNode GetBroker(Node obj) { return (BrkrNode)brkrNode; }
        internal BrkrPuOrder GetBroker(PuOrder obj) { return (BrkrPuOrder)brkrOrder; }
        internal BrkrTrOrder GetBroker(TrOrder obj) { return (BrkrTrOrder)brkrTrOrder; }
        internal BrkrProduct GetBroker(Product obj) { return (BrkrProduct)brkrProduct; }
        internal BrkrRelation GetBroker(Relation obj) { return (BrkrRelation)brkrRelation; }
        internal BrkrSku GetBroker(Sku obj) { return (BrkrSku)brkrSku; }
        internal BrkrSupplier GetBroker(Supplier obj) { return (BrkrSupplier)brkrSupplier; }
        internal BrkrSupply GetBroker(Supply obj) { return (BrkrSupply)brkrSupply; }
        internal BrkrParameter GetBroker(Parameter obj) { return (BrkrParameter)brkrParameter; }
        internal BrkrProcess GetBroker(Process obj) { return (BrkrProcess)brkrProcess; }
        internal BrkrTimeSeries GetBroker(TimeSeries obj) { return (BrkrTimeSeries)brkrTimeSeries; }
        internal BrkrBomRelation GetBroker(BomRelation obj) { return (BrkrBomRelation)brkrBomRelation; }
        internal BrkrFcstOverride GetBroker(FcstOverride obj) { return (BrkrFcstOverride)brkrFcstOverride; }
        internal BrkrSimFcstDist GetBroker(SimFcstDist obj) { return (BrkrSimFcstDist)brkrSimFcstDist; }
        internal BrkrPlan GetBroker(Plan obj) { return (BrkrPlan)brkrPlan; }
        internal BrkrBinLargeObj GetBroker(BinLargeObj obj) { return (BrkrBinLargeObj)brkrBinLargeObj; }
        internal BrkrUser GetBroker(User obj) { return (BrkrUser)brkrUser; }
        internal BrkrFcstModel GetBroker(FcstModel obj) { return (BrkrFcstModel)brkrFcstModel; }
        internal BrkrSubSku GetBroker(SubSku obj) { return (BrkrSubSku)brkrSubSku; }
         
        #endregion

        #region Static virtual Methods

        internal void ReadMany(List<Calendar> objs, string condition) {
            List<DbObject> dbObjs = new List<DbObject>();
            ((BrkrCalendar)brkrCalendar).ReadMany(dbObjs, condition);
            foreach (Calendar obj in dbObjs) { objs.Add(obj); }
        }

        internal void ReadMany(List<Customer> objs, string condition) {
            List<DbObject> dbObjs = new List<DbObject>();
            ((BrkrCustomer)brkrCustomer).ReadMany(dbObjs, condition);
            foreach (Customer obj in dbObjs) { objs.Add(obj); }
        }

        internal void ReadMany(List<Demand> objs, string condition) {
            List<DbObject> dbObjs = new List<DbObject>();
            ((BrkrDemand)brkrDemand).ReadMany(dbObjs, condition);
            foreach (Demand obj in dbObjs) { objs.Add(obj); }
        }

        internal void ReadMany(List<Node> objs, string condition) {
            List<DbObject> dbObjs = new List<DbObject>();
            ((BrkrNode)brkrNode).ReadMany(dbObjs, condition);
            foreach (Node obj in dbObjs) { objs.Add(obj); }
        }

        internal void ReadMany(List<PuOrder> objs, string condition) {
            List<DbObject> dbObjs = new List<DbObject>();
            ((BrkrPuOrder)brkrOrder).ReadMany(dbObjs, condition);
            foreach (PuOrder obj in dbObjs) { objs.Add(obj); }
        }

        internal void ReadMany(List<TrOrder> objs, string condition) {
            List<DbObject> dbObjs = new List<DbObject>();
            ((BrkrTrOrder)brkrOrder).ReadMany(dbObjs, condition);
            foreach (TrOrder obj in dbObjs) { objs.Add(obj); }
        }

        internal void ReadMany(List<Product> objs, string condition) {
            List<DbObject> dbObjs = new List<DbObject>();
            ((BrkrProduct)brkrProduct).ReadMany(dbObjs, condition);
            foreach (Product obj in dbObjs) { objs.Add(obj); }
        }

        internal void ReadMany(List<Relation> objs, string condition) {
            List<DbObject> dbObjs = new List<DbObject>();
            ((BrkrRelation)brkrRelation).ReadMany(dbObjs, condition);
            foreach (Relation obj in dbObjs) { objs.Add(obj); }
        }

        internal void ReadMany(List<Sku> objs, string condition) {
            List<DbObject> dbObjs = new List<DbObject>();
            ((BrkrSku)brkrSku).ReadMany(dbObjs, condition);
            foreach (Sku obj in dbObjs) { objs.Add(obj); }
        }

        internal void ReadMany(List<Supplier> objs, string condition) {
            List<DbObject> dbObjs = new List<DbObject>();
            ((BrkrSupplier)brkrSupplier).ReadMany(dbObjs, condition);
            foreach (Supplier obj in dbObjs) { objs.Add(obj); }
        }

        internal void ReadMany(List<Supply> objs, string condition)
        {
            List<DbObject> dbObjs = new List<DbObject>();
            ((BrkrSupply)brkrSupply).ReadMany(dbObjs, condition);
            foreach (Supply obj in dbObjs) { objs.Add(obj); }
        }
        internal void ReadMany(List<Parameter> objs, string condition)
        {
            List<DbObject> dbObjs = new List<DbObject>();
            ((BrkrParameter)brkrParameter).ReadMany(dbObjs, condition);
            foreach (Parameter obj in dbObjs) { objs.Add(obj); }
        }

        internal void ReadMany(List<TimeSeries> objs, string condition) { 
            List<DbObject> dbObjs = new List<DbObject>();
            ((BrkrTimeSeries)brkrTimeSeries).ReadMany(dbObjs, condition);
            foreach (TimeSeries obj in dbObjs) { objs.Add(obj); }
        }

        internal void ReadMany(List<BomRelation> objs, string condition) {
            List<DbObject> dbObjs = new List<DbObject>();
            ((BrkrBomRelation)brkrBomRelation).ReadMany(dbObjs, condition);
            foreach (BomRelation obj in dbObjs) { objs.Add(obj); }
        }

        internal void ReadMany(List<FcstOverride> objs, string condition) {
            List<DbObject> dbObjs = new List<DbObject>();
            ((BrkrFcstOverride)brkrFcstOverride).ReadMany(dbObjs, condition);
            foreach (FcstOverride obj in dbObjs) { objs.Add(obj); }
        }

        internal void ReadMany(List<SimFcstDist> objs, string condition) {
            List<DbObject> dbObjs = new List<DbObject>();
            ((BrkrSimFcstDist)brkrSimFcstDist).ReadMany(dbObjs, condition);
            foreach (SimFcstDist obj in dbObjs) { objs.Add(obj); }
        }

        internal void ReadMany(List<Plan> objs, string condition) {
            List<DbObject> dbObjs = new List<DbObject>();
            ((BrkrPlan)brkrPlan).ReadMany(dbObjs, condition);
            foreach (Plan obj in dbObjs) { objs.Add(obj); }
        }

        internal void ReadMany(List<BinLargeObj> objs, string condition) {
            List<DbObject> dbObjs = new List<DbObject>();
            ((BrkrBinLargeObj)brkrBinLargeObj).ReadMany(dbObjs, condition);
            foreach (BinLargeObj obj in dbObjs) { objs.Add(obj); }
        }

        internal void ReadMany(List<User> objs, string condition) {
            List<DbObject> dbObjs = new List<DbObject>();
            ((BrkrUser)brkrUser).ReadMany(dbObjs, condition);
            foreach (User obj in dbObjs) { objs.Add(obj); }
        }

        internal void ReadMany(List<FcstModel> objs, string condition)  {
            List<DbObject> dbObjs = new List<DbObject>();
            ((BrkrFcstModel)brkrFcstModel).ReadMany(dbObjs, condition);
            foreach (FcstModel obj in dbObjs) { objs.Add(obj); }
        }

        internal void ReadMany(List<SubSku> objs, string condition) {
            List<DbObject> dbObjs = new List<DbObject>();
            ((BrkrSubSku)brkrSubSku).ReadMany(dbObjs, condition);
            foreach (SubSku obj in dbObjs) { objs.Add(obj); }
        }

        
        #endregion

        #region Extended Read Methods

        internal void Read(TimeSeries ts, Sku sku) { GetBroker(ts).Read(sku); }
        internal void Read(SubSku ts, Sku sku) { GetBroker(ts).Read(sku); }
        
        #endregion

     }
}
