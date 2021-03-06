#region Imports

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

#endregion

namespace GUI {

    public class SysLog {

        private StreamWriter logFile;

        public SysLog() { 
            string logFileStr = @"..\Files\ailsLog.txt";
            logFile = new StreamWriter(logFileStr);
        }

        public void WriteLog(string txt) {
            DateTime date = SysEnvironment.GetInstance().Now;
            string msg = date.ToShortDateString() + " " + date.ToShortTimeString() + ":\t " + txt;
            Console.WriteLine(msg);
            logFile.WriteLine(msg);
            logFile.Close();
        }
    }
}
