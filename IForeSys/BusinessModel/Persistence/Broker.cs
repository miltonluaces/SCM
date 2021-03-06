#region Imports

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Configuration;
using System.Resources;
using System.Reflection;
using System.Linq;
using System.Text;
using FirebirdSql.Data.FirebirdClient;

#endregion

namespace AibuSet {

    internal abstract class Broker : DBJob {

        #region Fields

        protected int uFieldCount;
        protected bool identityInsert;

        #endregion

        #region Constructor

        internal Broker() : base() {
            Initialize();
            uFieldCount = 2;
         }

        #endregion

        #region Properties

        internal int UFieldCount {
            get { return uFieldCount; }
            set { uFieldCount = value; }
        }

        #endregion

        #region internal Methods

        internal void SaveUpdate(DbObject dbObj) {
            try {
                set = new DataSet(); 
                string tableName = this.GetTableName();
                string query = "SELECT * FROM " + tableName + " WHERE " + GetPrimaryKey() + "=" + dbObj.Id;
                conn.Open();

                switch(dbProvider) {
                    case SysEnvironment.Provider.SqlServer:
                        adapter = new SqlDataAdapter();
                        adapter.SelectCommand = new SqlCommand(query, (SqlConnection)conn);
                        builder = new SqlCommandBuilder((SqlDataAdapter)adapter);
                        break;
                    case SysEnvironment.Provider.MySql:
                        adapter = new OleDbDataAdapter();
                        adapter.SelectCommand = new OleDbCommand(query, (OleDbConnection)conn);
                        builder = new OleDbCommandBuilder((OleDbDataAdapter)adapter);
                        break;
                    case SysEnvironment.Provider.Firebird:
                        adapter = new FbDataAdapter();
                        adapter.SelectCommand = new FbCommand(query, (FbConnection)conn);
                        builder = new FbCommandBuilder((FbDataAdapter)adapter);
                        break;
                }
                adapter.Fill(set, tableName);

                if (set.Tables[0].Rows.Count == 0) {
                    DataRow newRow = set.Tables[0].NewRow();
                    dbObj.Created = DateTime.Now;
                    dbObj.Updated = dbObj.Created;
                    this.ObjASet(dbObj, newRow);
                    set.Tables[0].Rows.Add(newRow);
                }
                else if (set.Tables[0].Rows.Count == 1) {
                    dbObj.Updated = DateTime.Now;
                    this.ObjASet(dbObj, set.Tables[0].Rows[0]);
                }
                else { 
                    throw new Exception("Error."); 
                }

                adapter.UpdateCommand = builder.GetUpdateCommand();
                adapter.Update(set, tableName);

            }

            catch (Exception e) { Console.WriteLine(e.StackTrace); }
            finally  {
                try {
                    conn.Close();
                }
                catch (Exception e) { Console.WriteLine(e.StackTrace); }
            }
        }

        internal void Read(DbObject dbObj) {
            try  {
                //set = this.GetSet();
                set = new DataSet();
                string tableName = this.GetTableName();
                string query = "SELECT * FROM " + tableName + " WHERE " + GetPrimaryKey() + "=" + dbObj.Id;
                adapter = GetDataAdapter(query);
                conn.Open();
                adapter.Fill(set);

                if (set.Tables[0].Rows.Count == 1)  {
                    this.SetAObj(set.Tables[0].Rows[0], dbObj);
                }
                else {
                    throw new Exception("Error. Number of registers different from one : " + this.GetSet().Tables[0].Rows.Count);
                }

            }
            catch (Exception ex) { /*throw new Exception("Error", ex);*/ }
            finally  {
                try  {
                    conn.Close();
                }
                catch (Exception e) { Console.WriteLine(e.StackTrace); }
            }
        }

        internal void Read(DbObject dbObj, string fieldName, ulong id) {
            try  {
                set = new DataSet();
                string tableName = this.GetTableName();
                string query = "SELECT * FROM " + tableName + " WHERE " + fieldName + "=" + id;
                adapter = GetDataAdapter(query);
                conn.Open();
                adapter.Fill(set);

                if (set.Tables[0].Rows.Count == 1) { this.SetAObj(set.Tables[0].Rows[0], dbObj); }
                else {  throw new Exception("Error. Number of registers different from one : " + this.GetSet().Tables[0].Rows.Count); }

            }
            catch (Exception ex) { /*throw new Exception("Error", ex);*/ }
            finally {
                try { conn.Close(); }
                catch (Exception e) { Console.WriteLine(e.StackTrace); }
            }
        }

