#region Imports

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace AibuSet {

    internal class BomRelation : Relation {

        #region Fields

        private double qty;
        private int offset;

        #endregion

        #region Constructor

        internal BomRelation() : base() {
        }

        internal BomRelation(ulong id) : base(id)  {
        }
        internal BomRelation(Node origin, Node target)
            : base(origin, target) {
            this.origin = origin;
            this.target = target;
        }

        #endregion

        #region Properties

        internal double Qty {
            get { return qty; }
            set { qty = value; }
        }

        internal int Offset {
            get { return offset; }
            set { offset = value; }
        }

        #endregion

        #region Persistence

        internal override Broker GetBroker() { return BrkrMgr.GetInstance().GetBroker((BomRelation)this); }

        #endregion

        #region ToString override

        public override string ToString() {
            return base.ToString() + " " + qty + " " + offset;
        }

        #endregion


    }
}
