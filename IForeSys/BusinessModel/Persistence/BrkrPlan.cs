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

    internal class BrkrPlan : Broker {

        #region Fields
        #endregion

        #region Constructor

        internal BrkrPlan()
            : base() {
       }

        #endregion

        #region internal Virtual Methods

        protected override DataSet GetSet() {
            return set;
        }

        protected override string GetTableName()  {
            return "AilPlan";
        }

        protected override string GetPrimaryKey() {
            return "planId";
        }

        protected override void ObjASet(DbObject obj, DataRow row) {
            row[GetPrimaryKey()] = ((Plan)obj).Id.ToString();
            row["skuId"] = ((Plan)obj).Sku.Id;
            row["firstDate"] = ((Plan)obj).FirstDate.ToString();
            row["horizon"] = ((Plan)obj).PlanHorizon;
            row["isRoot"] = ((Plan)obj).IsRoot;
            row["periods"] = ((Plan)obj).GetString(((Plan)obj).Periods);
            row["updated"] = ((Plan)obj).Updated;
            row["created"] = ((Plan)obj).Created;
        }

        protected override void SetAObj(DataRow row, DbObject obj) {
            ((Plan)obj).Id = GetUlong(row[GetPrimaryKey()]);
            Sku sku = new Sku(GetUlong(row["skuId"]));
            ((Plan)obj).Sku = sku;
            ((Plan)obj).FirstDate = Convert.ToDateTime(row["firstDate"]);
            ((Plan)obj).PlanHorizon = (int)row["horizon"];
            ((Plan)obj).IsRoot = Convert.ToBoolean(row["isRoot"]);
            ((Plan)obj).Periods = ((Plan)obj).GetValues(row["periods"].ToString());
            ((Plan)obj).Updated = GetDate(row["updated"]);
            ((Plan)obj).Created = GetDate(row["created"]);
        }

        protected override void SetAObjs(DataRowCollection rows, List<DbObject> objs) {
            Plan obj;
            foreach (DataRow row in rows) {
                obj = new Plan();
                SetAObj(row, obj);
                objs.Add(obj);
            }
        }
        
        #endregion
    }
}
