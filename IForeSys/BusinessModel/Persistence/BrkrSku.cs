#region Imports

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Configuration;
using System.Resources;
using System.Reflection;

#endregion

namespace AibuSet
{

    internal class BrkrSku : Broker
    {

        #region Fields
        #endregion

        #region Constructor

        internal BrkrSku()
            : base() {
        }

        #endregion

        #region internal Virtual Methods

        protected override DataSet GetSet() {
            return set;
        }

        protected override string GetTableName() {
            return "AilSku";
        }

        protected override string GetPrimaryKey() {
            return "skuId";
        }

        protected override void ObjASet(DbObject obj, DataRow row) {
            row[GetPrimaryKey()] = ((Sku)obj).Id.ToString();
            row["code"] = ((Sku)obj).Code;
            row["supplierId"] = ((Sku)obj).Supplier.Id;
            row["supplierCalId"] = ((Sku)obj).SupplierCal.Id;
            row["productId"] = ((Sku)obj).Product.Id;
            row["nodeId"] = ((Sku)obj).Node.Id;
            row["rsmpFirstDate"] = ((Sku)obj).RsmpFirstDate;
            row["serviceLevel"] = ((Sku)obj).ServiceLevel;
            row["leadTime"] = ((Sku)obj).LeadTime;
            row["simFcst"] = ((Sku)obj).SimFcst;
            row["replenishmentTime"] = ((Sku)obj).ReplenishmentTime;
            row["lotSize"] = ((Sku)obj).LotSize;
            row["roundingQty"] = ((Sku)obj).RoundingQty;
            row["isPeriodFixed"] = ((Sku)obj).IsPeriodFixed;
            row["planJustCustOrders"] = ((Sku)obj).PlanJustCustOrders;
            row["hasSubSkus"] = ((Sku)obj).HasSubSkus;
            row["obsRisk"] = ((Sku)obj).ObsRisk;
            row["obsExpValue"] = ((Sku)obj).ObsExpValue;
            row["planningRule"] = ((Sku)obj).PlanningRule;
            row["supplierCalId"] = ((Sku)obj).SupplierCal.Id;
            row["policy"] = ((Sku)obj).Policy;
            row["RsmpFilteringProb"] = ((Sku)obj).RsmpFilteringProb;
            row["rsmpNoise"] = ((Sku)obj).RsmpNoise;
            row["rsmpClusterThreshold"] = ((Sku)obj).RsmpClusterThreshold;
            row["stock"] = ((Sku)obj).Stock;
            row["bckSafetyStock"] = ((Sku)obj).BckSafetyStock;
            row["price"] = ((Sku)obj).Price;
            row["ltFcstMinPercOver"] = ((Sku)obj).LtFcstMinPercOver;
            row["orderMinPercOver"] = ((Sku)obj).OrderMinPercOver;
            row["lastNPeriods"] = ((Sku)obj).LastNPeriods;
            row["bomLevel"] = ((Sku)obj).BomLevel;
            row["firstSellingDate"] = ((Sku)obj).FirstSellingDate;
            row["lastSupplyDate"] = ((Sku)obj).LastSupplyDate;
            row["rsmpRollingFcstHist"] = ((Sku)obj).RsmpRollingFcstHist;
            row["leadTimeFcstManual"] = ((Sku)obj).LeadTimeFcstManual;
            row["rsmpRollingFcstManual"] = ((Sku)obj).RsmpRollingFcstManual;
            row["replenishmentFcstManual"] = ((Sku)obj).ReplenishmentFcstManual;
            row["leadTimeFcstOrigin"] = ((Sku)obj).LeadTimeFcstOrigin;
            row["verySMP"] = ((Sku)obj).VerySMP;
            row["minServiceLevel"] = ((Sku)obj).MinServiceLevel;
            row["maxServiceLevel"] = ((Sku)obj).MaxServiceLevel;
            row["cost"] = ((Sku)obj).Cost;
            row["volume"] = ((Sku)obj).Volume;
            row["predictable"] = ((Sku)obj).Predictable;
            row["fcstHorizon"] = ((Sku)obj).FcstHorizon;
            row["planHorizon"] = ((Sku)obj).PlanHorizon;
            row["validHorizon"] = ((Sku)obj).ValidHorizon;
            row["firstHistDate"] = ((Sku)obj).FirstHistDate;
            row["provider"] = GetInt((((Sku)obj).Provider));
            row["state"] = GetInt((((Sku)obj).State));
            row["updated"] = ((Sku)obj).Updated;
            row["created"] = ((Sku)obj).Created;
            for (int i = 1; i <= uFieldCount; i++) { 
                row["uStr" + i] = ((Sku)obj).GetUStr(i);
                //row["uNum" + i] = ((Sku)obj).GetUNum(i);
                //row["uDate" + i] = ((Sku)obj).GetUDate(i);
            }
        }


