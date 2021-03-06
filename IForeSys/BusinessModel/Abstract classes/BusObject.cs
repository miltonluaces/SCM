#region Imports

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace AibuSet {

    internal abstract class BusObject : DbObject {

        #region Fields

        protected List<string> uStr;
	    protected List<double> uNum;
	    protected List<DateTime> uDate;
        
        #endregion

        #region Constructor

        protected BusObject() : base() {
            uStr = new List<string>();
            uStr.Add("");
            uStr.Add("");
            uStr.Add("");
            uNum = new List<double>();
            uNum.Add(0);
            uNum.Add(0);
            uNum.Add(0);
            uDate = new List<DateTime>();
            uDate.Add(new DateTime(1800,1,1));
            uDate.Add(new DateTime(1800, 1, 1));
            uDate.Add(new DateTime(1800, 1, 1));
        }

        protected BusObject(ulong id) : this() {
		    this.id = id;
        }

        #endregion

        #region Properties

        internal List<string> UStr {
		    get { return uStr; }
		    set { uStr = value; }
	    }

        public string UStr1 {
            get {
                if (uStr == null) { return ""; }
                return uStr[0]; 
            }
            set {
                if (uStr == null) { return; }
                uStr[0] = value; 
            }
        }

        internal string UStr2 {
            get {
                if (uStr == null) { return ""; }
                return uStr[1]; 
            }
            set {
                if (uStr == null) { return; }
                uStr[1] = value;
            }
        }

        internal string UStr3 {
            get {
                if (uStr == null) { return ""; }
                return uStr[2]; 
            }
            set {
                if (uStr == null) { return; }
                uStr[2] = value;
            }
        }

        internal List<double> UNum {
		    get { return uNum; }
		    set { uNum = value; }
	    }

        internal List<DateTime> UDate {
		    get { return uDate; }
		    set { uDate = value; }
        }

        internal void SetUStr(int i, string s) {
            uStr[i-1] = s;
        }

        internal string GetUStr(int i) {
            return uStr[i-1];
        }

        internal void SetUNum(int i, double n) {
            uNum[i-1] = n;
        }

        internal double GetUNum(int i) {
            return uNum[i-1];
        }

        internal void SetUDate(int i, DateTime d) {
            uDate[i-1] = d;
        }

        internal DateTime GetUDate(int i) {
            return uDate[i-1];
        }

        #endregion

    }  
}
