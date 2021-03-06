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

    internal class BrkrTrOrder : Broker {

        #region Fields
        #endregion

        #region Constructor

        internal BrkrTrOrder()
            : base() {
        }

        #endregion

        #region internal Virtual Methods

        protected override DataSet GetSet() {
            return set;
        }

        protected override string GetTableName() {
            return "AilTrOrder";
        }

        protected override string GetPrimaryKey() {
            return "trOrderId";
        }


        protected override void ObjASet(DbObject obj, DataRow row) {
            row[GetPrimaryKey()] = ((TrOrder)obj).Id.ToString();
            row["code"] = ((TrOrder)obj).Code;
            row["barcode"] = ((TrOrder)obj).Code;
            row["supSkuId"] = ((TrOrder)obj).SupSku.Id;
            row["conSkuId"] = ((TrOrder)obj).ConSku.Id;
            row["qty"] = ((TrOrder)obj).Qty;
            row["allocQty"] = ((TrOrder)obj).AllocQty;
            row["ordDate"] = ((TrOrder)obj).OrdDate;
            row["rcpDate"] = ((TrOrder)obj).RcpDate;
            row["status"] = ((TrOrder)obj).State;
            row["updated"] = ((TrOrder)obj).Updated;
            row["created"] = ((TrOrder)obj).Created;
        }

        protected override void SetAObj(DataRow row, DbObject obj) {
            ((TrOrder)obj).Id = GetUlong(row[GetPrimaryKey()]);
            ((TrOrder)obj).Code = row["code"].ToString();
            ((TrOrder)obj).Barcode = row["barcode"].ToString();
            ulong supSkuId = GetUlong(row["supSkuId"]);
            ((TrOrder)obj).SupSku = new Sku(supSkuId);
            ulong conSkuId = GetUlong(row["conSkuId"]);
            ((TrOrder)obj).ConSku = new Sku(conSkuId);
            ((TrOrder)obj).Qty = GetDouble(row["reqQty"]);
            ((TrOrder)obj).AllocQty = GetDouble(row["AllocQty"]);
            ((TrOrder)obj).OrdDate = GetDate(row["ordDate"]);
            ((TrOrder)obj).RcpDate = GetDate(row["rcpDate"]);
            int stateInt = GetInt(row["state"]);
            switch (stateInt) {
                case 0: ((TrOrder)obj).State = TrOrder.StateType.planned; break;
                case 1: ((TrOrder)obj).State = TrOrder.StateType.confirmed; break;
                case 2: ((TrOrder)obj).State = TrOrder.StateType.transit; break;
            }
            ((TrOrder)obj).Updated = GetDate(row["updated"]);
            ((TrOrder)obj).Created = GetDate(row["created"]);
        }


        protected override void SetAObjs(DataRowCollection rows, List<DbObject> objs) {
            TrOrder obj;
            foreach (DataRow row in rows) {
                obj = new TrOrder();
                SetAObj(row, obj);
                objs.Add(obj);
            }
        }

        #endregion
    }
}
