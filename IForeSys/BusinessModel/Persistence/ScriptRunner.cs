#region Imports

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.OleDb;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Data;
using System.IO;
using FirebirdSql.Data.FirebirdClient;

#endregion

namespace AibuSet {

    internal class ScriptRunner : DBJob {

        #region Fields

        private bool writeLogs;
        private string path;

        #endregion

        #region Constructors

        internal ScriptRunner(string path)
            : base() {
            writeLogs = true;
            this.path = path;
            if (path == "") { this.path = @"C:\Users\Milton\NewDeal\Workspace\IForeSys\SqlScripts\"; }
            WriteLog("Inicializando...");
            Initialize();
            WriteLog("Inicializado Ok.");

        }

        internal ScriptRunner()
            : this("") {
        }

        #endregion

        #region Properties

        internal bool WriteLogs {
            get { return writeLogs; }
            set { writeLogs = value; }
        }

        internal string Path {
            get { return path; }
        }

        #endregion

        #region Public Methods

        internal void RunScript(string fileName) {
            List<string> commands = GetCommands(fileName);
            RunCommands(commands);
        }

        internal void RunScript(string filePath, string fileName) {
            this.path = filePath;
            List<string> commands = GetCommands(fileName);
            RunCommands(commands);
        }

        #endregion

        #region Private Methods

        internal List<string> GetCommands(string fileName) {
            List<string> commands = new List<string>();
            StreamReader sr = new StreamReader(path + fileName + ".sql");
            string line = sr.ReadLine();
            while (line != null) {
                if (!line.StartsWith("--") && line != "") { commands.Add(line); }
                line = sr.ReadLine();
            }
            return commands;
        }

        private void RunCommands(List<string> commands) {
            try {
                WriteLog("Abriendo conexión...");
                conn.Open();
                WriteLog("Conexión abierta Ok.");

                WriteLog("Ejecutando comandos....");
                foreach (string command in commands) {
                    switch (dbProvider) {
                        case SysEnvironment.Provider.SqlServer:
                            cmd = new SqlCommand(command, (SqlConnection)conn); adapter = new SqlDataAdapter(); break;
                        case SysEnvironment.Provider.MySql:
                            cmd = new OleDbCommand(command, (OleDbConnection)conn); adapter = new OleDbDataAdapter(); break;
                        case SysEnvironment.Provider.Firebird:
                            cmd = new FbCommand(command, (FbConnection)conn); adapter = new FbDataAdapter(); break;
                    }
                    cmd.ExecuteNonQuery();
                }
                WriteLog("Comandos ejecutados Ok.");
            }

            catch (Exception e) { Console.WriteLine(e.StackTrace); }
            finally {
                try { conn.Close(); }
                catch (Exception e) { Console.WriteLine(e.StackTrace); }
            }
        }

        private void WriteLog(string log) {
            if (!writeLogs) { return; }
            Console.WriteLine(log);
        }

        #endregion
    }
}

