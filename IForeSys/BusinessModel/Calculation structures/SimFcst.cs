#region Imports

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace BusinessModel {

    public class SimFcst : DbObject {

        public SimFcst() : base() { 
        }

        #region Persistence

        public override Broker GetBroker() { return BrkrMgr.GetInstance().GetBroker((SimFcst)this); }

        #endregion


    }
}
