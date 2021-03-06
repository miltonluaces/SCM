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
using System.Text;
using FirebirdSql.Data.FirebirdClient;

#endregion

namespace AibuSet {

    internal class DBJob {

        #region Fields

        protected string connStr;
        protected string dbName;
        protected SysEnvironment.Provider dbProvider;
        protected string dbConnStr;
        protected DbConnection conn;
        protected DbCommand cmd;
        protected DbDataAdapter adapter;
        protected DataSet set;
        protected DbCommandBuilder builder;
   
        #endregion

        #region Constructor

        internal DBJob() {
        }

        #endregion

        #region Properties

        #endregion

        #region internal Methods


        internal void Initialize() {
            SysEnvironment sysEnv = SysEnvironment.GetInstance();
            dbName = sysEnv.DatabaseName;
            dbProvider = sysEnv.DatabaseProvider;
            dbConnStr = sysEnv.DatabaseConnStr;
            set = new DataSet();
            try {
                switch (dbProvider) {
                    case SysEnvironment.Provider.SqlServer: conn = new SqlConnection(dbConnStr); break;
                    case SysEnvironment.Provider.MySql: conn = new OleDbConnection(dbConnStr); break;
                    case SysEnvironment.Provider.Firebird: conn = new FbConnection(dbConnStr); break;
                }
            }

            catch (Exception ex) {
                SysEnvironment.GetInstance().WriteLog("Error: Failed to create a database connection. " + ex.StackTrace);
            }
        }

        internal void Initialize(string dbName, SysEnvironment.Provider dbProvider, string dbConnStr) {
            this.dbName = dbName;
            this.dbProvider = dbProvider;
            this.dbConnStr = dbConnStr;
            set = new DataSet();
            try {
                switch (dbProvider) {
                    case SysEnvironment.Provider.SqlServer: conn = new SqlConnection(dbConnStr); break;
                    case SysEnvironment.Provider.MySql: conn = new OleDbConnection(dbConnStr); break;
                    case SysEnvironment.Provider.Firebird: conn = new FbConnection(dbConnStr); break;
                }
            }

            catch (Exception ex) {
                SysEnvironment.GetInstance().WriteLog("Error: Failed to create a database connection. " + ex.StackTrace);
            }
        }

        internal void TestConnection() {
            try {
                conn.Open();
                set = new DataSet();
                string query = "SELECT * FROM AilParameter";
                switch (dbProvider) {
                    case SysEnvironment.Provider.SqlServer:
                        adapter = new SqlDataAdapter();
                        adapter.SelectCommand = new SqlCommand(query, (SqlConnection)conn);
                        builder = new SqlCommandBuilder((SqlDataAdapter)adapter);
                        break;
                    case SysEnvironment.Provider.Firebird:
                        adapter = new FbDataAdapter();
                        adapter.SelectCommand = new FbCommand(query, (FbConnection)conn);
                        builder = new FbCommandBuilder((FbDataAdapter)adapter);
                        break;
                }
                adapter.Fill(set, "AilParameter");
                SysEnvironment.GetInstance().WriteLog("Connection Ok");
            }
            catch (Exception ex) {
                SysEnvironment.GetInstance().WriteLog("Error: Failed to open a database connection. " + ex.StackTrace);
            }
            finally { conn.Close(); }
        }

        internal void Execute(string query) {
            try {
                conn.Open();
                cmd = GetDbCommand(query);
                cmd.ExecuteNonQuery();
            }
            catch (Exception e) { Console.WriteLine(e.StackTrace); }
            finally {
                try { conn.Close(); }
                catch (Exception e) { Console.WriteLine(e.StackTrace); }
            }
        }

        internal DataTable GetDataTable(string query) {
            conn.Open();
            DataTable dt = new DataTable();
            try {
                adapter = GetAdapter(query);
                this.adapter.Fill(dt);
            }
            catch (Exception ex) {
                Console.WriteLine("Error in GetDataTable:" + ex.StackTrace);
            }
            finally {
                conn.Close();
            }
            return dt;
        }

        #endregion
        
        #region Protected Methods

        protected DbConnection GetConnection()  {
            switch (dbProvider) {
                case SysEnvironment.Provider.SqlServer: conn = new SqlConnection(dbConnStr); break;
                case SysEnvironment.Provider.MySql: conn = new OleDbConnection(dbConnStr); break;
                case SysEnvironment.Provider.Firebird: conn = new FbConnection(dbConnStr); break;
            }
            return conn; 
        }

        protected DbDataAdapter GetAdapter(string query) {
            switch (dbProvider) {
                case SysEnvironment.Provider.SqlServer:
                    adapter = new SqlDataAdapter();
                    adapter.SelectCommand = new SqlCommand(query, (SqlConnection)conn);
                    builder = new SqlCommandBuilder((SqlDataAdapter)adapter);
                    break;
                case SysEnvironment.Provider.Firebird:
                    adapter = new FbDataAdapter();
                    adapter.SelectCommand = new FbCommand(query, (FbConnection)conn);
                    builder = new FbCommandBuilder((FbDataAdapter)adapter);
                    break;
            }
            return adapter;     
        }


        protected DbCommand GetDbCommand(string query)   {
            switch (dbProvider)
            {
                case SysEnvironment.Provider.SqlServer: cmd = new SqlCommand(query, (SqlConnection)conn); break;
                case SysEnvironment.Provider.MySql: cmd = new OleDbCommand(query, (OleDbConnection)conn); break;
                case SysEnvironment.Provider.Firebird: cmd = new FbCommand(query, (FbConnection)conn); break;
            }
            return cmd;
        }

        protected DbDataAdapter GetDataAdapter()
        {
            switch (dbProvider)
            {
                case SysEnvironment.Provider.SqlServer: adapter = new SqlDataAdapter(); break;
                case SysEnvironment.Provider.MySql: adapter = new OleDbDataAdapter(); break;
                case SysEnvironment.Provider.Firebird: adapter = new FbDataAdapter(); break;
            }
            return adapter;
        }

        protected DbDataAdapter GetDataAdapter(string query)
        {
            switch (dbProvider)
            {
                case SysEnvironment.Provider.SqlServer: adapter = new SqlDataAdapter(query, (SqlConnection)conn); break;
                case SysEnvironment.Provider.MySql: adapter = new OleDbDataAdapter(query, (OleDbConnection)conn); break;
                case SysEnvironment.Provider.Firebird: adapter = new FbDataAdapter(query, (FbConnection)conn); break;
            }
            return adapter;
        }
        
        #endregion

    }
}
