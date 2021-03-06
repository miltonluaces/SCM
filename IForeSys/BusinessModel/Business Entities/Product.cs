#region Imports

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace AibuSet {

    internal class Product : BusObject {

        #region Fields

        protected string barcode;
        protected string desc;
	    protected double cost;
	    protected double price;
        protected Supplier supplier;
        protected string supplierCode;
        protected string unit;
        protected string category;
        protected string group;
        protected string division;
  
        #endregion

        #region Constructor

        internal Product()
            : base() {
        }

        internal Product(ulong id)
            : base(id)
        {
        }

        #endregion

        #region Properties


        public string Barcode  {
            get { return barcode; }
            set { barcode = value; }
        }

        public string Desc {
		    get { return desc; }
		    set { desc = value; }
	    }

        internal double Cost {
		    get { return cost; }
		    set { cost = value; }
	    }

        internal double Price {
		    get { return price; }
		    set { price = value; }
        }

        internal Supplier Supplier {
            get { return supplier; }
            set { supplier = value; }
        }

        internal string SupplierCode {
            get { return supplierCode; }
            set { supplierCode = value; }
        }

        public string Unit {
            get { return unit; }
            set { unit = value; }
        }

        public string Category {
            get { return category; }
            set { category = value; }
        }

        public string Group
        {
            get { return group; }
            set { group = value; }
        }

        public string Division
        {
            get { return division; }
            set { division = value; }
        }
        
        #endregion

        #region Persistence 

        internal override Broker GetBroker() { return BrkrMgr.GetInstance().GetBroker((Product)this); }

        #endregion

        #region ToString override

        public override string ToString()  {
            return base.ToString() + " " + desc + " " + cost + " " + price;
        }

        #endregion
    }
}
