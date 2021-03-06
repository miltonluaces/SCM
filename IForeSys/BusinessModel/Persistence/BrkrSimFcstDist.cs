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

    internal class BrkrSimFcstDist : Broker {

        #region Fields
        #endregion

        #region Constructor

        internal BrkrSimFcstDist()
            : base() {
        }

        #endregion

        #region internal Virtual Methods

        protected override DataSet GetSet() {
            return set;
        }

        protected override string GetTableName()  {
            return "AilSimFcstDist";
        }

        protected override string GetPrimaryKey()  {
            return "simFcstDistId";
        }


        protected override void ObjASet(DbObject obj, DataRow row) {
            row[GetPrimaryKey()] = ((SimFcstDist)obj).Id.ToString();
            row["code"] = ((SimFcstDist)obj).Code;
            row["skuId"] = ((SimFcstDist)obj).Sku.Id;
            row["lower"] = ((SimFcstDist)obj).Lower;
            row["step"] = ((SimFcstDist)obj).Step;
            row["percentiles"] = ((SimFcstDist)obj).GetString();
            row["updated"] = ((SimFcstDist)obj).Updated;
            row["created"] = ((SimFcstDist)obj).Created;
        }

        protected override void SetAObj(DataRow row, DbObject obj) {
            ((SimFcstDist)obj).Id = GetUlong(row[GetPrimaryKey()]);
            ((SimFcstDist)obj).Code = row["code"].ToString();
            ulong skuId = GetUlong(row["skuId"]);
            ((SimFcstDist)obj).Sku = new Sku(skuId);
            ((SimFcstDist)obj).Lower = Convert.ToDouble(row["lower"]);
            ((SimFcstDist)obj).Step = Convert.ToDouble(row["step"]);
            string percentilesStr = row["percentiles"].ToString();
            ((SimFcstDist)obj).Percentiles = ((SimFcstDist)obj).GetValues(percentilesStr);
            ((SimFcstDist)obj).Updated = GetDate(row["updated"]);
            ((SimFcstDist)obj).Created = GetDate(row["created"]);
        }

        protected override void SetAObjs(DataRowCollection rows, List<DbObject> objs) {
            SimFcstDist obj;
            foreach (DataRow row in rows)
            {
                obj = new SimFcstDist();
                SetAObj(row, obj);
                objs.Add(obj);
            }
        }
        
        #endregion


    }
}
