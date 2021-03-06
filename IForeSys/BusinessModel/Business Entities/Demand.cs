#region Imports

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace AibuSet {

    internal class Demand : Note {

        #region Fields

        protected Sku sku;
        protected SubSku subSku;
        protected DateTime desiredDate;
        protected double orderPrice;
        protected Customer customer;

        #endregion

        #region Constructor

        internal Demand() : base() {
            subSku = new SubSku();
        }

        internal Demand(ulong id) : base(id) {
        }

        #endregion

        #region Properties

        internal Sku Sku {
            get { return sku; }
            set { sku = value; }
        }

        internal SubSku SubSku {
            get { return subSku; }
            set { subSku = value; }
        }

        internal DateTime DesiredDate
        {
            get { return desiredDate; }
            set { desiredDate = value; }
        }

        internal double OrderPrice {
            get { return orderPrice; }
            set { orderPrice = value; }
        }

        internal Customer Customer {
            get { return customer; }
            set { customer = value; }
        }

        #endregion

        #region Persistence

        internal override Broker GetBroker() { return BrkrMgr.GetInstance().GetBroker((Demand)this); }

        #endregion

        #region ToString override

        public override string ToString()  {
            return base.ToString() + " " + desiredDate + " " + orderPrice + " ";
        }

        #endregion

    }
}
