#region Import

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace KernelWrapp {

    internal class Results : IResults {

        #region Fields

        private string processName;
        private Dictionary<string,string> ress;

        #endregion

        #region Constructor

        internal Results(string processName) {
            this.processName = processName;
            ress = new Dictionary<string,string>();
        }

        #endregion

        #region IResults Members

        string IResults.GetProcessName() {
            return processName;
        }

        void IResults.AddResult(string result) {
            ress.Add(result, result);
        }

        internal void AddResult(ResutType resType) {
            ((IResults)this).AddResult(resType.ToString());
        }

        List<string> IResults.GetResults() {
            List<string> ressList = ress.Keys.ToList<string>();
            return ressList;
        }

        #endregion

        #region internal enum

        internal enum ResutType { Ok, Error };

        #endregion
    }
}
