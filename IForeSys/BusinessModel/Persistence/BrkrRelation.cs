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

    internal class BrkrRelation : Broker
    {

        #region Fields
        #endregion

        #region Constructor

        internal BrkrRelation()
            : base() {
        }

        #endregion

        #region internal Virtual Methods

        protected override DataSet GetSet() {
            return set;
        }

        protected override string GetTableName() {
            return "AilRelation";
        }

        protected override string GetPrimaryKey() {
            return "relationId";
        }

        protected override void ObjASet(DbObject obj, DataRow row) {
            row[GetPrimaryKey()] = ((Relation)obj).Id.ToString();
            row["code"] = ((Relation)obj).Code;
            row["originId"] = ((Relation)obj).Origin.Id;
            row["targetId"] = ((Relation)obj).Target.Id;
            row["updated"] = ((Relation)obj).Updated;
            row["created"] = ((Relation)obj).Created;
        }


        protected override void SetAObj(DataRow row, DbObject obj) {
            ((Relation)obj).Id = GetUlong(row[GetPrimaryKey()]);
            ((Relation)obj).Code = row["code"].ToString();
            ulong originId = GetUlong(row["originId"]);
            ((Relation)obj).Origin = new Node(originId);
            ulong targetId = GetUlong(row["targetId"]);
            ((Relation)obj).Target = new Node(targetId);
            ((Relation)obj).Updated = GetDate(row["updated"]);
            ((Relation)obj).Created = GetDate(row["created"]);
        }

        protected override void SetAObjs(DataRowCollection rows, List<DbObject> objs)
        {
            Relation obj;
            foreach (DataRow row in rows)
            {
                obj = new Relation();
                SetAObj(row, obj);
                objs.Add(obj);
            }
        }
        
        #endregion
    }
}
