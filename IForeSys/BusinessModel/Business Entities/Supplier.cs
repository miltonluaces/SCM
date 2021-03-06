#region Imports

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace AibuSet {

    internal class Supplier : Agent, ITTItem {

        #region Fields

        private bool[] orderDays;
        private int weeklyFreq;

        #endregion

        #region Constructor

        internal Supplier()
            : base() {
            orderDays = new bool[] { true, true, true, true, true, false, false };
            weeklyFreq = 1;
        }

        internal Supplier(ulong id)
            : base(id) {
        }

        #endregion

        #region Properties

        internal bool[] OrderDays {
            get { return orderDays; }
            set { orderDays = value; }
        }

        internal int WeeklyFreq {
            get { return weeklyFreq; }
            set { weeklyFreq = value; }
        }
        
        #endregion

        #region Persistence

        internal override Broker GetBroker() { return BrkrMgr.GetInstance().GetBroker((Supplier)this); }

        #endregion

        #region ToString override

        public override string ToString()  {
            return base.ToString();
        }

        #endregion

        #region ITTItem interface implementation

        string ITTItem.GetName() { return name; }
        void ITTItem.SetOrderDays(bool[] orderDays) { this.orderDays = orderDays; }
        bool[] ITTItem.GetOrderDays() { return orderDays; }
        void ITTItem.SetWeeklyFreq(int weeklyFreq) { this.weeklyFreq = weeklyFreq; }
        int ITTItem.GetWeeklyFreq() { return weeklyFreq; }

        #endregion
    }
}
