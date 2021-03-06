#region Imports

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Configuration;
using System.Resources;
using System.Reflection;
using System.Text;
using FirebirdSql.Data.FirebirdClient;

#endregion

namespace AibuSet {

    internal class DBGen : DBJob {

        #region Fields

        private List<string> queryCreateTables;
        private List<string> queryCreateIndexes;
        private List<string> queryDropTables;
        private char[] sep;

        #endregion

        #region Constructor

        internal DBGen()
            : base() {
            SetQueryCreateTables();
            SetQueryCreateIndexes();
            SetQueryDropTables();
            sep = new char[1];
            sep[0] = ' ';
        }

        #endregion

        #region internal Methods


        internal void CreateDatabase(string dbName) {
            Console.WriteLine("Creating database...");
            try {
                switch (dbProvider) {
                    case SysEnvironment.Provider.SqlServer: conn = new SqlConnection(dbConnStr); break;
                    case SysEnvironment.Provider.MySql: conn = new OleDbConnection(dbConnStr); break;
                    case SysEnvironment.Provider.Firebird: conn = new FbConnection(dbConnStr); break;
                }
            }
            catch (Exception ex) { Console.WriteLine("Could not create connection"); return; }

            string query = "CREATE DATABASE " + dbName;

            try {
                conn.Open();
                switch (dbProvider) {
                    case SysEnvironment.Provider.SqlServer:
                        cmd = new SqlCommand(query, (SqlConnection)conn);
                        break;
                    case SysEnvironment.Provider.MySql:
                        cmd = new OleDbCommand(query, (OleDbConnection)conn);
                        break;
                    case SysEnvironment.Provider.Firebird:
                        cmd = new FbCommand(query, (FbConnection)conn);
                        break;
                }
                cmd.ExecuteNonQuery();
                Console.WriteLine("Database created");
            }

            catch (Exception ex) { Console.WriteLine("Error. Could not create database"); }
            finally {
                if (conn.State == ConnectionState.Open) { conn.Close(); }
            }
        }

        internal void CreateTablesAndIndexes() {
            Console.WriteLine("Dropping tables...\n");
            try {
                switch (dbProvider) {
                    case SysEnvironment.Provider.SqlServer: conn = new SqlConnection(dbConnStr); break;
                    case SysEnvironment.Provider.MySql: conn = new OleDbConnection(dbConnStr); break;
                    case SysEnvironment.Provider.Firebird: conn = new FbConnection(dbConnStr); break;
                }
                conn.Open();
            }
            catch (Exception ex) { Console.WriteLine("Could not create connection"); return; }

            for (int i = 0; i < queryDropTables.Count; i++) {
                try {
                    switch (dbProvider) {
                        case SysEnvironment.Provider.SqlServer:
                            cmd = new SqlCommand(queryDropTables[i], (SqlConnection)conn); break;
                        case SysEnvironment.Provider.MySql:
                            cmd = new OleDbCommand(queryDropTables[i], (OleDbConnection)conn); break;
                        case SysEnvironment.Provider.Firebird:
                            cmd = new FbCommand(queryDropTables[i], (FbConnection)conn); break;
                    }
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex) { Console.Write("NOT "); }
                WriteResult(queryDropTables[i]);
            }

            Console.WriteLine("\nCreating tables...\n");
            try {
                for (int i = 0; i < queryCreateTables.Count; i++) {
                    switch (dbProvider) {
                        case SysEnvironment.Provider.SqlServer:
                            cmd = new SqlCommand(queryCreateTables[i], (SqlConnection)conn); break;
                        case SysEnvironment.Provider.MySql:
                            cmd = new OleDbCommand(queryCreateTables[i], (OleDbConnection)conn); break;
                        case SysEnvironment.Provider.Firebird:
                            cmd = new FbCommand(queryCreateTables[i], (FbConnection)conn); break;
                    }
                    cmd.ExecuteNonQuery();
                    WriteResult(queryCreateTables[i]);
                }
                Console.WriteLine("\nProcess ended Ok");
            }
            catch (Exception ex) {
                Console.WriteLine("Error. Could not create tables " + ex.StackTrace);
            }

            Console.WriteLine("\nCreating indexes...\n");
            try {
                for (int i = 0; i < queryCreateIndexes.Count; i++) {
                    switch (dbProvider) {
                        case SysEnvironment.Provider.SqlServer:
                            cmd = new SqlCommand(queryCreateIndexes[i], (SqlConnection)conn); break;
                        case SysEnvironment.Provider.MySql:
                            cmd = new OleDbCommand(queryCreateIndexes[i], (OleDbConnection)conn); break;
                        case SysEnvironment.Provider.Firebird:
                            cmd = new FbCommand(queryCreateIndexes[i], (FbConnection)conn); break;
                    }
                    cmd.ExecuteNonQuery();
                    WriteResult(queryCreateIndexes[i]);
                }
                Console.WriteLine("\nProcess ended Ok");
            }
            catch (Exception ex) {
                Console.WriteLine("Error. Could not create indexes " + ex.StackTrace);
            }
            finally {
                if (conn.State == ConnectionState.Open) { conn.Close(); }
            }
        }

