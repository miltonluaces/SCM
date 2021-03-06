#region Imports

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

#endregion

namespace GUI {

    internal class SysLog {

        private StreamWriter logFile;

        internal SysLog() { 
            string logFileStr = @"..\..\..\Documents\SysLog.txt";
            logFile = new StreamWriter(logFileStr);
        }

        internal void WriteLog(string txt) {
            DateTime date = DateTime.Now;
            logFile.WriteLine(date.ToShortDateString() + " " + date.ToShortTimeString() + ":\t " + txt);
            logFile.Close();
        }
    }
}
