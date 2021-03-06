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

    internal class BrkrCalendar : Broker {

        #region Fields

        #endregion

        #region Constructor

        internal BrkrCalendar()
            : base() {
        }

        #endregion

        #region internal Virtual Methods

        protected override DataSet GetSet() {
            return set;
        }

        protected override string GetTableName() {
            return "AilCalendar";
        }

        protected override string GetPrimaryKey() {
            return "calendarId";
        }

        protected override void ObjASet(DbObject obj, DataRow row) {
            row[GetPrimaryKey()] = ((Calendar)obj).Id.ToString();
            row["code"] = ((Calendar)obj).Code;
            row["firstDate"] = ((Calendar)obj).FirstDate;
            row["lastDate"] = ((Calendar)obj).LastDate;
            row["weekHolsMon"] = ((Calendar)obj).WeekHols[0];
            row["weekHolsTue"] = ((Calendar)obj).WeekHols[1];
            row["weekHolsWed"] = ((Calendar)obj).WeekHols[2];
            row["weekHolsThu"] = ((Calendar)obj).WeekHols[3];
            row["weekHolsFri"] = ((Calendar)obj).WeekHols[4];
            row["weekHolsSat"] = ((Calendar)obj).WeekHols[5];
            row["weekHolsSun"] = ((Calendar)obj).WeekHols[6];
            row["updated"] = ((Calendar)obj).Updated;
            row["created"] = ((Calendar)obj).Created;
        }

        protected override void SetAObj(DataRow row, DbObject obj) {
            ((Calendar)obj).Id = GetUlong(row[GetPrimaryKey()]);
            ((Calendar)obj).Code = row["code"].ToString();
            ((Calendar)obj).FirstDate = (DateTime)row["firstDate"];
            ((Calendar)obj).LastDate = (DateTime)row["lastDate"];
            ((Calendar)obj).WeekHols[0] = Convert.ToBoolean(row["weekHolsMon"]);
            ((Calendar)obj).WeekHols[1] = Convert.ToBoolean(row["weekHolsTue"]);
            ((Calendar)obj).WeekHols[2] = Convert.ToBoolean(row["weekHolsWed"]);
            ((Calendar)obj).WeekHols[3] = Convert.ToBoolean(row["weekHolsThu"]);
            ((Calendar)obj).WeekHols[4] = Convert.ToBoolean(row["weekHolsFri"]);
            ((Calendar)obj).WeekHols[5] = Convert.ToBoolean(row["weekHolsSat"]);
            ((Calendar)obj).WeekHols[6] = Convert.ToBoolean(row["weekHolsSun"]);
            ((Calendar)obj).Updated = GetDate(row["updated"]);
            ((Calendar)obj).Created = GetDate(row["created"]);
        }

        protected override void SetAObjs(DataRowCollection rows, List<DbObject> objs) {
            Calendar obj;
            foreach (DataRow row in rows) {
                obj = new Calendar();
                SetAObj(row, obj);
                objs.Add(obj);
            }
        }
     
        #endregion
    }
}
