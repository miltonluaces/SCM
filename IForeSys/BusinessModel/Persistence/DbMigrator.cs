#region Imports

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace AibuSet {

    internal class DbMigrator : DBJob  {

        #region Fields

        private SysEnvironment sysEnv = SysEnvironment.GetInstance();

        #endregion

        #region Constructor

        internal DbMigrator()  {
        }

        #endregion

        #region internal Methods

        internal void MigrateAll(string dbName, SysEnvironment.Provider dbProvider, string dbConnStr)  {
            MigrateNodes(dbName, dbProvider, dbConnStr);
            MigrateSuppliers(dbName, dbProvider, dbConnStr);
            MigrateProducts(dbName, dbProvider, dbConnStr);
            MigrateSkus(dbName, dbProvider, dbConnStr);
            MigrateTimeSeries(dbName, dbProvider, dbConnStr);
            MigratePuOrders(dbName, dbProvider, dbConnStr);
        }

        internal void MigrateNodes(string dbName, SysEnvironment.Provider dbProvider, string dbConnStr)  {
            Broker brkrNode = new Node().GetBroker();
            brkrNode.Initialize(dbName, dbProvider, dbConnStr);
            foreach (Node node in sysEnv.Nodes.Values)  {
                if (node.Code == "CEDIS") { continue; }
                node.UStr2 = node.Id.ToString();
                node.SaveUpdate();
            }
            brkrNode.Initialize();
        }

        internal void MigrateSuppliers(string dbName, SysEnvironment.Provider dbProvider, string dbConnStr)  {
            Broker brkrSupplier = new Supplier().GetBroker();
            brkrSupplier.Initialize(dbName, dbProvider, dbConnStr);
            foreach (Supplier supplier in sysEnv.Suppliers.Values)  {
                supplier.UStr2 = supplier.Id.ToString();
                supplier.SaveUpdate();
            }
            brkrSupplier.Initialize();
        }
     
        internal void MigrateProducts(string dbName, SysEnvironment.Provider dbProvider, string dbConnStr)  {
            Broker brkrProduct = new Product().GetBroker();
            List<DbObject> productObjs = new List<DbObject>();
            brkrProduct.ReadMany(productObjs, "1=1");
            brkrProduct.Initialize(dbName, dbProvider, dbConnStr);
            foreach (Product prod in productObjs) {
                prod.UStr2 = prod.Id.ToString();
                prod.SaveUpdate(); 
            }
            brkrProduct.Initialize();
        }

        internal void MigrateSkus(string dbName, SysEnvironment.Provider dbProvider, string dbConnStr) {
            Broker brkrSku = new Sku().GetBroker();
            List<DbObject> skuObjs = new List<DbObject>();
            brkrSku.ReadMany(skuObjs, "state=2");
            brkrSku.Initialize(dbName, dbProvider, dbConnStr);
            foreach (Sku sku in skuObjs) {
                sku.UStr2 = sku.Id.ToString();
                sku.SaveUpdate(); 
            }
            brkrSku.Initialize();
        }

        internal void MigrateTimeSeries(string dbName, SysEnvironment.Provider dbProvider, string dbConnStr)  {
            Broker brkrTimeSeries = new TimeSeries().GetBroker();
            List<DbObject> timeSeriesObjs = new List<DbObject>();
            brkrTimeSeries.ReadMany(timeSeriesObjs, "1=1");
            brkrTimeSeries.Initialize(dbName, dbProvider, dbConnStr);
            foreach (TimeSeries ts in timeSeriesObjs) {
                if (ts.Sku.Id > 1660000) { break; }
                ts.UStr2 = ts.Id.ToString();
                ts.SaveUpdate(); 
            }
            brkrTimeSeries.Initialize();
        }

        internal void MigratePuOrders(string dbName, SysEnvironment.Provider dbProvider, string dbConnStr)  {
            Broker brkrPuOrders = new PuOrder().GetBroker();
            List<DbObject> puOrderObjs = new List<DbObject>();
            brkrPuOrders.ReadMany(puOrderObjs, "1=1");
            brkrPuOrders.Initialize(dbName, dbProvider, dbConnStr);
            foreach (PuOrder po in puOrderObjs)  {
                po.SaveUpdate();
            }
            brkrPuOrders.Initialize();
        }
        
        #endregion

        #region Private Methods

            //try  {
            //    Initialize(dbName, dbProvider, dbConnStr);
            //    conn.Open();
            //    foreach (Node node in sysEnv.Nodes.Values)  {
            //        if (node.Code == "CEDIS") { continue; }
            //        cmd = GetDbCommand("SET IDENTITY_INSERT AilNode ON");
            //        cmd.ExecuteNonQuery();
            //        cmd = GetDbCommand("UPDATE AilNode SET nodeId = " + node.Id + " WHERE code = '" + node.Code + "'");
            //        cmd.ExecuteNonQuery();
            //    }
            //    cmd = GetDbCommand("SET IDENTITY_INSERT AilNode OFF");
            //    cmd.ExecuteNonQuery();
            //    conn.Close();
            //}
            //catch (Exception e) { Console.WriteLine(e.StackTrace); }
            //finally  {
            //    try { conn.Close(); }
            //    catch (Exception e) { Console.WriteLine(e.StackTrace); }
            //}
 
        #endregion
    }
}
