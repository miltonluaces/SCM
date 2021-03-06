#region Imports

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace AibuSet {

    internal class FcstOverride : DbObject {

     	#region Fields

        private Sku sku;
        private Dictionary<DateTime, double> overrides;

	    #endregion

	    #region Constructors

        internal FcstOverride() { 
        }

        internal FcstOverride(Sku sku, DateTime firstDate, DateTime lastDate) {
            this.sku = sku;
       	    overrides = new Dictionary<DateTime, double>();
	    }

	    #endregion

	    #region Properties

        internal Sku Sku {
            get { return sku; }
            set { sku = value; }
	    }

        internal Dictionary<DateTime, double> Overrides {
		    get { return overrides; }
	    }

	    #endregion

        #region internal Methods

        internal void AddFcstOverride(DateTime date, double value) {
		    if(!overrides.ContainsKey(date)) { overrides.Add(date, value); }
		    else { overrides[date] =  overrides[date] + value; }
    	}
	
	    #endregion

        #region Persistence

        internal override Broker GetBroker() { return BrkrMgr.GetInstance().GetBroker((FcstOverride)this); }

        #endregion

    }
}