        internal void Read(DbObject dbObj, string condition) {
            try {
                set = new DataSet();
                string tableName = this.GetTableName();
                string query = "SELECT * FROM " + tableName + " WHERE " + condition;
                adapter = GetDataAdapter(query);
                conn.Open();
                adapter.Fill(set);

                if (set.Tables[0].Rows.Count == 1) { this.SetAObj(set.Tables[0].Rows[0], dbObj); }
                else { throw new Exception("Error. Number of registers different from one : " + this.GetSet().Tables[0].Rows.Count); }

            }
            catch (Exception ex) { /*throw new Exception("Error", ex);*/ }
            finally {
                try { conn.Close(); }
                catch (Exception e) { Console.WriteLine(e.StackTrace); }
            }
        }

        internal void Delete(DbObject dbObj) {
            try {
                string query = "DELETE FROM " + GetTableName() + " WHERE " + GetPrimaryKey() + "=" + dbObj.Id;
                cmd = GetDbCommand(query);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
            catch (Exception e) { Console.WriteLine(e.StackTrace); }

            finally { 
                try { conn.Close();  }
                catch (Exception e) { Console.WriteLine(e.StackTrace); }
            }
        }

        internal void DeleteMany(DbObject dbObj, string condition) {
            try {
                string query = "DELETE FROM " + GetTableName() + " WHERE " + condition;
                cmd = GetDbCommand(query);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
            catch (Exception e) { Console.WriteLine(e.StackTrace); }

            finally {
                try { conn.Close(); }
                catch (Exception e) { Console.WriteLine(e.StackTrace); }
            }
        }

        internal void UpdateMany(DbObject dbObj, string updQuery) {
            try {
                string query = "UPDATE " + GetTableName() + " " + updQuery;
                cmd = GetDbCommand(query);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
            catch (Exception e) { Console.WriteLine(e.StackTrace); }

            finally {
                try { conn.Close(); }
                catch (Exception e) { Console.WriteLine(e.StackTrace); }
            }
        }

        internal void ReadMany(List<DbObject> objs, string condition)   {
            if (condition == "") { condition = "1=1"; }
            try  {
                set = new DataSet();
                string tableName = this.GetTableName();
                string query = "SELECT * FROM " + tableName + " WHERE " + condition;
                adapter = GetDataAdapter(query);
                conn.Open();
                adapter.Fill(set);

                if (set.Tables[0].Rows.Count > 0) {  this.SetAObjs(set.Tables[0].Rows, objs); }
            }
            catch (Exception ex) { /*throw new Exception("Error", ex);*/ }
            finally  {
                try  { conn.Close(); }
                catch (Exception e) { Console.WriteLine(e.StackTrace); }
            }
        }
        
        #endregion

        #region Virtual Methods

        //metodos virtuales
        internal void saveReferences(DbObject dbObj) { }
        internal void readReferences(DbObject dbObj) { }
     
        //helpers
        protected virtual void ObjASet(DbObject obj, DataRow row) { }
        protected virtual void SetAObj(DataRow row, DbObject obj) { }
        protected virtual void SetAObjs(DataRowCollection rows, List<DbObject> objs) { }
        protected virtual DataSet GetSet() { return null; }
        protected virtual string GetTableName() { return null; }
        protected virtual string GetPrimaryKey() { return null; }
      
        #endregion

        #region Auxiliar Methods

        protected DateTime GetDate(object dateObj)
        {
            if (dateObj == null || dateObj.ToString() == "") { return new DateTime(1900, 1, 1); }
            DateTime date;
            try { date = ((DateTime)dateObj).Date; }
            catch { date = new DateTime(1900, 1, 1); }
            return date;
        }

        protected double GetDouble(object doubleObj) {
            if (doubleObj == null || doubleObj.ToString() == "") { return -1; }
            return Convert.ToDouble(doubleObj);
        }

        protected int GetInt(object intObj) {
            if (intObj == null || intObj.ToString() == "") { return -1; }
            return (int)(intObj);
        }

        protected ulong GetUlong(object ulongObj) {
            if (ulongObj == null || ulongObj.ToString() == "") { return 0; }
            return Convert.ToUInt64(ulongObj);
        }

        protected bool GetBoolean(object boolObj) {
            int boolInt = GetInt(boolObj);
            return (boolInt == 1);
        }

        protected string GetString(object strObj) {
            if (strObj == null) { return ""; }
            return strObj.ToString();
        }

        #endregion

    }
}