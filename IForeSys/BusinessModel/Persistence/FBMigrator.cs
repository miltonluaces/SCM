#region Imports

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FirebirdSql.Data.FirebirdClient;

#endregion

namespace AibuSet {

    internal class FBMigrator : DBJob {

        #region Fields

        private string fbConnStr;
        private FbConnection fbConn;
        private SysEnvironment sysEnv = SysEnvironment.GetInstance();

        #endregion

        #region Constructor

        internal FBMigrator() {
            Initialize();
        }

        #endregion

        #region internal Methods

        internal void Migrate() {

            string dbName = "AILogSysDemo";
            SysEnvironment.Provider dbProvider = SysEnvironment.Provider.Firebird;
            string dbConnStr = fbConnStr;

            List<ITTItem> nodes = new List<ITTItem>(sysEnv.Nodes.Values);
            SaveNodes(nodes, dbName, dbProvider, fbConnStr);

            List<ITTItem> suppliers = new List<ITTItem>(sysEnv.Suppliers.Values);
            SaveSuppliers(suppliers, dbName, dbProvider, fbConnStr);

            List<DbObject> productObjs = new List<DbObject>();
            new Product().ReadMany(productObjs, "1=1");
            SaveProducts(productObjs, dbName, dbProvider, fbConnStr);
            
            List<DbObject> skuObjs = new List<DbObject>();
            new Sku().ReadMany(skuObjs, "state=2");
            SaveSkus(skuObjs, dbName, dbProvider, fbConnStr);
            
            List<DbObject> timeSeriesObjs = new List<DbObject>();
            new TimeSeries().ReadMany(timeSeriesObjs, "1=1");
            SaveTimeSeries(timeSeriesObjs, dbName, dbProvider, fbConnStr);
        }

        internal void SaveNodes(IList<ITTItem> nodes, string dbName, SysEnvironment.Provider dbProvider, string dbConnStr)  {
            new Node().GetBroker().Initialize(dbName, dbProvider, dbConnStr);
            foreach (Node node in nodes) { node.SaveUpdate(); }
            new Node().GetBroker().Initialize();
        }

        internal void SaveSuppliers(IList<ITTItem> suppliers, string dbName, SysEnvironment.Provider dbProvider, string dbConnStr)  {
            new Supplier().GetBroker().Initialize(dbName, dbProvider, dbConnStr);
            foreach (Supplier sup in suppliers) { sup.SaveUpdate(); }
            new Supplier().GetBroker().Initialize();
        }

        internal void SaveProducts(IList<DbObject> productObjs, string dbName, SysEnvironment.Provider dbProvider, string dbConnStr)  {
            new Product().GetBroker().Initialize(dbName, dbProvider, dbConnStr);
            foreach (Product pr in productObjs) { pr.SaveUpdate(); }
            new Product().GetBroker().Initialize();
        }

        internal void SaveSkus(IList<DbObject> skuObjs, string dbName, SysEnvironment.Provider dbProvider, string dbConnStr) {
            new Sku().GetBroker().Initialize(dbName, dbProvider, dbConnStr);
            foreach (Sku sku in skuObjs) { sku.SaveUpdate(); }
            new Sku().GetBroker().Initialize();
        }

        internal void SaveTimeSeries(IList<DbObject> timeSeriesObjs, string dbName, SysEnvironment.Provider dbProvider, string dbConnStr)  {
            new TimeSeries().GetBroker().Initialize(dbName, dbProvider, dbConnStr);
            foreach (TimeSeries ts in timeSeriesObjs) { ts.SaveUpdate(); }
            new TimeSeries().GetBroker().Initialize();
        }

        internal void SavePuOrder(IList<DbObject> puOrderObjs, string dbName, SysEnvironment.Provider dbProvider, string dbConnStr) {
            new PuOrder().GetBroker().Initialize(dbName, dbProvider, dbConnStr);
            foreach (PuOrder po in puOrderObjs) { po.SaveUpdate(); }
            new PuOrder().GetBroker().Initialize();
        }

        internal void SavePuOrders(IList<DbObject> puOrderObjs) { 
        
        }
        
        #endregion

        #region Private Methods

        private void Initialize() {
            FbConnectionStringBuilder fcsb = new FbConnectionStringBuilder();
            fcsb.Dialect = 3;
            fcsb.Charset = "UTF8";
            fcsb.Password = "masterkey";
            fcsb.UserID = "SYSDBA";
            fcsb.Database = @"AILogSysDemo.fdb";
            fcsb.ServerType = FbServerType.Embedded;
            fbConnStr = fcsb.ToString();
            fbConn = new FbConnection(fbConnStr);
        }
    
        #endregion
    }
}
