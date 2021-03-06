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

    internal class BrkrPuOrder : Broker {

        #region Fields
        #endregion

        #region Constructor

        internal BrkrPuOrder()
            : base() {
        }

        #endregion

        #region internal Virtual Methods

        protected override DataSet GetSet() {
            return set;
        }

        protected override string GetTableName() {
            return "AilPuOrder";
        }

        protected override string GetPrimaryKey()  {
            return "puOrderId";
        }


        protected override void ObjASet(DbObject obj, DataRow row) {
            row[GetPrimaryKey()] = ((PuOrder)obj).Id.ToString();
             row["nodeCode"] = ((PuOrder)obj).Code;
             row["productCode"] = ((PuOrder)obj).ProductCode;
             row["barCode"] = ((PuOrder)obj).Barcode;
             row["supplierCode"] = ((PuOrder)obj).SupplierCode;
             row["skuId"] = ((PuOrder)obj).Sku.Id;
             row["subSkuId"] = ((PuOrder)obj).SubSku.Id;
             row["qty"] = ((PuOrder)obj).Qty;
             row["ordDate"] = ((PuOrder)obj).OrdDate;
             row["rcpDate"] = ((PuOrder)obj).RcpDate;
             row["state"] = GetInt(((PuOrder)obj).State);
             row["orderNumber"] = ((PuOrder)obj).OrderNumber;
             row["lineNumber"] = ((PuOrder)obj).LineNumber;
             row["updated"] = ((PuOrder)obj).Updated;
             row["created"] = ((PuOrder)obj).Created;
        }

        protected override void SetAObj(DataRow row, DbObject obj) {
            ((PuOrder)obj).Id = GetUlong(row[GetPrimaryKey()]);
            ((PuOrder)obj).Code = row["nodeCode"].ToString();
            ((PuOrder)obj).ProductCode = row["productCode"].ToString();
            ((PuOrder)obj).Barcode = row["barcode"].ToString();
            ((PuOrder)obj).SupplierCode = row["supplierCode"].ToString();
            ulong skuId = GetUlong(row["skuId"]);
	        ((PuOrder)obj).Sku = new Sku(skuId);
            ulong subSkuId = GetUlong(row["subSkuId"]);
            ((PuOrder)obj).SubSku = new SubSku(subSkuId);
            ((PuOrder)obj).Qty = GetDouble(row["qty"]);
	        ((PuOrder)obj).OrdDate = GetDate(row["ordDate"]);
	        ((PuOrder)obj).RcpDate = GetDate(row["rcpDate"]);
            int stateInt = GetInt(row["state"]);
            switch (stateInt) {
                case 0: ((PuOrder)obj).State = PuOrder.StateType.planned; break;
                case 1: ((PuOrder)obj).State = PuOrder.StateType.confirmed; break;
                case 2: ((PuOrder)obj).State = PuOrder.StateType.transit; break;
            }
            ((PuOrder)obj).OrderNumber = GetInt(row["orderNumber"]);
            ((PuOrder)obj).LineNumber = GetInt(row["lineNumber"]);
            ((PuOrder)obj).Updated = GetDate(row["updated"]);
            ((PuOrder)obj).Created = GetDate(row["created"]);
        }


        protected override void SetAObjs(DataRowCollection rows, List<DbObject> objs) {
            PuOrder obj;
            foreach (DataRow row in rows)  {
                obj = new PuOrder();
                SetAObj(row, obj);
                objs.Add(obj);
            }
        }
        
        #endregion
    }
}
