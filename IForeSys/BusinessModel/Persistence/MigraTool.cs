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

#endregion

namespace AibuSet {

    internal class MigraTool : DBJob {

        #region Fields

        private string excelConnStr;

        #endregion
        
        #region Constructor

        internal MigraTool() {
            excelConnStr = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=Databases/Migrat.xls;Extended Properties=Excel 8.0;";
        }

        #endregion


    }
}
