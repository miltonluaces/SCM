#region Imports

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace KernelWrapp {

    interface IResults {
        string GetProcessName();
        void AddResult(string res);
        List<string> GetResults();
    }
}
