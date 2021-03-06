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

    internal class BrkrCustomer : Broker
    {

        #region Fields
        #endregion

        #region Constructor

        internal BrkrCustomer()
            : base() {
        }

        #endregion

        #region internal Virtual Methods

        protected override DataSet GetSet() {
            return set;
        }

        protected override string GetTableName() {
            return "AilCustomer";
        }

        protected override string GetPrimaryKey() {
            return "customerId";
        }

        protected override void ObjASet(DbObject obj, DataRow row) {
            row[GetPrimaryKey()] = ((Customer)obj).Id.ToString();
            row["code"] = ((Customer)obj).Code;
            row["name"] = ((Customer)obj).Name;
            row["address"] = ((Customer)obj).Address;
            row["city"] = ((Customer)obj).City;
            row["phone"] = ((Customer)obj).Phone;
            row["mail"] = ((Customer)obj).Mail;
            row["updated"] = ((Customer)obj).Updated;
            row["created"] = ((Customer)obj).Created;
        }

        protected override void SetAObj(DataRow row, DbObject obj) {
            ((Customer)obj).Id = GetUlong(row[GetPrimaryKey()]);
            ((Customer)obj).Code = row["code"].ToString();
            ((Customer)obj).Name = row["name"].ToString();
            ((Customer)obj).Address = row["address"].ToString();
            ((Customer)obj).City = row["city"].ToString();
            ((Customer)obj).Phone = row["phone"].ToString();
            ((Customer)obj).Mail = row["mail"].ToString();
            ((Customer)obj).Updated = GetDate(row["updated"]);
            ((Customer)obj).Created = GetDate(row["created"]);
        }

        protected override void SetAObjs(DataRowCollection rows, List<DbObject> objs) {
            Customer obj;
            foreach (DataRow row in rows) {
                obj = new Customer();
                SetAObj(row, obj);
                objs.Add(obj);
            }
        }
        
        #endregion
    }
}
