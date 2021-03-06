#region Imports

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace AibuSet {

    internal abstract class Note : BusObject
    {

        #region Fields

        private int orderId;
        private int lineId;
        private double initialQty;
        private double actualQty;
        private double pendingQty;

        #endregion
        
        #region Constructor

        protected Note() : base() {
        }

        protected Note(ulong id)
            : base(id)
        {
        }

        #endregion
        
        #region Properties

        internal int OrderId {
            get { return orderId; }
            set { orderId = value; }
        }

        internal int LineId {
            get { return lineId; }
            set { lineId = value; }
        }

        internal double InitialQty {
            get { return initialQty; }
            set { initialQty = value; }
        }

        internal double ActualQty {
            get { return actualQty; }
            set { actualQty = value; }
        }

        internal double PendingQty {
            get { return pendingQty; }
            set { pendingQty = value; }
        }

        #endregion

    }
}
