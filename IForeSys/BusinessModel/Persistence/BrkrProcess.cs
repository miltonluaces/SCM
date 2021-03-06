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
using System.Globalization;

#endregion

namespace AibuSet {

    internal class BrkrProcess : Broker {

        #region Fields

        #endregion

        #region Constructor

        internal BrkrProcess()
            : base() {
        }

        #endregion

        #region internal Virtual Methods

        protected override DataSet GetSet() {
            return set;
        }

        protected override string GetTableName() {
            return "AilProcess";
        }

        protected override string GetPrimaryKey() {
            return "processId";
        }

        protected override void ObjASet(DbObject obj, DataRow row) {
            row[GetPrimaryKey()] = ((Process)obj).Id.ToString();
            row["code"] = ((Process)obj).Code;
            row["descr"] = ((Process)obj).Descr;
            row["type"] = ((Process)obj).Type.ToString();
            row["lastClosure"] = ((Process)obj).LastClosure;
            row["start"] = ((Process)obj).Start;
            row["finish"] = ((Process)obj).Finish;
            row["state"] = ((Process)obj).State.ToString();
            row["notes"] = ((Process)obj).Notes;
        }

        protected override void SetAObj(DataRow row, DbObject obj) {
            ((Process)obj).Id = GetUlong(row[GetPrimaryKey()]);
            ((Process)obj).Code = row["code"].ToString();
            ((Process)obj).Descr = row["descr"].ToString();
            ((Process)obj).Type = GetType(row["type"].ToString());
            ((Process)obj).LastClosure = Convert.ToDateTime(row["lastClosure"]);
            ((Process)obj).Start = (TimeSpan)row["start"];
            ((Process)obj).Finish = (TimeSpan)row["finish"];
            ((Process)obj).State = GetState(row["state"].ToString());
            ((Process)obj).Notes = row["notes"].ToString();
        }

        protected override void SetAObjs(DataRowCollection rows, List<DbObject> objs) {
            Process obj;
            foreach (DataRow row in rows) {
                obj = new Process();
                SetAObj(row, obj);
                objs.Add(obj);
            }
        }

        #endregion

        #region Private Methods

        private Process.TypeType GetType(string typeStr) {
            Process.TypeType type = Process.TypeType.Interface;
            switch (typeStr) {
                case "Interface": type = Process.TypeType.Interface; break;
                case "TSeriesBuilding": type = Process.TypeType.TSeriesBuilding; break;
                case "Calculation": type = Process.TypeType.Calculation; break;
            }
            return type;
        }

        private Process.StateType GetState(string stateStr) {
            Process.StateType state = Process.StateType.Ok;
            switch (stateStr) {
                case "Pending": state = Process.StateType.Pending; break;
                case "Ok": state = Process.StateType.Ok; break;
                case "Warning": state = Process.StateType.Warning; break;
                case "Error": state = Process.StateType.Error; break;
                default: state = Process.StateType.Pending; break;
            }
            return state;
        }
        
        #endregion

    }
}
