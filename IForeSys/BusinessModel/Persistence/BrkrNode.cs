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
using System.Text;

#endregion

namespace AibuSet  {

    internal class BrkrNode : Broker {

        #region Fields
        #endregion

        #region Constructor

        internal BrkrNode()
            : base() {
        }

        #endregion

        #region internal Virtual Methods

        protected override DataSet GetSet() {
            return set;
        }

        protected override string GetTableName()  {
            return "AilNode";
        }

        protected override string GetPrimaryKey()  {
            return "nodeId";
        }

        protected override void ObjASet(DbObject obj, DataRow row) {
            row[GetPrimaryKey()] = ((Node)obj).Id.ToString();
            row["code"] = ((Node)obj).Code;
            row["name"] = ((Node)obj).Name;
            row["descr"] = ((Node)obj).Descrip;
            row["address"] = ((Node)obj).Address;
            row["city"] = ((Node)obj).City;
            row["phone"] = ((Node)obj).Phone;
            row["mail"] = ((Node)obj).Mail;
            row["calendarId"] = ((Node)obj).Calendar.Id;
            row["schLevel"] = ((Node)obj).SchLevel;
            row["topOrder"] = ((Node)obj).TopOrder;
            row["aggrHist"] = ((Node)obj).AggrHist;
            row["aggrFcst"] = ((Node)obj).AggrFcst;
            row["rootNodeId"] = ((Node)obj).RootNode.Id;
            row["orderDays"] = GetOrderDaysStr(((Node)obj).OrderDays);
            row["weeklyFreq"] = ((Node)obj).WeeklyFreq;
            row["uStr1"] = ((Node)obj).UStr1;
            row["uStr2"] = ((Node)obj).UStr2;
            row["updated"] = ((Node)obj).Updated;
            row["created"] = ((Node)obj).Created;
        }

        protected override void SetAObj(DataRow row, DbObject obj) {
            ((Node)obj).Id = GetUlong(row[GetPrimaryKey()]);
            ((Node)obj).Code = row["code"].ToString();
            ((Node)obj).Name = row["name"].ToString();
            ((Node)obj).Descrip = row["descr"].ToString();
            ((Node)obj).Address = row["address"].ToString();
            ((Node)obj).City = row["city"].ToString();
            ((Node)obj).Phone = row["phone"].ToString();
            ((Node)obj).Mail = row["mail"].ToString();
            ulong calendarId = GetUlong(row["calendarId"]);
            ((Node)obj).Calendar = new Calendar(calendarId);
            ulong rootNodeId = GetUlong(row["rootNodeId"]);
            ((Node)obj).RootNode = new Node(rootNodeId);
            ((Node)obj).SchLevel = GetInt(row["schLevel"]);
            ((Node)obj).TopOrder = GetInt(row["topOrder"]);
            ((Node)obj).AggrHist = Convert.ToBoolean(row["aggrHist"]);
            ((Node)obj).AggrFcst = Convert.ToBoolean(row["aggrFcst"]);
            ((Node)obj).OrderDays = GetOrderDays(row["orderDays"].ToString());
            ((Node)obj).WeeklyFreq = GetInt(row["weeklyFreq"]);
            ((Node)obj).UStr1 = row["uStr1"].ToString();
            ((Node)obj).UStr2 = row["uStr2"].ToString();
            ((Node)obj).Updated = GetDate(row["updated"]);
            ((Node)obj).Created = GetDate(row["created"]);
        }

        protected override void SetAObjs(DataRowCollection rows, List<DbObject> objs) {
            Node obj;
            foreach (DataRow row in rows)  {
                obj = new Node();
                SetAObj(row, obj);
                objs.Add(obj);
            }
        }

        private string GetOrderDaysStr(bool[] orderDays) {
            StringBuilder sb = new StringBuilder();
            foreach (bool od in orderDays) { sb.Append(od == true ? "1" : "0"); }
            return sb.ToString();
        }

        private bool[] GetOrderDays(string orderDaysStr) {
            bool[] orderDays = new bool[7];
            for (int i = 0; i < orderDaysStr.Length; i++) { orderDays[i] = orderDaysStr[i] == '1' ? true : false; }
            return orderDays;
        }
        
        #endregion
    }
}
