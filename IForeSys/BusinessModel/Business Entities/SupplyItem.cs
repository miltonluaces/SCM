#region Imports

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace BusinessModel {

    public class SupplyItem : Item {

        #region Fields

        private Supply supply;

        #endregion

        #region Constructor

        public SupplyItem() : base() {
        }

        #endregion

        #region Properties

        public Supply Supply {
            get { return supply; }
            set { supply = value; }
        }

        #endregion

        #region Persistence

        public override Broker GetBroker() { return BrkrMgr.GetInstance().GetBroker((SupplyItem)this); }

        #endregion

        #region ToString override

        public override string ToString() {
            return base.ToString() + " " + supply.Code;
        }

        #endregion

    }
}
