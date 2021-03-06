#region Imports

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace AibuSet {

    internal class PuOrder : BusObject {

        #region Fields

        protected Sku sku;
        protected SubSku subSku;
        protected double qty;
        protected DateTime ordDate;
        protected DateTime rcpDate;
        protected StateType state;
        protected int orderNumber;
        protected int lineNumber;
        protected string productCode;
        protected string supplierCode;
        protected string barcode;

        #endregion
        
        #region Constructor

        internal PuOrder() : base() {
            state = StateType.planned;
            orderNumber = 0;
            lineNumber = 0;
        }

        #endregion
        
        #region Properties

        public string Barcode  {
            get { return barcode; }
            set { barcode = value; }
        }
        
        internal Sku Sku {
		    get { return sku; }
		    set { sku = value; }
	    }

        internal SubSku SubSku {
            get { return subSku; }
            set { subSku = value; }
        }

        internal double Qty
        {
		    get { return qty; }
		    set { qty = value; }
	    }

        internal DateTime OrdDate {
		    get { return ordDate; }
		    set { ordDate = value; }
	    }

        internal DateTime RcpDate {
		    get { return rcpDate; }
		    set { rcpDate = value; }
	    }

        internal StateType State {
		    get { return state; }
		    set { state = value; }
	    }

        internal int OrderNumber {
            get { return orderNumber; }
            set { orderNumber = value; }
        }

        internal int LineNumber {
            get { return lineNumber; }
            set { lineNumber = value; }
        }

        internal string ProductCode {
            get { return productCode; }
            set { productCode = value; }
        }

        internal string SupplierCode {
            get { return supplierCode; }
            set { supplierCode = value; }
        }

        internal string NodeCode {
            get { return code; }
            set { code = value; }
        }
        
        #endregion

        #region ToString override

        public override string ToString() {
            return base.ToString() + " " + sku.Code + " " + qty;
        }

        #endregion

        #region Enums

        internal enum StateType { planned, confirmed, transit };

        #endregion

        #region Persistence

        internal override Broker GetBroker() { return BrkrMgr.GetInstance().GetBroker((PuOrder)this); }

        #endregion
    }
}
