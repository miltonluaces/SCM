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

    internal class BrkrBinLargeObj : Broker
    {

        #region Fields
        #endregion

        #region Constructor

        internal BrkrBinLargeObj() : base()  {
        }

        #endregion

        #region internal Virtual Methods

        protected override DataSet GetSet()
        {
            return set;
        }

        protected override string GetTableName()
        {
            return "AilBinLargeObj";
        }

        protected override string GetPrimaryKey()
        {
            return "blobId";
        }

        protected override void ObjASet(DbObject obj, DataRow row)
        {
            row[GetPrimaryKey()] = ((BinLargeObj)obj).Id.ToString();
            row["code"] = ((BinLargeObj)obj).Code;
            row["data"] = ((BinLargeObj)obj).GetData();
            row["updated"] = ((BinLargeObj)obj).Updated;
            row["created"] = ((BinLargeObj)obj).Created;
        }

        protected override void SetAObj(DataRow row, DbObject obj)
        {
            ((BinLargeObj)obj).Id = GetUlong(row[GetPrimaryKey()]);
            ((BinLargeObj)obj).Code = row["code"].ToString();
            ((BinLargeObj)obj).AddData(row["data"].ToString());
            ((BinLargeObj)obj).Updated = GetDate(row["updated"]);
            ((BinLargeObj)obj).Created = GetDate(row["created"]);
           
        }

        protected override void SetAObjs(DataRowCollection rows, List<DbObject> objs)
        {
            BinLargeObj obj;
            foreach (DataRow row in rows)
            {
                obj = new BinLargeObj();
                SetAObj(row, obj);
                objs.Add(obj);
            }
        }

        #endregion
    }
}
