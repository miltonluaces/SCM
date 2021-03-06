#region Imports

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace AibuSet {

    internal class Node : Agent, ITTItem {

        #region Fields

        protected string descrip;
        protected Calendar calendar;
        protected int topOrder;
	    protected int schLevel;
	    protected bool aggrHist;
	    protected bool aggrFcst;
        protected Node rootNode;
        protected bool[] orderDays;
        protected int weeklyFreq;

        #endregion
        
        #region Constructor

        internal Node() : base() {
            topOrder = 0;
        }

        internal Node(ulong id)  : base(id) {
            topOrder = 0;
        }

        #endregion
        
        #region Properties

        internal string Descrip {
		    get { return descrip; }
		    set { descrip = value; }
	    }

        internal Calendar Calendar {
            get { return calendar; }
            set { calendar = value; }
        }

        internal int SchLevel {
		    get { return schLevel; }
		    set { schLevel = value; }
	    }

        internal int TopOrder {
            get { return topOrder; }
            set { topOrder = value; }
        }

        internal bool AggrHist {
		    get { return aggrHist; }
		    set { aggrHist = value; }
	    }

        internal bool AggrFcst {
		    get { return aggrFcst; }
		    set { aggrFcst = value; }
        }

        internal Node RootNode {
            get { return rootNode; }
            set { rootNode = value; }
        }

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

        internal override Broker GetBroker() { return BrkrMgr.GetInstance().GetBroker((Node)this); }

        #endregion

        #region ToString override

        public override string ToString() {
            return this.id.ToString();
            //return base.ToString() + " " + descrip + " " + calendar.Code + " " + topologOrder + " " + (aggrHist? "aggrHist": "noAggrHist") + (aggrFcst? "aggrFcst": "noAggrFcst");
        }

        #endregion

        #region ITTItem interface implementation

        string ITTItem.GetName() { return descrip; }
        void ITTItem.SetOrderDays(bool[] orderDays) { this.orderDays = orderDays; }
        bool[] ITTItem.GetOrderDays() { return orderDays; }
        void ITTItem.SetWeeklyFreq(int weeklyFreq) { this.weeklyFreq = weeklyFreq; }
        int ITTItem.GetWeeklyFreq() { return weeklyFreq; }

        #endregion
    }
}
