#region Imports

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using AibuSet;
using System.Data.Sql;
using System.Data.SqlClient;
using FirebirdSql.Data.FirebirdClient;

#endregion

namespace AibuSet {

    internal class Pager : DBJob {
        
        #region Fields

        private int pageSize;
        private int advSize;
        private int pageIndex;
        private int totPages;
        private string condition;
        
        #endregion

        #region Constructor

        internal Pager() {
            Initialize();
            pageSize = 1000;
            advSize = 5;
            pageIndex = 1;
            totPages = 0;
            condition = "1=1";
        }

        #endregion

        #region Properties

        internal int PageSize {
            get { return pageSize; }
            set { pageSize = value; }
        }

        internal int AdvSize {
            get { return advSize; }
            set { advSize = value; }
        }

        internal string Condition {
            get { return condition; }
            set { condition = value; }
        }
        
        #endregion

        #region internal Methods

        internal void CalculateTotalPages() {
            int rowCount = 0;
            try {
                conn.Open();
                string query = "SELECT COUNT(*) FROM AilSku WHERE " + condition;
                DataTable dt = new DataTable();
                //cmd = new SqlCommand(query, (SqlConnection)conn);
                //adapter = new SqlDataAdapter();
                //this.adapter.SelectCommand = cmd;
                adapter = GetAdapter(query);
                this.adapter.Fill(dt);
                rowCount = (int)dt.Rows[0][0];
            }
            catch (Exception ex) { Console.WriteLine(ex.StackTrace); }
            finally {
                conn.Close();
            }
            totPages = rowCount / pageSize;
            if (rowCount % pageSize > 0) { totPages += 1; } //if rest, add one page
        }

        internal DataTable GetCurrentRecords(int page) {
            conn.Open();
            DataTable dt = new DataTable();
            string query = "";
            if (page == 1)   {
                switch (dbProvider) {
                    case SysEnvironment.Provider.SqlServer:
                        query = "SELECT TOP " + pageSize + " * FROM AilSku  WHERE " + condition + " ORDER BY skuId";
                        adapter = new SqlDataAdapter();
                        adapter.SelectCommand = new SqlCommand(query, (SqlConnection)conn);
                        builder = new SqlCommandBuilder((SqlDataAdapter)adapter);
                        break;
                    case SysEnvironment.Provider.Firebird:
                        query = "SELECT FIRST " + pageSize + " * FROM AilSku  WHERE " + condition + " ORDER BY skuId";
                        adapter = new FbDataAdapter();
                        adapter.SelectCommand = new FbCommand(query, (FbConnection)conn);
                        builder = new FbCommandBuilder((FbDataAdapter)adapter);
                        break;
                }
            }
            else  {
                int PreviousPageOffSet= (page - 1) * pageSize;
                switch (dbProvider) {
                    case SysEnvironment.Provider.SqlServer:
                        query = "SELECT TOP " + pageSize + " * FROM AilSku WHERE " + condition + " AND skuId NOT IN (SELECT TOP " + PreviousPageOffSet + " skuId FROM AilSku WHERE " + condition + " ORDER BY skuId) ";
                        adapter = new SqlDataAdapter();
                        adapter.SelectCommand = new SqlCommand(query, (SqlConnection)conn);
                        builder = new SqlCommandBuilder((SqlDataAdapter)adapter);
                        break;
                    case SysEnvironment.Provider.Firebird:
                        query = "SELECT FIRST " + pageSize + " * FROM AilSku WHERE " + condition + " AND skuId NOT IN (SELECT TOP " + PreviousPageOffSet + " skuId FROM AilSku WHERE " + condition + " ORDER BY skuId) ";
                        adapter = new FbDataAdapter();
                        adapter.SelectCommand = new FbCommand(query, (FbConnection)conn);
                        builder = new FbCommandBuilder((FbDataAdapter)adapter);
                        break;
                }
            }
    
            try  {
                //this.adapter.SelectCommand = cmd;
                //adapter = GetAdapter(query);
                this.adapter.Fill(dt);
            }
            catch(Exception ex) {
                Console.WriteLine("Error in GetCurrentRecords");
            }
            finally  {
                conn.Close();
            }
            return dt;
        }

        internal DataTable GetAllRecords(string condition) {
            conn.Open();
            DataTable dt = new DataTable();
            string query = "SELECT * FROM AilSku WHERE " + condition + " ORDER BY skuId";
            //cmd = new SqlCommand(query, (SqlConnection)conn);
            try {
                //adapter = new SqlDataAdapter();
                //this.adapter.SelectCommand = cmd;
                adapter = GetAdapter(query);
                this.adapter.Fill(dt);
            }
            catch(Exception ex) {
                Console.WriteLine("Error in GetAllRecords:" + ex.StackTrace);
            }
            finally {
                conn.Close();
            }
            return dt;
        }
        
        #endregion

        #region Event Helpers

        internal DataTable GetFirstPage() {
            this.pageIndex = 1;
            return GetCurrentRecords(pageIndex);
        }

        internal DataTable GetNextPage() {
            if (pageIndex >= this.totPages)  { return null; }
            pageIndex++;
            return GetCurrentRecords(pageIndex);
        }

        internal DataTable GetAdvNextPage() {
            if (pageIndex >= totPages) { return null; }
            pageIndex= pageIndex + advSize;
            return GetCurrentRecords(pageIndex);
        }

        internal DataTable GetPrevPage() {
            if (pageIndex <= 1) { return null; }
            pageIndex--;
            return GetCurrentRecords(pageIndex);
        }

        internal DataTable GetAdvPrevPage() {
            if (pageIndex <= 1) { return null; }
            pageIndex = pageIndex - advSize;
            return GetCurrentRecords(pageIndex);
        }

        internal DataTable GetLastPage() {
            pageIndex = totPages;
            return GetCurrentRecords(pageIndex); 
        }

        internal DataTable GetAll(string condition) {
            return GetAllRecords(condition);
        }

        #endregion

      }
}
