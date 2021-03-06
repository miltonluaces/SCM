#region Imports

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace AibuSet
{

    internal class BomDemand : Demand
    {

        #region Fields

        private BomRelation bomRel;

        #endregion

        #region Constructor

        internal BomDemand() : base() {
        }

        #endregion

        #region Properties

        internal BomRelation BomRel {
            get { return bomRel; }
            set { bomRel = value; }
        }

        #endregion

        #region Persistence

        internal override Broker GetBroker() { return BrkrMgr.GetInstance().GetBroker((BomDemand)this); }

        #endregion

        #region ToString override

        public override string ToString()  {
            return base.ToString() + " " + bomRel.ToString();
        }

        #endregion



    }
}
