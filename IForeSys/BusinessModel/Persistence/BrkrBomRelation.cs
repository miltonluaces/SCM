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

    internal class BrkrBomRelation : Broker {

        #region Fields
        #endregion

        #region Constructor

        internal BrkrBomRelation()
            : base() {
      
        }

        #endregion

        #region internal Virtual Methods

        protected override DataSet GetSet() {
            return set;
        }

        protected override string GetTableName() {
            return "AilBomRelation";
        }

        protected override string GetPrimaryKey() {
            return "bomRelationId";
        }

        protected override void ObjASet(DbObject obj, DataRow row) {
            row[GetPrimaryKey()] = ((BomRelation)obj).Id.ToString();
            row["code"] = ((BomRelation)obj).Code;
            row["originId"] = ((BomRelation)obj).Origin.Id;
            row["targetId"] = ((BomRelation)obj).Target.Id;
            row["qty"] = ((BomRelation)obj).Qty;
            row["offset"] = ((BomRelation)obj).Offset;
            row["updated"] = ((BomRelation)obj).Updated;
            row["created"] = ((BomRelation)obj).Created;
        }

        protected override void SetAObj(DataRow row, DbObject obj)
        {
            ((BomRelation)obj).Id = GetUlong(row[GetPrimaryKey()]);
            ((BomRelation)obj).Code = row["code"].ToString();
            ulong originId = GetUlong(row["originId"]);
            ((BomRelation)obj).Origin = new Node(originId);
            ulong targetId = GetUlong(row["targetId"]);
            ((BomRelation)obj).Target = new Node(targetId);
            ((BomRelation)obj).Qty = Convert.ToDouble(row["qty"]);
            ((BomRelation)obj).Offset = (int)row["offset"];
            ((BomRelation)obj).Updated = GetDate(row["updated"]);
            ((BomRelation)obj).Created = GetDate(row["created"]);
        }

        protected override void SetAObjs(DataRowCollection rows, List<DbObject> objs) {
            BomRelation obj;
            foreach (DataRow row in rows) {
                obj = new BomRelation();
                SetAObj(row, obj);
                objs.Add(obj);
            }
        }
        
        #endregion
    }
}
