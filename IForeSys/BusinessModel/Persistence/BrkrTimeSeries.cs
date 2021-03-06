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
using System.Runtime.Serialization;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Text;

#endregion

namespace AibuSet {

    internal class BrkrTimeSeries : Broker  {

        #region Fields

        private char separator;
        private string separatorStr;
          
        #endregion

        #region Constructor

        internal BrkrTimeSeries()  : base() {
            separator = ' ';
            separatorStr = " ";
        }

        #endregion

        #region internal Virtual Methods

        protected override DataSet GetSet() {
            return set;
        }

        protected override string GetTableName()  {
            return "AilTimeSeries";
        }

        protected override string GetPrimaryKey() {
            return "timeSeriesId";
        }

        protected override void ObjASet(DbObject obj, DataRow row)
        {
            row[GetPrimaryKey()] = ((TimeSeries)obj).Id.ToString();
            row["code"] = ((TimeSeries)obj).Code;
            row["skuId"] = ((TimeSeries)obj).Sku.Id;
            row["leadtime"] = ((TimeSeries)obj).Leadtime;
            row["firstDate"] = ((TimeSeries)obj).FirstDate;
            row["lastDate"] = ((TimeSeries)obj).LastDate;
            row["lastLdtDate"] = ((TimeSeries)obj).LastDate;
            row["dayHist"] =  GetString(((TimeSeries)obj).DayHist);
            row["dayStock"] =  GetString(((TimeSeries)obj).DayStock);
            row["dayUnDmd"] =  GetString(((TimeSeries)obj).DayUnsatDmd);
            row["ldtHist"] = GetString(((TimeSeries)obj).LdtHist);
            row["ldtFcst"] =  GetString(((TimeSeries)obj).LdtFcst);
            row["ldtOver"] =  GetString(((TimeSeries)obj).LdtOver);
            row["uStr1"] = ((TimeSeries)obj).UStr1;
            row["uStr2"] = ((TimeSeries)obj).UStr2;
            row["updated"] = ((TimeSeries)obj).Updated;
            row["created"] = ((TimeSeries)obj).Created;
        }
        
        private string GetAscii(string unicodeStr) {
            byte[] uni = Encoding.Unicode.GetBytes(unicodeStr);
            return Encoding.ASCII.GetString(uni);
        }

        protected override void SetAObj(DataRow row, DbObject obj) {
            ((TimeSeries)obj).Id = GetUlong(row[GetPrimaryKey()]);
            ((TimeSeries)obj).Code = row["code"].ToString();
            ulong skuId = GetUlong(row["skuId"]);
            ((TimeSeries)obj).Sku = new Sku(skuId);
            ((TimeSeries)obj).Leadtime = GetInt(row["leadtime"]);
            ((TimeSeries)obj).FirstDate = GetDate(row["firstDate"]);
            ((TimeSeries)obj).LastDate = GetDate(row["lastDate"]);
            ((TimeSeries)obj).LastLdtDate = GetDate(row["lastLdtDate"]);
            ((TimeSeries)obj).DayHist = GetValues(row["dayHist"].ToString());
            if (SysEnvironment.GetInstance().GetParameter("stockTrack").Value.ToString() == "S")  {
                ((TimeSeries)obj).DayStock = GetValues(row["dayStock"].ToString());
                ((TimeSeries)obj).DayUnsatDmd = GetValues(row["dayUnDmd"].ToString());
            }
            ((TimeSeries)obj).LdtHist = GetValues(row["ldtHist"].ToString());
            ((TimeSeries)obj).LdtFcst = GetValues(row["ldtFcst"].ToString());
            ((TimeSeries)obj).LdtOver = GetValues(row["ldtOver"].ToString());
            ((TimeSeries)obj).UStr1 = row["uStr1"].ToString();
            ((TimeSeries)obj).UStr2 = row["uStr2"].ToString();
            ((TimeSeries)obj).Updated = GetDate(row["updated"]);
            ((TimeSeries)obj).Created = GetDate(row["created"]);
        }


        protected override void SetAObjs(DataRowCollection rows, List<DbObject> objs) {
            TimeSeries obj;
            foreach (DataRow row in rows)       {
                obj = new TimeSeries();
                SetAObj(row, obj);
                objs.Add(obj);
            }
        }

        #endregion

        #region Private Methods

        private List<double> GetValues(string valuesStr) {
            List<double> values = new List<double>();
            try {
                string[] tokens = valuesStr.Split(separator);
                double value;
                for (int i = 0; i < tokens.Length; i++) {
                    if (tokens[i] == "") { continue; }
                    value = Convert.ToDouble(tokens[i]);
                    values.Add(value);
                }
            }
            catch (Exception ex) { Console.WriteLine("valuesStr : " + valuesStr + " stacktrace: " + ex.StackTrace); }
            return values;
        }

        private string GetString(IList<double> values) {
            if (values == null || values.Count == 0) { return ""; }
            StringBuilder dataStr = new StringBuilder();
            for (int i = 0; i < values.Count; i++) { dataStr.Append(values[i] + separatorStr); }
            return dataStr.ToString();
        }

        #endregion

    }
}
