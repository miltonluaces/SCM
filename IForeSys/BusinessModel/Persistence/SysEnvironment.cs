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
using System.ComponentModel;
using System.Linq;
using System.IO;


#endregion

namespace AibuSet {

    internal class SysEnvironment {
        
        #region Singleton

        private static SysEnvironment se;
        private Config config;
        private string system;
        private string version;
        private bool demo;
        private DateTime nullDate;
        
        private SysEnvironment()  {

            demo = false;
            nullDate = new DateTime(1800, 1, 1);

            system = "AILogSys";
            version = "2.0";
            supChain = new SupplyChain();
            //supChain.LoadGraphs();
            config = new Config();
            config.Database = "AILogSys";

            string logFileStr = @"..\Files\ailsLog.txt";
            try { log = new StreamWriter(logFileStr); }
            catch (Exception ex) { Console.WriteLine("Error. No se encontro el archivo ailsLog.txt"); }
        }

        internal static SysEnvironment GetInstance() {
            if (se == null) { se = new SysEnvironment(); }
            return se;
        }
        
        #endregion

        #region Fields

        private SupplyChain supChain;
        private bool test;
        private bool loggedOn;
        private Dictionary<string, Parameter> parameters;
        private Dictionary<string, Process> processes;
        private Dictionary<ulong, ITTItem> suppliers;
        private Dictionary<ulong, ITTItem> nodes;
        private DataTable plannedOrders;
        private Dictionary<string, Database> databases;
        private StreamWriter log;
        
        #endregion

        #region Properties

        internal bool Demo {
            get { return demo; }
            set { demo = value; }
        }

        internal string System {
            get { return system; }
        }

        internal string Version {
            get { return version; }
        }

        internal string SysFileName {
            get {
                return "Install/" + system + version.Replace(".", "_") + "zip";
            }
        }

        internal string DatabaseName {
            get {
                if (demo) { return "AILogSysDemo"; }
                return config.Database; 
            }
            set { config.Database = value; }
        }

        internal Provider DatabaseProvider {
            get {
                if (demo) { return Provider.Firebird; }
                return GetProviderFromName(ConfigurationManager.ConnectionStrings[config.Database].ProviderName);
            }
            
        }

        internal string DatabaseConnStr {
            get {
                if (demo) { return "dialect=3;character set=UTF8;password=masterkey;user id=SYSDBA;initial catalog=AILogSysDemo.fdb;server type=Embedded"; }
                if (ConfigurationManager.ConnectionStrings[config.Database] == null) { return ""; }
                return ConfigurationManager.ConnectionStrings[config.Database].ConnectionString; 
            }
        }

        internal Provider GetProvider(string dbName) {
            if (!databases.ContainsKey(dbName)) { throw new Exception("Error. La base de datos " + dbName + " no esta registrada."); }
            return databases[dbName].provider;
        }

        internal string GetConnStr(string dbName) {
            if (!databases.ContainsKey(dbName)) { throw new Exception("Error. La base de datos " + dbName + " no esta registrada."); }
            return databases[dbName].connStr;
        }

        internal bool Test {
            get { return test; }
            set { test = value; }
        }

        internal bool LoggedOn {
            get { return loggedOn; }
            set { loggedOn = value; }
        }

        internal Parameter GetParameter(string code) { 
            if(!parameters.ContainsKey(code)) { 
                WriteLog("Error. No existe el parametro " + code);
                WriteLog("Parametros validos : ");
                foreach (string key in parameters.Keys) { WriteLog(key); }
            }
            return parameters[code];
        }

        internal void SetParameter(string code, object value) {
            if (!parameters.ContainsKey(code)) {
                WriteLog("Error. No existe el parametro " + code);
            }
            parameters[code].Value = value;
            parameters[code].SaveUpdate();
        }

        internal Process GetProcess(string code) {
            if (!processes.ContainsKey(code)) { 
                WriteLog("Error. No existe el proceso " + code); 
                WriteLog("Procesos validos : ");
                foreach (string key in processes.Keys) { WriteLog(key); }
            }
            return processes[code];
        }

        internal Dictionary<string, Parameter> Parameters  {
            get { return parameters; }
            set { parameters = value; }
        }

        internal Dictionary<string, Process> Processes  {
            get { return processes; }
            set { processes = value; }
        }
        
        internal Dictionary<ulong, ITTItem> Suppliers  {
            get { return suppliers; }
            set { suppliers = value; }
        }

        internal Dictionary<ulong, ITTItem> Nodes {
            get { return nodes; }
            set { nodes = value; }
        }

        internal DataTable PlannedOrders  {
            get { return plannedOrders; }
            set { plannedOrders = value; }
        }

        internal Dictionary<string, Database> Databases    {
            get { return databases; }
            set { databases = value; }
        }
        
        internal Dictionary<string, string> GetMailList()
        {
            Dictionary<string, string> mails = new Dictionary<string, string>();
            string mailList = ConfigurationManager.AppSettings["mailList"];
            char sep1 = ';';
            char sep2 = ',';
            string[] members = mailList.Split(sep1);
            foreach (string member in members) {
                string[] tokens = member.Split(sep2);
                mails.Add(tokens[0], tokens[1]);
            }
            return mails;
        }

        internal DateTime NullDate() {
            return nullDate;
        }
  
        #endregion

        #region internal Methods

        internal Provider GetProviderFromName(string providerName) {
            switch (providerName)   {
                case "System.Data.SqlClient": return Provider.SqlServer;
                case "MySql": return Provider.MySql;
                case "FirebirdSql.Data.FirebirdClient": return Provider.Firebird;
                default: return Provider.SqlServer;
            }
        }

        internal List<Node> GetBranches(Node root, bool bom) {
            if (bom) { return supChain.GetMainAdjacents(root); }
            else { return supChain.GetBomAdjacents(root); }
        }

        internal DateTime GetDate(string dateStr) {
            if (dateStr == null || dateStr == "") { return new DateTime(1900, 1, 1); }
            DateTime date;
            try { date = Convert.ToDateTime(dateStr); }
            catch { date = new DateTime(1900, 1, 1); }
            return date;
        }

        internal double GetDouble(object doubleStr) {
            if (doubleStr == null || doubleStr.ToString() == "") { return -1; }
            return Convert.ToDouble(doubleStr);
        }

        internal int GetInt(object intStr) {
            if (intStr == null || intStr.ToString() == "") { return -1; }
            return (int)(intStr);
        }

        internal ulong GetUlong(object ulongStr) {
            if (ulongStr == null || ulongStr.ToString() == "") { return 0; }
            return Convert.ToUInt64(ulongStr);
        }

        internal bool GetBoolean(object boolStr) {
            int boolInt = GetInt(boolStr);
            return (boolInt == 1);
        }

        internal void WriteLog(string txt) {
            if (log == null) { return; }
            DateTime date = DateTime.Now;
            string msg = date.ToShortDateString() + " " + date.ToShortTimeString() + ":\t " + txt;
            Console.WriteLine(msg);
            log.WriteLine(msg);
        }

        internal void CloseLog() {
            if (log == null) { return; }
            log.Close();
        }
             
        #endregion

        #region internal classes and enums

        internal class Database {
            internal string name;
            internal Provider provider;
            internal string connStr;

            internal Database(string name, Provider provider, string connStr) {
                this.name = name;
                this.provider = provider;
                this.connStr = connStr;
            }
        }
        
        internal enum Provider { SqlServer, MySql, Firebird };

        #endregion

   
    }
}
