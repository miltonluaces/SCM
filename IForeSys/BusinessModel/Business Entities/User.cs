#region Imports

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace AibuSet  {

    internal class User : BusObject {

        #region Fields

        private string name;
        private string password;
        private List<Access> accesses; 

        #endregion

        #region Constructors

        internal User() { 
        
        }
        
        internal User(ulong id) {
            this.id = id;
        }

        internal User(ulong id, string code, string name, string password) {
            this.id = id;
            this.code = code;
            this.name = name;
            this.password = password;
            this.accesses = new List<Access>();
        }
        
        #endregion

        #region Properties

        internal string Name {
            get { return name; }
            set { name = value; }
        }

        internal string Password {
            get { return password; }
            set { password = value; }
        }

        internal List<Access> Accesses {
            get { return accesses; }
        }

        #endregion

        #region internal Methods

        internal void AddAccess(Access a) {
            accesses.Add(a);
        }
        
        #endregion

        #region Enums

        internal enum Access { dataBase, logStruct, dataAnal, advance, batch, backup, personal, planning, report, tools, update, variables }

        #endregion
    }
}
