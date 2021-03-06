#region Imports

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace BusinessModel  {

    public class User : BusObject {

        #region Fields

        private string name;
        private string password;
        private List<Access> accesses; 

        #endregion

        #region Constructor

        public User() { 
        
        } 
        
        #endregion

        #region Properties

        public string Name {
            get { return name; }
            set { name = value; }
        }

        #endregion
        
        #region Enums

        public enum Access { dataBase, logStruct, dataAnal, advance, batch, backup, personal, planning, report, tools, update, variables }

        #endregion
    }
}
