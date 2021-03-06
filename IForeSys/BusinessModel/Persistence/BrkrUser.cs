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

    internal class BrkrUser : Broker {

        #region Fields
        #endregion

        #region Constructor

        internal BrkrUser()
            : base() {
        }

        #endregion

        #region internal Virtual Methods

        protected override DataSet GetSet()
        {
            return set;
        }

        protected override string GetTableName()
        {
            return "AilUser";
        }

        protected override string GetPrimaryKey()
        {
            return "userId";
        }

        protected override void ObjASet(DbObject obj, DataRow row)
        {
            row[GetPrimaryKey()] = ((User)obj).Id;
            row["code"] = ((User)obj).Code;
            row["name"] = ((User)obj).Name;
            row["password"] = ((User)obj).Password;
            //row["accesses"] = ((User)obj).Accesses;
            row["updated"] = ((User)obj).Updated;
            row["created"] = ((User)obj).Created;
        }

        protected override void SetAObj(DataRow row, DbObject obj)
        {
            ((User)obj).Id = GetUlong(row[GetPrimaryKey()]);
            ((User)obj).Code = row["code"].ToString();
            ((User)obj).Name = row["name"].ToString();
            ((User)obj).Password = row["password"].ToString();
            //((User)obj).Accesses = row["accesses"].ToString();
            ((User)obj).Updated = GetDate(row["updated"]);
            ((User)obj).Created = GetDate(row["created"]);
        }

        protected override void SetAObjs(DataRowCollection rows, List<DbObject> objs)
        {
            User obj;
            foreach (DataRow row in rows)
            {
                obj = new User();
                SetAObj(row, obj);
                objs.Add(obj);
            }
        }

        #endregion
    }
}
