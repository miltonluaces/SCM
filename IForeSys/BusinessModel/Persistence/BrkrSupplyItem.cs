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

namespace BusinessModel
{

    public class BrkrSupplyItem : Broker  {

        #region Fields
        #endregion

        #region Constructor

       public BrkrSupplyItem() : base()  {
       }

        #endregion

        #region Public Virtual Methods

        public override DataSet GetSet() {
            return set;
        }

        protected override string GetTableName() {
            return "AilSupplyItem";
        }

        protected override string GetPrimaryKey() {
            return "supplyItemId";
        }
        
        public override void ObjASet(DbObject obj, DataRow row) {
            row[GetPrimaryKey()] = ((SupplyItem)obj).Id.ToString();
            row["code"] = ((SupplyItem)obj).Code;
            row["supplyId"] = ((SupplyItem)obj).Supply.Id;
            row["demandId"] = ((SupplyItem)obj).Demand.Id;
            row["Qty"] = ((SupplyItem)obj).Qty;
            row["creation"] = (DateTime.Now).Date;
        }

        public override void SetAObj(DataRow row, DbObject obj) {
            ((SupplyItem)obj).Id = GetUlong(row[GetPrimaryKey()]);
            ((SupplyItem)obj).Code = row["code"].ToString();
            ulong supplyId = GetUlong(row["supplyId"]);
            ((SupplyItem)obj).Supply = new Supply(supplyId);
            ulong demandId = GetUlong(row["demandId"]);
            ((SupplyItem)obj).Demand = new Demand(demandId);
            ((SupplyItem)obj).Qty = Convert.ToDouble(row["Qty"]);
            ((SupplyItem)obj).Creation = ((DateTime)row["creation"]).Date;
        }

        public override void SetAObjs(DataRowCollection rows, List<DbObject> objs)
        {
            SupplyItem obj;
            foreach (DataRow row in rows)
            {
                obj = new SupplyItem();
                SetAObj(row, obj);
                objs.Add(obj);
            }
        }
        
        #endregion
    }
}
