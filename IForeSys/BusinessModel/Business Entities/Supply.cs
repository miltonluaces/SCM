#region Imports

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace AibuSet {

    internal class Supply : Note {

        #region Fields

        private Supplier supplier;
	    private string orderType; //TODO: change type
	    private Sku sku;
        private SubSku subSku;
	    private double backOrderQty;
	    private double bomQty;
	    private double outQty;
	    private double lTFcstQty;
	    private double orderQty;
        private DateTime releaseDate;
        private DateTime receptionDate;
        private DateTime consumptionDate;
  
        #endregion

        #region Constructor

        internal Supply()
            : base() {
        }

        internal Supply(ulong id)
            : base(id)
        {
        }

        #endregion

        #region Properties

        internal SubSku SubSku {
            get { return subSku; }
            set { subSku = value; }
        }
        
        internal Supplier Supplier
        {
		    get { return supplier; }
		    set { supplier = value; }
	    }

        internal string OrderType {
		    get { return orderType ; }
		    set { orderType = value; }
	    }

        internal DateTime ReleaseDate
        {
            get { return releaseDate; }
            set { releaseDate = value; }
        }

        internal DateTime ReceptionDate
        {
            get { return receptionDate; }
            set { receptionDate = value; }
        }

        internal DateTime ConsumptionDate
        {
            get { return consumptionDate; }
            set { consumptionDate = value; }
        }

        internal Sku Sku
        
        {
		    get { return sku; }
		    set { sku = value; }
	    }

        internal double BackOrderQty {
		    get { return backOrderQty; }
		    set { backOrderQty = value; }
	    }

        internal double BomQty {
		    get { return bomQty; }
		    set { bomQty = value; }
	    }

        internal double OutQty {
		    get { return outQty; }
		    set { outQty = value; }
	    }

        internal double LTFcstQty {
		    get { return lTFcstQty; }
		    set { lTFcstQty = value; }
	    }

        internal double OrderQty {
		    get { return orderQty; }
		    set { orderQty = value; }
        }

        #endregion

        #region Persistence

        internal override Broker GetBroker() { return BrkrMgr.GetInstance().GetBroker((Supply)this); }

        #endregion

        #region ToString override

        public override string ToString() {
            return base.ToString() + " " + supplier.Code + " " + sku.Code + " " + backOrderQty + " " + bomQty + " " + outQty + " " + lTFcstQty + " " + orderQty;
        }

        #endregion


    }
}
