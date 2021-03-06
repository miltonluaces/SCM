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
using Maths;

#endregion

namespace AibuSet
{

    internal class BrkrFcstModel : Broker  {

        #region Fields
        #endregion

        #region Constructor

        internal BrkrFcstModel() : base()  {
        }

        #endregion

        #region internal Virtual Methods

        protected override DataSet GetSet()   {
            return set;
        }

        protected override string GetTableName()  {
            return "AilFcstModel";
        }

        protected override string GetPrimaryKey()  {
            return "fcstModelId";
        }

        protected override void ObjASet(DbObject obj, DataRow row)  {
            row[GetPrimaryKey()] = ((FcstModel)obj).Id.ToString();
            row["code"] = ((FcstModel)obj).Code;
            row["skuId"] = ((FcstModel)obj).Sku.Id;
            row["lastUpdate"] = ((FcstModel)obj).LastUpdate.ToString();
            row["method"] = (int)((FcstModel)obj).FcstMethod;
            row["model"] = ((FcstModel)obj).ModelStr;
            row["updated"] = ((FcstModel)obj).Updated;
            row["created"] = ((FcstModel)obj).Created;
        }

        protected override void SetAObj(DataRow row, DbObject obj) {
            ((FcstModel)obj).Id = GetUlong(row[GetPrimaryKey()]);
            ((FcstModel)obj).Code= GetString(row["code"]);
            Sku sku = new Sku(GetUlong(row["skuId"]));
            ((FcstModel)obj).Sku = sku;
            ((FcstModel)obj).LastUpdate = Convert.ToDateTime(row["lastUpdate"]);
            ((FcstModel)obj).FcstMethod = GetFcstMethod((int)row["method"]);
            ((FcstModel)obj).ModelStr = row["model"].ToString();
            ((FcstModel)obj).Updated = GetDate(row["updated"]);
            ((FcstModel)obj).Created = GetDate(row["created"]);
        }

        protected override void SetAObjs(DataRowCollection rows, List<DbObject> objs)  {
            FcstModel obj;
            foreach (DataRow row in rows) {
                obj = new FcstModel();
                SetAObj(row, obj);
                objs.Add(obj);
            }
        }

        private FcstMethodType GetFcstMethod(int value) {
            switch (value) { 
                case 0: return FcstMethodType.Naive;
                case 1: return FcstMethodType.Regression;
                case 2: return FcstMethodType.ZChart;
                case 3: return FcstMethodType.HoltWinters;
                case 4: return FcstMethodType.ARIMA;
                case 5: return FcstMethodType.DLMKalman;
                case 6: return FcstMethodType.NeuNet;
                default: return FcstMethodType.Naive;
            }
        }

        #endregion
    }
}
