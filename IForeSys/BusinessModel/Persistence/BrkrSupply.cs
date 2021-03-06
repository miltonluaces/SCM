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

    internal class BrkrSupply : Broker
    {

        #region Fields
        #endregion

        #region Constructor

        internal BrkrSupply()
            : base() {
        }

        #endregion

        #region internal Virtual Methods

        protected override DataSet GetSet() {
            return set;
        }

        protected override string GetTableName()  {
            return "AilSupply";
        }

        protected override string GetPrimaryKey() {
            return "supplyId";
        }

        protected override void ObjASet(DbObject obj, DataRow row) {
            row[GetPrimaryKey()] = ((Supply)obj).Id.ToString();
            row["code"] = ((Supply)obj).Code;
            row["supplierId"] = ((Supply)obj).Supplier.Id;
            row["orderType"] = ((Supply)obj).OrderType;
            row["skuId"] = ((Supply)obj).Sku.Id;
            row["orderId"] = ((Supply)obj).OrderId;
            row["lineId"] = ((Supply)obj).LineId;
            row["initialQty"] = ((Supply)obj).InitialQty;
            row["actualQty"] = ((Supply)obj).ActualQty;
            row["releaseDate"] = ((Supply)obj).ReleaseDate;
            row["receptionDate"] = ((Supply)obj).ReceptionDate;
            row["consumptionDate"] = ((Supply)obj).ConsumptionDate;
            row["backOrderQty"] = ((Supply)obj).BackOrderQty;
            row["bomQty"] = ((Supply)obj).BomQty;
            row["outQty"] = ((Supply)obj).OutQty;
            row["lTFcstQty"] = ((Supply)obj).LTFcstQty;
            row["updated"] = ((Supply)obj).Updated;
            row["created"] = ((Supply)obj).Created;
        }

        protected override void SetAObj(DataRow row, DbObject obj) {
            ((Supply)obj).Id = GetUlong(row[GetPrimaryKey()]);
            ((Supply)obj).Code = row["code"].ToString();
            ulong supplierId = GetUlong(row["supplierId"]);
            ((Supply)obj).Supplier = new Supplier(supplierId);
            ((Supply)obj).OrderType = row["orderType"].ToString();
            ((Supply)obj).ReceptionDate = (DateTime)row["receptionDate"];
            ulong skuId = GetUlong(row["skuId"]);
            ((Supply)obj).Sku = new Sku(skuId);
            ((Supply)obj).OrderId = (int)row["orderId"];
            ((Supply)obj).LineId = (int)row["lineId"];
            ((Supply)obj).InitialQty = Convert.ToDouble(row["initialQty"]);
            ((Supply)obj).ActualQty = Convert.ToDouble(row["actualQty"]);
            ((Supply)obj).BackOrderQty = Convert.ToDouble(row["backOrderQty"]);
            ((Supply)obj).BomQty = Convert.ToDouble(row["bomQty"]);
            ((Supply)obj).OutQty = Convert.ToDouble(row["outQty"]);
            ((Supply)obj).LTFcstQty = Convert.ToDouble(row["lTFcstQty"]);
            ((Supply)obj).OrderQty = Convert.ToDouble(row["orderQty"]);
            ((Supply)obj).Updated = GetDate(row["updated"]);
            ((Supply)obj).Created = GetDate(row["created"]);
        }

        protected override void SetAObjs(DataRowCollection rows, List<DbObject> objs)
        {
            Supply obj;
            foreach (DataRow row in rows)
            {
                obj = new Supply();
                SetAObj(row, obj);
                objs.Add(obj);
            }
        }
     

        #endregion
    }
}
