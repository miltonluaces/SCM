#region Imports

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace AibuSet
{
    internal class BinLargeObj : DbObject {

        private StringBuilder data;

        internal BinLargeObj()
            : base() {
            data = new StringBuilder();
        }

        #region Properties

        internal string GetData() { return data.ToString(); }
        internal void AddData(string str) { data.Append(str); }
        internal void ClearData() { data = new StringBuilder(); }

	    #endregion

        #region Persistence

        internal override Broker GetBroker() { return BrkrMgr.GetInstance().GetBroker((BinLargeObj)this); }

        #endregion

        #region ToString override

        public override string ToString()
        {
            return base.ToString();
        }

        #endregion


    }
}
