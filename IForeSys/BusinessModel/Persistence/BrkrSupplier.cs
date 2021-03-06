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

    internal class BrkrSupplier : Broker {

        #region Fields
        #endregion

        #region Constructor

        internal BrkrSupplier()
            : base()
        {
        }

        #endregion

        #region internal Virtual Methods

        protected override DataSet GetSet() {
            return set;
        }

        protected override string GetTableName()  {
            return "AilSupplier";
        }

        protected override string GetPrimaryKey() {
            return "supplierId";
        }

        protected override void ObjASet(DbObject obj, DataRow row) {
            row[GetPrimaryKey()] = ((Supplier)obj).Id;
            row["code"] = ((Supplier)obj).Code;
            row["name"] = ((Supplier)obj).Name;
            row["address"] = ((Supplier)obj).Address;
            row["city"] = ((Supplier)obj).City;
            row["phone"] = ((Supplier)obj).Phone;
            row["phone"] = ((Supplier)obj).Mail;
            row["orderDays"] = GetOrderDaysStr(((Supplier)obj).OrderDays);
            row["weeklyFreq"] = ((Supplier)obj).WeeklyFreq;
            row["uStr1"] = ((Supplier)obj).UStr1;
            row["uStr2"] = ((Supplier)obj).UStr2;
            row["updated"] = ((Supplier)obj).Updated;
            row["created"] = ((Supplier)obj).Created;
        }

        protected override void SetAObj(DataRow row, DbObject obj) {
            ((Supplier)obj).Id = GetUlong(row[GetPrimaryKey()]);
            ((Supplier)obj).Code = row["code"].ToString();
            ((Supplier)obj).Name = row["name"].ToString();
            ((Supplier)obj).Address = row["address"].ToString();
            ((Supplier)obj).City = row["city"].ToString();
            ((Supplier)obj).Phone = row["phone"].ToString();
            ((Supplier)obj).Mail = row["mail"].ToString();
            ((Supplier)obj).OrderDays = GetOrderDays(row["orderDays"].ToString());
            ((Supplier)obj).WeeklyFreq = GetInt(row["weeklyFreq"]);
            ((Supplier)obj).UStr1 = row["uStr1"].ToString();
            ((Supplier)obj).UStr2 = row["uStr2"].ToString();
            ((Supplier)obj).Updated = GetDate(row["updated"]);
            ((Supplier)obj).Created = GetDate(row["created"]);
        }

        protected override void SetAObjs(DataRowCollection rows, List<DbObject> objs)
        {
            Supplier obj;
            foreach (DataRow row in rows)
            {
                obj = new Supplier();
                SetAObj(row, obj);
                objs.Add(obj);
            }
        }

        private string GetOrderDaysStr(bool[] orderDays) {
            StringBuilder sb = new StringBuilder();
            foreach (bool od in orderDays) { sb.Append(od==true ? "1" : "0");  }
            return sb.ToString();
        }

        private bool[] GetOrderDays(string orderDaysStr)  {
            bool[] orderDays = new bool[7];
            for(int i=0;i<orderDaysStr.Length;i++) { orderDays[i] = orderDaysStr[i] == '1' ? true : false;  }
            return orderDays;
        }

        #endregion
    }
}
