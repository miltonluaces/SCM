#region Imports

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace AibuSet {
    
    internal abstract class Agent : BusObject  {

        #region Fields 

        protected string name;
	    protected string address;
	    protected string city;
	    protected string phone;
        protected string mail;

        #endregion

        #region Constructor
        
        protected Agent() : base() {
	    }

        protected Agent(ulong id)  : base(id)  {
        }
        
        #endregion

        #region Properties

        internal string Name {
		    get { return name; }
		    set { name = value; }
	    }

        internal string Address {
		    get { return address; }
		    set { address = value; }
	    }

        internal string City {
		    get { return city; }
		    set { city = value; }
	    }

        internal string Phone {
		    get { return phone; }
		    set { phone = value; }
	    }

        internal string Mail  {
            get { return mail; }
            set { mail = value; }
        }
        
        #endregion

    }
}
