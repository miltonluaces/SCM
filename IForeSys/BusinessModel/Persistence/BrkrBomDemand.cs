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

    internal class BrkrBomDemand : Broker {

        #region Fields
        #endregion

        #region Constructor

        internal BrkrBomDemand()
            : base() {
        }

        #endregion

        #region internal Virtual Methods

        protected override DataSet GetSet() {
            return set;
        }

        protected override string GetTableName() {
            return "AilBomDemand";
        }

        protected override string GetPrimaryKey() {
            return "bomDemandId";
        }

        protected override void ObjASet(DbObject obj, DataRow row) {
            row[GetPrimaryKey()] = ((BomDemand)obj).Id.ToString();
            row["code"] = ((BomDemand)obj).Code;
            row["skuId"] = ((BomDemand)obj).Sku.Id;
            row["orderId"] = ((BomDemand)obj).OrderId;
            row["lineId"] = ((BomDemand)obj).LineId;
            row["initialQty"] = ((BomDemand)obj).InitialQty;
            row["actualQty"] = ((BomDemand)obj).ActualQty;
            row["desiredDate"] = ((BomDemand)obj).DesiredDate;
            row["orderPrice"] = ((BomDemand)obj).OrderPrice;
            row["customerId"] = ((BomDemand)obj).Customer.Id;
            row["bomRelationId"] = ((BomDemand)obj).BomRel.Id;
            row["updated"] = ((BomDemand)obj).Updated;
            row["created"] = ((BomDemand)obj).Created;
        }

        protected override void SetAObj(DataRow row, DbObject obj) {
            ((BomDemand)obj).Id = GetUlong(row[GetPrimaryKey()]);
            ((BomDemand)obj).Code = row["code"].ToString();
            Sku sku = new Sku(GetUlong(row["skuId"]));
            ((BomDemand)obj).Sku = sku;
            ((BomDemand)obj).OrderId = (int)row["orderId"];
            ((BomDemand)obj).LineId = (int)row["lineId"];
            ((BomDemand)obj).InitialQty = Convert.ToDouble(row["initialQty"]);
            ((BomDemand)obj).ActualQty = Convert.ToDouble(row["actualQty"]);
            ((BomDemand)obj).DesiredDate = (DateTime)row["desiredDate"];
            ((BomDemand)obj).OrderPrice = Convert.ToDouble(row["orderPrice"]);
            ulong customerId = GetUlong(row["customerId"]);
            ((BomDemand)obj).Customer = new Customer(customerId);
            ulong bomRelId = GetUlong(row["bomRelationId"]);
            ((BomDemand)obj).BomRel = new BomRelation(bomRelId);
            ((BomDemand)obj).Created = ((DateTime)row["creation"]).Date;
            ((BomDemand)obj).Updated = GetDate(row["updated"]);
            ((BomDemand)obj).Created = GetDate(row["created"]);
        }

        protected override void SetAObjs(DataRowCollection rows, List<DbObject> objs) {
            BomDemand obj;
            foreach (DataRow row in rows) {
                obj = new BomDemand();
                SetAObj(row, obj);
                objs.Add(obj);
            }
        }
        
        #endregion
    }
}