        #endregion

        #region Private Methods

        private void SetQueryDropTables() {
            queryDropTables = new List<string>();
            string query;
            query = "DROP TABLE AilTrOrderItem"; queryDropTables.Add(query);
            query = "DROP TABLE AilTrOrder"; queryDropTables.Add(query);
            query = "DROP TABLE AilPuOrderItem"; queryDropTables.Add(query);
            query = "DROP TABLE AilPuOrder"; queryDropTables.Add(query);
            query = "DROP TABLE AilSupplyItem"; queryDropTables.Add(query);
            query = "DROP TABLE AilSupply"; queryDropTables.Add(query);
            query = "DROP TABLE AilSku"; queryDropTables.Add(query);
            query = "DROP TABLE AilBomDemand"; queryDropTables.Add(query);
            query = "DROP TABLE AilDemand"; queryDropTables.Add(query);
            query = "DROP TABLE AilBomRelation"; queryDropTables.Add(query);
            query = "DROP TABLE AilRelation"; queryDropTables.Add(query);
            query = "DROP TABLE AilNode"; queryDropTables.Add(query);
            query = "DROP TABLE AilProduct"; queryDropTables.Add(query);
            query = "DROP TABLE AilCalendar"; queryDropTables.Add(query);
            query = "DROP TABLE AilCustomer"; queryDropTables.Add(query);
            query = "DROP TABLE AilSupplier"; queryDropTables.Add(query);
            query = "DROP TABLE AilBinLargeObj"; queryDropTables.Add(query);
            query = "DROP TABLE AilTimeSeries"; queryDropTables.Add(query);
            query = "DROP TABLE AilParameter"; queryDropTables.Add(query);
            query = "DROP TABLE AilSimFcstDist"; queryDropTables.Add(query);
        }

