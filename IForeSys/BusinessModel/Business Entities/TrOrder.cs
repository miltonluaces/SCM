#region Imports

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace AibuSet {

    internal class TrOrder : PuOrder {

        #region Fields

        private Sku supSku;
        private Sku conSku;
        private double allocQty;
        private string barcode;

        #endregion

        #region Constructor

        internal TrOrder()
            : base() {
        }

        #endregion

        #region Properties

        public string Barcode  {
            get { return barcode; }
            set { barcode = value; }
        }

        internal Sku SupSku
        {
            get { return supSku; }
            set { supSku = value; }
        }

        internal Sku ConSku {
            get { return conSku; }
            set { conSku = value; }
        }

        internal double AllocQty {
            get { return allocQty; }
            set { allocQty = value; }
        }

        #endregion

        #region ToString override

        public override string ToString() {
            return base.ToString() + " " + supSku.Code + " " + supSku.Code + " " + qty;
        }

        #endregion

        #region Persistence

        internal override Broker GetBroker() { return BrkrMgr.GetInstance().GetBroker((TrOrder)this); }

        #endregion
  
    }
}
