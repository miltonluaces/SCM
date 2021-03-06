#region Imports

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace AibuSet {

    internal class Relation : BusObject {

        #region Fields

        protected Node origin;
        protected Node target;

        #endregion
        
        #region Constructor

        internal Relation()
            : base() {
        }


        internal Relation(ulong id)
            : base(id)
        {
        }

        internal Relation(Node origin, Node target)
            : base() {
            this.origin = origin;
            this.target = target;
        }
        
        #endregion

        #region Properties

        internal Node Origin {
            get { return origin; }
            set { origin = value; }
        }

        internal Node Target
        {
            get { return target; }
            set { target = value; }
        }
        
        #endregion

        #region Persistence

        internal override Broker GetBroker() { return BrkrMgr.GetInstance().GetBroker((Relation)this); }

        #endregion

        #region ToString override

        public override string ToString() {
            return base.ToString() + " " + origin.Code + " " + target.Code;
        }

        #endregion


    }
}