        private void SetQueryCreateTables() {
            queryCreateTables = new List<string>();
            string query;

            query = "CREATE TABLE AilParameter (" +
                    "parameterId int NOT NULL," +
                    "code varchar(30), " +
                    "name varchar(50), " +
                    "type varchar(1), " +
                    "value varchar(30), " +
                    "creation datetime NOT NULL," +
                    "CONSTRAINT PkParameter PRIMARY KEY (parameterId));";
            queryCreateTables.Add(query);

            query = "CREATE TABLE AilSupplier (" +
                    "supplierId int NOT NULL," +
                    "code varchar(30), " +
                    "name varchar(30), " +
                    "address varchar(120), " +
                    "city varchar(30), " +
                    "phone varchar(30), " +
                    "creation datetime NOT NULL," +
                    "CONSTRAINT PkSupplier PRIMARY KEY (supplierId));";
            queryCreateTables.Add(query);

            query = "CREATE TABLE AilCustomer (" +
                    "customerId int NOT NULL," +
                    "code varchar(30), " +
                    "name varchar(30), " +
                    "address varchar(120), " +
                    "city varchar(30), " +
                    "phone varchar(30), " +
                    "creation datetime NOT NULL," +
                    "CONSTRAINT PkCustomer PRIMARY KEY (customerId));";
            queryCreateTables.Add(query);

            query = "CREATE TABLE AilProduct (" +
                     "productId int NOT NULL," +
                     "code varchar(30), " +
                     "descr varchar(50), " +
                     "cost float, " +
                     "price float, " +
                     "creation datetime NOT NULL," +
                     "CONSTRAINT PkProduct PRIMARY KEY (productId));";
            queryCreateTables.Add(query);

            query = "CREATE TABLE AilCalendar (" +
                     "calendarId int NOT NULL," +
                     "code varchar(30), " +
                     "firstDate datetime NOT NULL, " +
                     "lastDate datetime NOT NULL, " +
                     "weekHolsMon int, " +
                     "weekHolsTue int, " +
                     "weekHolsWed int, " +
                     "weekHolsThu int, " +
                     "weekHolsFri int, " +
                     "weekHolsSat int, " +
                     "weekHolsSun int, " +
                     "creation datetime NOT NULL," +
                     "CONSTRAINT PkCalendar PRIMARY KEY (calendarId));";
            queryCreateTables.Add(query);

            query = "CREATE TABLE AilNode (" +
                     "nodeId int NOT NULL," +
                     "code varchar(30), " +
                     "descr varchar(50), " +
                     "schLevel int, " +
                     "aggrHist int, " +
                     "aggrFcst int, " +
                     "creation datetime NOT NULL," +
                     "calendarId int NOT NULL REFERENCES AilCalendar(calendarId), " +
                     "CONSTRAINT PkNode PRIMARY KEY (nodeId));";
            queryCreateTables.Add(query);

            query = "CREATE TABLE AilRelation (" +
                      "relationId int NOT NULL," +
                      "code varchar(30), " +
                      "originId int NOT NULL REFERENCES AilNode(nodeId), " +
                      "targetId int NOT NULL REFERENCES AilNode(nodeId), " +
                      "creation datetime NOT NULL," +
                      "CONSTRAINT PkRelation PRIMARY KEY (relationId));";
            queryCreateTables.Add(query);

            query = "CREATE TABLE AilBomRelation (" +
                     "bomRelationId int NOT NULL," +
                     "code varchar(30), " +
                     "originId int NOT NULL REFERENCES AilNode(nodeId), " +
                     "targetId int NOT NULL REFERENCES AilNode(nodeId), " +
                     "qty float, " +
                     "offset int, " +
                     "creation datetime NOT NULL," +
                     "CONSTRAINT PkBomRelation PRIMARY KEY (bomRelationId));";
            queryCreateTables.Add(query);

            query = "CREATE TABLE AilSku (" +
                     "skuId int NOT NULL," +
                     "code varchar(30), " +
                     "rsmpFirstDate datetime, " +
                     "serviceLevel float, " +
                     "leadTime int, " +
                     "simFcst float, " +
                     "replenishmentTime int, " +
                     "lotSize int, " +
                     "roundingQty int, " +
                     "planJustCustOrders int, " +
                     "obsRisk float, " +
                     "obsExpValue float, " +
                     "planningRule varchar(20), " +
                     "policy varchar(20), " +
                     "rsmpFilteringProb float, " +
                     "rsmpNoise float, " +
                     "rsmpClusterThreshold float, " +
                     "stock float, " +
                     "bckSafetyStock float, " +
                     "bckMinPercOver float, " +
                     "ltFcstMinPercOver float, " +
                     "orderMinPercOver float, " +
                     "lastNPeriods int, " +
                     "bomLevel int, " +
                     "firstSellingDate datetime, " +
                     "lastSupplyDate datetime, " +
                     "rsmpRollingFcstHist float, " +
                     "leadTimeFcstManual float, " +
                     "rsmpRollingFcstManual float, " +
                     "replenishmentFcstManual float, " +
                     "leadTimeFcstOrigin varchar(10), " +
                     "verySMP int, " +
                     "minServiceLevel float, " +
                     "maxServiceLevel float, " +
                     "cost float, " +
                     "volume float, " +
                     "predictable int, " +
                     "fcstHorizon int, " +
                     "planHorizon int, " +
                     "firstHistDate datetime, " +
                     "uStr1 varchar(20)" +
                     "uStr2 varchar(20)" +
                     "uStr3 varchar(20)" +
                     "uNum1 float" +
                     "uNum2 float" +
                     "uNum3 float" +
                     "uDate1 datetime" +
                     "uDate2 datetime" +
                     "uDate3 datetime" +
                     "nodeId int NOT NULL REFERENCES AilNode(nodeId), " +
                     "productId int NOT NULL REFERENCES AilProduct(productId), " +
                     "supplierId int NOT NULL REFERENCES AilSupplier(supplierId), " +
                     "supplierCalId int NOT NULL REFERENCES AilCalendar(calendarId), " +
                     "creation datetime NOT NULL," +
                     "CONSTRAINT PkSku PRIMARY KEY (skuId));";
            queryCreateTables.Add(query);

            query = "CREATE TABLE AilDemand (" +
                         "demandId int NOT NULL," +
                         "code varchar(30), " +
                         "desiredDate datetime, " +
                         "initialQty float, " +
                         "actualQty float, " +
                         "orderPrice float, " +
                         "orderId int, " +
                         "lineId int," +
                         "customerId int REFERENCES AilCustomer(customerId), " +
                         "skuId int NOT NULL REFERENCES AilSku(skuId), " +
                         "creation datetime NOT NULL," +
                         "CONSTRAINT PkDemand PRIMARY KEY (demandId));";
            queryCreateTables.Add(query);

            query = "CREATE TABLE AilBomDemand (" +
                        "bomDemandId int NOT NULL," +
                        "code varchar(30), " +
                        "desiredDate datetime, " +
                        "initialQty float, " +
                        "actualQty float, " +
                        "orderPrice float, " +
                        "orderId int, " +
                        "lineId int," +
                        "customerId int REFERENCES AilCustomer(customerId), " +
                        "skuId int NOT NULL REFERENCES AilSku(skuId), " +
                        "bomRelationId int NOT NULL REFERENCES AilRelation(relationId), " +
                        "creation datetime NOT NULL," +
                        "CONSTRAINT PkBomDemand PRIMARY KEY (bomDemandId));";
            queryCreateTables.Add(query);

            query = "CREATE TABLE AilSupply (" +
                     "supplyId int NOT NULL," +
                     "code varchar(30), " +
                     "initialQty float, " +
                     "actualQty float, " +
                     "releaseDate datetime, " +
                     "receptionDate datetime, " +
                     "consumptionDate datetime, " +
                     "orderId int, " +
                     "lineId int," +
                     "orderType varchar(1), " +
                     "backOrderQty float, " +
                     "bomQty float, " +
                     "outQty float, " +
                     "lTFcstQty float, " +
                     "orderQty float, " +
                     "supplierId int NOT NULL REFERENCES AilSupplier(supplierId), " +
                     "skuId int NOT NULL REFERENCES AilSku(skuId), " +
                     "creation datetime NOT NULL," +
                     "CONSTRAINT PkSupply PRIMARY KEY (supplyId));";
            queryCreateTables.Add(query);

            query = "CREATE TABLE AilSupplyItem (" +
                      "supplyItemId int NOT NULL," +
                      "code varchar(30), " +
                      "Qty float, " +
                      "supplyId int NOT NULL REFERENCES AilSupply(supplyId), " +
                      "demandId int REFERENCES AilDemand(demandId), " +
                      "creation datetime NOT NULL," +
                      "CONSTRAINT PkSupplyLine PRIMARY KEY (supplyItemId));";
            queryCreateTables.Add(query);

            query = "CREATE TABLE AilPuOrder (" +
                      "puOrderId int IDENTITY(1,1) NOT NULL," +
                      "code varchar(30), " +
                      "qty float, " +
                      "ordDate datetime, " +
                      "rcpDate datetime, " +
                      "backQty float, " +
                      "stckQty float, " +
                      "fcstQty float, " +
                      "bomQty float, " +
                      "outQty float, " +
                      "state int, " +
                      "orderNumber int, " +
                      "lineNumber int, " +
                      "skuId int NOT NULL REFERENCES AilSku(skuId), " +
                      "creation datetime NOT NULL," +
                      "CONSTRAINT PkOrder PRIMARY KEY (orderId));";
            queryCreateTables.Add(query);

            query = "CREATE TABLE AilPuOrderItem (" +
                     "puOrderItemId int IDENTITY(1,1) NOT NULL," +
                     "code varchar(30), " +
                     "qty float, " +
                     "puOrderId int NOT NULL REFERENCES AilPuOrder(puOrderId), " +
                     "demandId int REFERENCES AilDemand(demandId), " +
                     "creation datetime NOT NULL," +
                     "CONSTRAINT PkPlOrderLine PRIMARY KEY (orderItemId));";
            queryCreateTables.Add(query);

            query = "CREATE TABLE AilTrOrder (" +
                     "trOrderId int IDENTITY(1,1) NOT NULL," +
                     "code varchar(30)," +
                     "ordDate datetime," +
                     "recDate datetime," +
                     "reqQty float, " +
                     "allocQty float, " +
                     "backQty float," +
                     "bomQty float, " +
                     "outQty float, " +
                     "fcstQty float, " +
                     "stckQty float," +
                     "status int, " +
                     "supSkuId int NOT NULL REFERENCES AilSku(skuId)," +
                     "conSkuId int NOT NULL REFERENCES AilSku(skuId)," +
                     "creation datetime NOT NULL," +
                     "CONSTRAINT PkOrder PRIMARY KEY (trOrderId));";

            query = "CREATE TABLE AilTrOrderItem (" +
                     "trOrderItemId int IDENTITY(1,1) NOT NULL," +
                     "code varchar(30), " +
                     "reqQty float, " +
                     "allocQty float, " +
                     "dmdType int, " +
                     "trOrderId int NOT NULL REFERENCES AilTrOrder(trOrderId)," +
                     "demandId int REFERENCES AilDemand(demandId), " +
                     "creation datetime NOT NULL," +
                     "CONSTRAINT PkPlOrderLine PRIMARY KEY (trOrderItemId));";

            query = "CREATE TABLE AilBinLargeObj (" +
                    "blobId int NOT NULL," +
                    "code varchar(30), " +
                    "data varchar(max), " +
                    "creation datetime NOT NULL," +
                    "CONSTRAINT PkBlobId PRIMARY KEY (blobId));";
            queryCreateTables.Add(query);

            query = "CREATE TABLE AilTimeSeries (" +
                    "timeSeriesId int NOT NULL, " +
                    "code varchar(30), " +
                    "leadtime int, " +
                    "firstDate datetime, " +
                    "lastDate datetime, " +
                    "dayHist varchar(max), " +
                    "ldtHist varchar(max), " +
                    "ldtFcst varchar(max), " +
                    "ldtOver varchar(max), " +
                    "skuId int NOT NULL REFERENCES AilSku(skuId), " +
                    "creation datetime NOT NULL," +
                    "CONSTRAINT PkTimeSeriesId PRIMARY KEY (timeSeriesId));";
            queryCreateTables.Add(query);

            query = "CREATE TABLE AilSimFcstDist (" +
                    "simFcstDistId int NOT NULL," +
                    "code varchar(30), " +
                    "name varchar(50), " +
                    "lower float, " +
                    "step  float, " +
                    "percentiles varchar(max), " +
                    "skuId int NOT NULL REFERENCES AilSku(skuId), " +
                    "creation datetime NOT NULL," +
                     "CONSTRAINT PkSimFcstDist PRIMARY KEY (simFcstDistId));";
            queryCreateTables.Add(query);
        }

        private void SetQueryCreateIndexes() {
            queryCreateIndexes = new List<string>();
            string query;

            query = "CREATE INDEX index_TimeSeries_SkuId ON AilTimeSeries(skuId);";
            queryCreateIndexes.Add(query);
            query = "CREATE INDEX index_SimFcstDist_SkuId ON AilSimFcstDist(skuId);";
            queryCreateIndexes.Add(query);
        }

        private void WriteResult(string query) {
            string[] tokens = query.Split(sep);
            Console.WriteLine(tokens[0] + " " + tokens[1] + " " + tokens[2]);
        }

        #endregion

    }
}
