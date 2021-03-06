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

namespace BusinessModel {

    public class BrkrSimFcst : Broker  {

        #region Fields
        #endregion

        #region Constructor

        public BrkrSimFcst() : base()  {
        }

        #endregion

        #region Public Virtual Methods

        public override DataSet GetSet() {
            return set;
        }

        protected override string GetTableName()  {
            return "SimFcst";
        }

        protected override string GetPrimaryKey()  {
            return "simFcstId";
        }
        

        public override void ObjASet(DbObject obj, DataRow row)  {
            row[GetPrimaryKey()] = ((SimFcst)obj).Id.ToString();
            row["code"] = ((SimFcst)obj).Code;
            row["creation"] = (DateTime.Now).Date;
        }

        public override void SetAObj(DataRow row, DbObject obj)  {
            ((SimFcst)obj).Id = (int)row[GetPrimaryKey()];
            ((SimFcst)obj).Code = row["code"].ToString();
            ((SimFcst)obj).Creation = ((DateTime)row["creation"]).Date;
        }

        public override void SetAObjs(DataRowCollection rows, List<DbObject> objs)
        {
            SimFcst obj;
            foreach (DataRow row in rows)
            {
                obj = new SimFcst();
                SetAObj(row, obj);
                objs.Add(obj);
            }
        }
        
        #endregion
    }
}