        protected override void SetAObj(DataRow row, DbObject obj) {
            ((Sku)obj).Id = GetUlong(row[GetPrimaryKey()]);
            ((Sku)obj).Code = row["code"].ToString();
            ulong supplierId = GetUlong(row["supplierId"]);
            ((Sku)obj).Supplier = new Supplier(supplierId);
            ulong productId = GetUlong(row["productId"]);
            ((Sku)obj).Product = new Product(productId);
            ulong nodeId = GetUlong(row["nodeId"]);
            ((Sku)obj).Node = new Node(nodeId);
            ((Sku)obj).RsmpFirstDate = GetDate(row["rsmpFirstDate"]);
            ((Sku)obj).ServiceLevel = GetDouble(row["serviceLevel"]);
            ((Sku)obj).LeadTime = GetInt(row["leadTime"]);
            ((Sku)obj).SimFcst = GetDouble(row["simFcst"]);
            ((Sku)obj).ReplenishmentTime = GetInt(row["replenishmentTime"]);
            ((Sku)obj).LotSize = GetInt(row["lotSize"]);
            ((Sku)obj).RoundingQty = GetInt(row["roundingQty"]);
            ((Sku)obj).IsPeriodFixed = GetBoolean(row["isPeriodFixed"]);
            ((Sku)obj).PlanJustCustOrders = GetBoolean(row["planJustCustOrders"]);
            ((Sku)obj).HasSubSkus = GetBoolean(row["hasSubSkus"]);
            ((Sku)obj).ObsRisk = GetDouble(row["obsRisk"]);
            ((Sku)obj).ObsExpValue = GetDouble(row["obsExpValue"]);
            ((Sku)obj).PlanningRule = GetString(row["planningRule"]);
            ulong supplierCalId = GetUlong(row["supplierCalId"]);
            ((Sku)obj).SupplierCal = new Calendar(supplierCalId);
            ((Sku)obj).RsmpFilteringProb = GetDouble(row["RsmpFilteringProb"]);
            ((Sku)obj).RsmpNoise = GetDouble(row["rsmpNoise"]);
            ((Sku)obj).RsmpClusterThreshold = GetDouble(row["rsmpClusterThreshold"]);
            ((Sku)obj).Stock = GetDouble(row["stock"]);
            ((Sku)obj).BckSafetyStock = GetDouble(row["bckSafetyStock"]);
            ((Sku)obj).Price = GetDouble(row["price"]);
            ((Sku)obj).LtFcstMinPercOver = GetDouble(row["ltFcstMinPercOver"]);
            ((Sku)obj).OrderMinPercOver = GetDouble(row["orderMinPercOver"]);
            ((Sku)obj).LastNPeriods = GetInt(row["lastNPeriods"]);
            ((Sku)obj).BomLevel = GetInt(row["bomLevel"]);
            ((Sku)obj).FirstSellingDate = GetDate(row["firstSellingDate"]);
            ((Sku)obj).LastSupplyDate = GetDate(row["lastSupplyDate"]);
            ((Sku)obj).RsmpRollingFcstHist = GetDouble(row["rsmpRollingFcstHist"]);
            ((Sku)obj).LeadTimeFcstManual = GetDouble(row["leadTimeFcstManual"]);
            ((Sku)obj).RsmpRollingFcstManual = GetDouble(row["rsmpRollingFcstManual"]);
            ((Sku)obj).ReplenishmentFcstManual = GetDouble(row["replenishmentFcstManual"]);
            ((Sku)obj).LeadTimeFcstOrigin = row["leadTimeFcstOrigin"].ToString();
            ((Sku)obj).VerySMP = GetBoolean(row["verySMP"]);
            ((Sku)obj).MinServiceLevel = GetDouble(row["minServiceLevel"]);
            ((Sku)obj).MaxServiceLevel = GetDouble(row["maxServiceLevel"]);
            ((Sku)obj).Cost = GetDouble(row["cost"]);
            ((Sku)obj).Volume = GetDouble(row["volume"]);
            ((Sku)obj).Predictable = GetBoolean(row["predictable"]);
            ((Sku)obj).FcstHorizon = GetInt(row["fcstHorizon"]);
            ((Sku)obj).PlanHorizon = GetInt(row["planHorizon"]);
            ((Sku)obj).ValidHorizon = GetInt(row["validHorizon"]);
            ((Sku)obj).FirstHistDate = GetDate(row["firstHistDate"]);
            Sku.ProviderType provider = Sku.ProviderType.none;
            switch(GetInt(row["provider"])) {
                case 0: provider = Sku.ProviderType.none; break;
                case 1: provider = Sku.ProviderType.rootNode; break;
                case 2: provider = Sku.ProviderType.supplier; break;
                default: throw new Exception("Error. Provider type not known.");
            }
            ((Sku)obj).Provider = provider;
            Sku.StateType state = Sku.StateType.inactive;
            switch (GetInt(row["state"])) {
                case 0: state = Sku.StateType.inactive; break;
                case 1: state = Sku.StateType.desactived; break;
                case 2: state = Sku.StateType.active; break;
                default: throw new Exception("Error. Provider type not known.");
            }
            ((Sku)obj).State = state;
            ((Sku)obj).Updated = GetDate(row["updated"]);
            ((Sku)obj).Created = GetDate(row["created"]);
            for (int i = 1; i <= uFieldCount; i++) { 
                ((Sku)obj).SetUStr(i, GetString(row["uStr" + i]));
                //((Sku)obj).SetUNum(i, GetDouble(row["uNum" + i]));
                //((Sku)obj).SetUDate(i, GetDate(row["uDate" + i]));
            }
        }

        protected override void SetAObjs(DataRowCollection rows, List<DbObject> objs) {
            Sku obj;
            foreach (DataRow row in rows)
            {
                obj = new Sku();
                SetAObj(row, obj);
                objs.Add(obj);
            }
        }
        
        #endregion
        
    }
}
