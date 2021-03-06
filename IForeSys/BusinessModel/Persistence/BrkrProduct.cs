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

    internal class BrkrProduct : Broker {

        #region Fields
        #endregion

        #region Constructor

        internal BrkrProduct()
            : base() {
        }

        #endregion

        #region internal Virtual Methods

        protected override DataSet GetSet() {
            return set;
        }

        protected override string GetTableName() {
            return "AilProduct";
        }

        protected override string GetPrimaryKey()  {
            return "productId";
        }

        protected override void ObjASet(DbObject obj, DataRow row) {
            row[GetPrimaryKey()] = ((Product)obj).Id;
            row["code"]  = ((Product)obj).Code;
            row["barcode"] = ((Product)obj).Barcode;
            row["descr"] = ((Product)obj).Desc;
            row["cost"]  = ((Product)obj).Cost;
            row["price"] = ((Product)obj).Price;
            row["creation"] = DateTime.Now;
            row["supplierId"] = ((Product)obj).Supplier.Id;
            row["supplierCode"] = ((Product)obj).SupplierCode;
            row["unit"] = ((Product)obj).Unit;
            row["category"] = ((Product)obj).Category;
            row["groupp"] = ((Product)obj).Group;
            row["division"] = ((Product)obj).Division;
            row["uStr1"] = ((Product)obj).UStr1;
            row["uStr2"] = ((Product)obj).UStr2;
            row["updated"] = ((Product)obj).Updated;
            row["created"] = ((Product)obj).Created;
        }

        protected override void SetAObj(DataRow row, DbObject obj) {
            ((Product)obj).Id = GetUlong(row[GetPrimaryKey()]);
            ((Product)obj).Code = row["code"].ToString();
            ((Product)obj).Barcode = row["barcode"].ToString();
            ((Product)obj).Desc = row["descr"].ToString();
            ((Product)obj).Cost = Convert.ToDouble(row["cost"]);
            ((Product)obj).Price = Convert.ToDouble(row["price"]);
            ((Product)obj).Created = ((DateTime)row["creation"]).Date;
            ulong supplierId = GetUlong(row["supplierId"]);
            ((Product)obj).Supplier = new Supplier(supplierId);
            ((Product)obj).SupplierCode = row["supplierCode"].ToString();
            ((Product)obj).Unit = row["unit"].ToString();
            ((Product)obj).Category = row["category"].ToString();
            ((Product)obj).Group = row["groupp"].ToString();
            ((Product)obj).Division = row["division"].ToString();
            ((Product)obj).UStr1 = row["uStr1"].ToString();
            ((Product)obj).UStr2 = row["uStr2"].ToString();
            ((Product)obj).Updated = GetDate(row["updated"]);
            ((Product)obj).Created = GetDate(row["created"]);
        }

        protected override void SetAObjs(DataRowCollection rows, List<DbObject> objs)
        {
            Product obj;
            foreach (DataRow row in rows)
            {
                obj = new Product();
                SetAObj(row, obj);
                objs.Add(obj);
            }
        }
     
        #endregion
 
    }
}
