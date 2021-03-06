#region Imports

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Globalization;
using System.Threading;

#endregion

namespace Testing {
    
    internal class GenRCode {

        private string path;
        private StreamWriter sw;

        internal GenRCode(string fileName) {
            CultureInfo customCulture = (CultureInfo)Thread.CurrentThread.CurrentCulture.Clone();
            customCulture.NumberFormat.NumberDecimalSeparator = ".";
            Thread.CurrentThread.CurrentCulture = customCulture; 
            
            path = "C:/Users/Milton/NewDeal/Workspace/Kernel/Testing/" + fileName + ".R";
            sw = new StreamWriter(path);
            sw.WriteLine("# TEST "+ fileName);
            sw.WriteLine("# ==================================");
            sw.WriteLine("library(MiscFunctions)");
            sw.WriteLine("#");
        }

        internal void AddHistFcst(int index, IList<double> hist, IList<double> fcst) {
            sw.WriteLine(GetRArray("hist" + index, hist));
            sw.WriteLine(GetRArray("fcst" + index, fcst));
            sw.WriteLine("");
        }

        internal void AddPlot(int index) {
            sw.WriteLine("PlotSeries(hist" + index + ", fcst" + index + ", title= 'Plot " + index + "' )");
        }

        internal void AddPlots(int ini, int end) {
            for (int i = ini; i <= end; i++) {
                AddPlot(i);
            }
        }

        internal void AddSection(string title) {
            sw.WriteLine("# " + title);
            sw.WriteLine("# ----------------------------------------------------------------------------------------------------");
            sw.WriteLine("#");
        }
        
        internal void Close() {
            sw.Close();
        }

        private string GetRArray(string name, IList<double> ts) {
            StringBuilder sb = new StringBuilder();
            sb.Append(name + " = c(");
            for (int i = 0; i < ts.Count - 1; i++) { sb.Append(ts[i] + ","); }
            sb.Append(ts[ts.Count - 1] + ")");
            return sb.ToString();
        }

    }
}
