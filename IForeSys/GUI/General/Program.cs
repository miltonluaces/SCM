#region Imports

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Configuration;
using System.Windows;
using AibuSet;
using KernelWrapp;

#endregion

namespace GUI {

    static class Program    {

        #region Fields

        static SysEnvironment sysEnv;

        #endregion

        #region internal Method Main

        [STAThread]
        static void Main(string[] args)  {

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new SplashScreenFrm());

            HcReader hcr = new HcReader();
            bool ok = hcr.Process(@"..\AILicKey.txt");
            //if (!ok) { MessageBox.Show("Database Error"); return; }
            sysEnv = SysEnvironment.GetInstance();
            sysEnv.Demo = true;
            if (sysEnv.Demo && DateTime.Now > new DateTime(2017, 1, 1)) { return; }

            UpdateVersion();
            ConnectionTest();
            Dictionary<string, Parameter> parameters = LoadParameters();
            LoadProcesses();
            LoadNodes();
            LoadSuppliers();
            LoadPlannedOrders();
            LoadDatabases();
            DeleteRcpOrders();
            sysEnv.WriteLog("Inicio AILogSys");

            if (!sysEnv.Demo && args != null && args.Length > 0)
            {
                BatchProcess bp = new BatchProcess();
                bp.Condition = parameters["batchFilter"].Value.ToString();
                bp.Process(args[0]);
                sysEnv.CloseLog();
                return;
            }

            Application.Run(new MainFrm());
        }

        #endregion

        #region Private Methods

        static void UpdateVersion()
        {
            if (sysEnv.Demo) { return; }

            FTP ftp = new FTP();
            string currSysName = sysEnv.SysFileName;
            string newSysName = ftp.GetDirList();
            char[] trimChars = { '/' };
            string newFileName = newSysName.Replace("Install/", "");
            string newVersion = newFileName.Replace("Install", "").Replace(".zip", "").Replace("_", ".");
            if (currSysName != newSysName)
            {
                DialogResult res = MessageBox.Show("Ud. tiene instalada la versión " + sysEnv.Version + ". Hay una nueva versión (" + newVersion + ") ¿Desea instalarla ahora?", "AILogSys", MessageBoxButtons.YesNo);
                if (res == DialogResult.Yes)
                {
                    ftp.Download("", newFileName);
                    Zipper zipper = new Zipper();
                    zipper.Unzip("\\", newFileName);
                    File.Delete(newFileName);
                    MessageBox.Show("La nueva versión " + newVersion + " ha sido instalada con éxito. Puede reiniciar el sistema.", "AILogSys");
                    return;
                }
            }
        }

        static void ConnectionTest()
        {
            DBJob test = new DBJob();
            test.Initialize();
            test.TestConnection();
        }

        static Dictionary<string, Parameter> LoadParameters()
        {
            Dictionary<string, Parameter> parameters = new Dictionary<string, Parameter>();
            List<DbObject> paramObjs = new List<DbObject>();
            new Parameter().ReadMany(paramObjs, "");
            foreach (Parameter par in paramObjs)
            {
                parameters.Add(par.Code, par); //sysEnv.WriteLog("Parameter : " + par.Code); 
            }
            SysEnvironment.GetInstance().Parameters = parameters;
            return parameters;
        }

        static void LoadProcesses()
        {
            Dictionary<string, AibuSet.Process> processes = new Dictionary<string, AibuSet.Process>();
            List<DbObject> processObjs = new List<DbObject>();
            new AibuSet.Process().ReadMany(processObjs, "");
            foreach (AibuSet.Process pro in processObjs)
            {
                processes.Add(pro.Code, pro); //sysEnv.WriteLog("Process : " + pro.Code); 
            }
            SysEnvironment.GetInstance().Processes = processes;
        }

        static void LoadNodes()
        {
            List<DbObject> dbObjs = new List<DbObject>();
            new AibuSet.Node().ReadMany(dbObjs, "1=1");
            Dictionary<ulong, ITTItem> ittItems = new Dictionary<ulong, ITTItem>();
            foreach (ITTItem itti in dbObjs) {
                ((AibuSet.Node)itti).Calendar.Read();
                ittItems.Add(((DbObject)itti).Id, itti); 
            }
            sysEnv.Nodes = ittItems;
        }

        static void LoadSuppliers()
        {
            List<DbObject> dbObjs = new List<DbObject>();
            new AibuSet.Supplier().ReadMany(dbObjs, "1=1");
            Dictionary<ulong, ITTItem> ittItems = new Dictionary<ulong, ITTItem>();
            foreach (ITTItem itti in dbObjs) { ittItems.Add(((DbObject)itti).Id, itti); }
            sysEnv.Suppliers = ittItems;
        }

        static void LoadPlannedOrders() {
            DBJob dbJob = new DBJob();
            dbJob.Initialize();
            sysEnv.PlannedOrders = dbJob.GetDataTable("SELECT * FROM AilPuOrder WHERE state = 0");
        }

        static void LoadDatabases()
        {
            Dictionary<string, SysEnvironment.Database> databases = new Dictionary<string, AibuSet.SysEnvironment.Database>();
            SysEnvironment.Database database;
            for (int i = 0; i < ConfigurationManager.ConnectionStrings.Count; i++)
            {
                database = new SysEnvironment.Database(ConfigurationManager.ConnectionStrings[i].Name, sysEnv.GetProviderFromName(ConfigurationManager.ConnectionStrings[i].ProviderName), ConfigurationManager.ConnectionStrings[i].ConnectionString);
                databases.Add(database.name, database);
            }
            sysEnv.Databases = databases;
        }

        static void DeleteRcpOrders()
        {
            List<DbObject> puOrderObjs = new List<DbObject>();
            new PuOrder().ReadMany(puOrderObjs, "state = 2");
            foreach (PuOrder po in puOrderObjs)
            {
                if (po.RcpDate >= DateTime.Now) { po.Delete(); }
            }

            List<DbObject> trOrderObjs = new List<DbObject>();
            new TrOrder().ReadMany(trOrderObjs, "state = 2");
            foreach (TrOrder to in trOrderObjs)
            {
                if (to.RcpDate >= DateTime.Now) { to.Delete(); }
            }
        }

        #endregion
    }
}
