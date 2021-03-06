#region Imports

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace AibuSet {

    internal abstract class Item : DbObject {

        #region Fields

        protected double qty;
	    protected Demand demand;
	    protected DateTime consumptionDate;
	    protected string allocType; //type

        #endregion

        #region Constructor

        internal Item()
            : base() {
        }

        #endregion

        #region Properties

        internal double Qty {
		    get { return qty; }
		    set { qty = value; }
	    }

        internal Demand Demand {
		    get { return demand; }
		    set { demand = value; }
	    }

        internal DateTime ConsumptionDate {
		    get { return consumptionDate; }
		    set { consumptionDate = value; }
	    }

        internal string AllocType {
		    get { return allocType; }
		    set { allocType = value; }
	    }  //type

        #endregion

    }

}
