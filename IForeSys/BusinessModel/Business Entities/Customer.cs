#region Imports

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace AibuSet {

    internal class Customer : Agent {

        #region Constructor

        internal Customer()
            : base() {
        }

        internal Customer(ulong id)
            : base(id)
        {
        }

        #endregion

        #region Persistence

        internal override Broker GetBroker() { return BrkrMgr.GetInstance().GetBroker((Customer)this); }

        #endregion

        #region ToString override

        public override string ToString()  {
            return base.ToString();
        }

        #endregion


    }
}
