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

    internal class BrkrFcstOverride : Broker {

        #region Fields

        #endregion

        #region Constructor

        internal BrkrFcstOverride()
            : base() {
        }

        #endregion

        #region internal Virtual Methods

        protected override DataSet GetSet() {
            return set;
        }

        protected override string GetTableName()  {
            return "AilFcstOverride";
        }

        protected override string GetPrimaryKey() {
            return "fcstOverrideId";
        }

        protected override void ObjASet(DbObject obj, DataRow row) {
            row[GetPrimaryKey()] = ((FcstOverride)obj).Id.ToString();
            row["code"] = ((FcstOverride)obj).Code;
            row["updated"] = ((FcstOverride)obj).Updated;
            row["created"] = ((FcstOverride)obj).Created;
        }

        protected override void SetAObj(DataRow row, DbObject obj) {
            ((FcstOverride)obj).Id = GetUlong(row[GetPrimaryKey()]);
            ((FcstOverride)obj).Code = row["code"].ToString();
            ((FcstOverride)obj).Updated = GetDate(row["updated"]);
            ((FcstOverride)obj).Created = GetDate(row["created"]);
        }

        protected override void SetAObjs(DataRowCollection rows, List<DbObject> objs)
        {
            FcstOverride obj;
            foreach (DataRow row in rows)
            {
                obj = new FcstOverride();
                SetAObj(row, obj);
                objs.Add(obj);
            }
        }
        
        #endregion
    }
}
