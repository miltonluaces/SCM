#region Imports

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace AibuSet {

    internal class Process : DbObject {

        #region Fields

        private string descr;
        private TypeType type;
        private DateTime lastClosure;
        private TimeSpan start;
        private TimeSpan finish;
        private StateType state;
        private string notes;
        private Process previous;

        #endregion

        #region Constructor

        internal Process() { 
        
        }

        #endregion

        #region Properties

        internal string Descr { 
            get { return descr; }
            set { descr = value; }
        }

        internal TypeType Type {
            get { return type; }
            set { type = value; }
        }

        internal DateTime LastClosure { 
            get { return lastClosure; }
            set { lastClosure = value; }
        }

        internal TimeSpan Start {
            get { return start; }
            set { start = value; }
        }

        internal TimeSpan Finish { 
            get { return finish; }
            set { finish = value; }
        }

        internal StateType State {
            get { return state; }
            set { state = value; }
        }

        internal string Notes {
            get { return notes; }
            set { notes = value; }
        }

        internal Process Previous { 
            get { return previous; }
            set { previous = value; }
        }

        #endregion

        #region Enums

        internal enum TypeType { Interface, TSeriesBuilding, Calculation };
        internal enum StateType { Pending, Ok, Warning, Error };

        #endregion

        #region Persistence

        internal override Broker GetBroker() { return BrkrMgr.GetInstance().GetBroker((Process)this); }

        #endregion

    }
}
