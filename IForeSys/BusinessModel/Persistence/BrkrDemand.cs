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

namespace AibuSet {

    internal class BrkrDemand : Broker {

        #region Fields
        #endregion

        #region Constructor

        internal BrkrDemand()
            : base() {
        }

        #endregion

        #region internal Virtual Methods

        protected override DataSet GetSet() {
            return set;
        }

        protected override string GetTableName()  {
            return "AilDemand";
        }

        protected override string GetPrimaryKey() {
            return "demandId";
        }

        protected override void ObjASet(DbObject obj, DataRow row) {
            row[GetPrimaryKey()] = ((Demand)obj).Id.ToString();
        	row["code"] = ((Demand)obj).Code;
		    row["skuId"] = ((Demand)obj).Sku.Id;
            row["subSkuId"] = ((Demand)obj).SubSku.Id;
            row["orderId"] = ((Demand)obj).OrderId;
            row["lineId"] = ((Demand)obj).LineId;
            row["initialQty"] = ((Demand)obj).InitialQty;
            row["actualQty"] = ((Demand)obj).ActualQty;
            row["desiredDate"] = ((Demand)obj).DesiredDate;
		    row["orderPrice"] = ((Demand)obj).OrderPrice;
		    row["customerId"] = ((Demand)obj).Customer.Id;
            row["updated"] = ((Demand)obj).Updated;
            row["created"] = ((Demand)obj).Created;
        }

        protected override void SetAObj(DataRow row, DbObject obj) {
            ((Demand)obj).Id = GetUlong(row[GetPrimaryKey()]);
            ((Demand)obj).Code = row["code"].ToString();
 		    Sku sku = new Sku(GetUlong(row["skuId"]));
		    ((Demand)obj).Sku = sku;
            SubSku subSku = new SubSku(GetUlong(row["subSkuId"]));
            ((Demand)obj).SubSku = subSku;
            ((Demand)obj).OrderId = (int)row["orderId"];
            ((Demand)obj).LineId = (int)row["lineId"];
            ((Demand)obj).InitialQty = Convert.ToDouble(row["initialQty"]);
            ((Demand)obj).ActualQty = Convert.ToDouble(row["actualQty"]);
            ((Demand)obj).DesiredDate = (DateTime)row["desiredDate"];
 		    ((Demand)obj).OrderPrice = Convert.ToDouble(row["orderPrice"]);
 		    ulong customerId = GetUlong(row["customerId"]);
		    ((Demand)obj).Customer = new Customer(customerId);
            ((Demand)obj).Updated = GetDate(row["updated"]);
            ((Demand)obj).Created = GetDate(row["created"]);
        }

        protected override void SetAObjs(DataRowCollection rows, List<DbObject> objs) {
            Demand obj;
            foreach (DataRow row in rows)
            {
                obj = new Demand();
                SetAObj(row, obj);
                objs.Add(obj);
            }
        }
        
        #endregion
    }
}
