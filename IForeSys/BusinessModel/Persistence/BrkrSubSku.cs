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
using Maths;

#endregion

namespace AibuSet {

    internal class BrkrSubSku : Broker  {

        #region Fields
        #endregion

        #region Constructor

        internal BrkrSubSku() : base() {
        }

        #endregion

        #region internal Virtual Methods

        protected override DataSet GetSet() {
            return set;
        }

        protected override string GetTableName()  {
            return "AilSubSku";
        }

        protected override string GetPrimaryKey() {
            return "subSkuId";
        }

        protected override void ObjASet(DbObject obj, DataRow row)  {
            row[GetPrimaryKey()] = ((SubSku)obj).Id.ToString();
            row["code"] = ((SubSku)obj).Code;
            row["skuId"] = ((SubSku)obj).Sku.Id;
            row["barcode"] = ((SubSku)obj).Barcode;
            row["category"] = ((SubSku)obj).Category;
            row["freq"] = (int)((SubSku)obj).Freq;
            row["updated"] = ((SubSku)obj).Updated;
            row["created"] = ((SubSku)obj).Created;
        }

        protected override void SetAObj(DataRow row, DbObject obj) {
            ((SubSku)obj).Id = GetUlong(row[GetPrimaryKey()]);
            ((SubSku)obj).Code = GetString(row["code"]);
            Sku sku = new Sku(GetUlong(row["skuId"]));
            ((SubSku)obj).Sku = sku;
            ((SubSku)obj).Barcode = row["barcode"].ToString();
            ((SubSku)obj).Category = row["category"].ToString();
            ((SubSku)obj).Freq = GetDouble(row["freq"]);
            ((SubSku)obj).Updated = GetDate(row["updated"]);
            ((SubSku)obj).Created = GetDate(row["created"]);
        }

        protected override void SetAObjs(DataRowCollection rows, List<DbObject> objs) {
            SubSku obj;
            foreach (DataRow row in rows) {
                obj = new SubSku();
                SetAObj(row, obj);
                objs.Add(obj);
            }
        }

        #endregion
    }
}
