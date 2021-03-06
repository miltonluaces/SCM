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

    internal class BrkrParameter : Broker {

        #region Fields

        #endregion

        #region Constructor

        internal BrkrParameter()
            : base() {
        }

        #endregion

        #region internal Virtual Methods

        protected override DataSet GetSet() {
            return set;
        }

        protected override string GetTableName() {
            return "AilParameter";
        }

        protected override string GetPrimaryKey() {
            return "parameterId";
        }

        protected override void ObjASet(DbObject obj, DataRow row) {
            row[GetPrimaryKey()] = ((Parameter)obj).Id.ToString();
            row["code"] = ((Parameter)obj).Code;
            row["descr"] = ((Parameter)obj).Descr;
            row["type"] = Parameter.GetParTypeStr(((Parameter)obj).Type);
            row["value"] = ((Parameter)obj).Value.ToString();
            row["updated"] = ((Parameter)obj).Updated;
            row["created"] = ((Parameter)obj).Created;
        }

        protected override void SetAObj(DataRow row, DbObject obj) {
            ((Parameter)obj).Id = GetUlong(row[GetPrimaryKey()]);
            ((Parameter)obj).Code = row["code"].ToString();
            ((Parameter)obj).Descr = row["descr"].ToString();
            ((Parameter)obj).Type = Parameter.GetParType(row["type"].ToString());
            ((Parameter)obj).Value = Parameter.GetParValue(((Parameter)obj).Type, row["value"].ToString());
            ((Parameter)obj).Updated = GetDate(row["updated"]);
            ((Parameter)obj).Created = GetDate(row["created"]);
        }

        protected override void SetAObjs(DataRowCollection rows, List<DbObject> objs) {
            Parameter obj;
            foreach (DataRow row in rows) {
                obj = new Parameter();
                SetAObj(row, obj);
                objs.Add(obj);
            }
        }

        #endregion

    }
}
