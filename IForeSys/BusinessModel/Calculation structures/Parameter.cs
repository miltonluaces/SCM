#region Imports

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

#endregion


namespace AibuSet {

    internal class Parameter : DbObject {

        #region Fields

        private string descr;
        private ParType type;
        private object val;

        #endregion

        #region Constructor

        internal Parameter()
            : base() {
        }

        internal Parameter(ulong id, string code, string descr, ParType type, object value)
            : base(id) {
            this.code = code;
            this.descr = descr;
            this.type = type;
            this.val = value;
        }
        
        #endregion

        #region Properties

        internal string Descr {
            get { return descr; }
            set { descr = value; }
        }

        internal ParType Type {
            get { return type; }
            set { type = value; }
        }

        internal object Value {
            get { return val; }
            set { val = value; }
        }

        #endregion

        #region internal Methods

        internal static Parameter.ParType GetParType(string typeStr) {
            switch (typeStr) {
                case "S": return Parameter.ParType.String;
                case "B": return Parameter.ParType.Boolean;
                case "I": return Parameter.ParType.Integer;
                case "R": return Parameter.ParType.Real;
                case "D": return Parameter.ParType.Date;
                default: throw new Exception("Error. Not found");
            }
        }

        internal static string GetParTypeStr(Parameter.ParType type) {
            switch (type) {
                case Parameter.ParType.String: return "S";
                case Parameter.ParType.Boolean: return "B";
                case Parameter.ParType.Integer: return "I";
                case Parameter.ParType.Real: return "R";
                case Parameter.ParType.Date: return "D";
                default: throw new Exception("Error. Not found");
            }
        }

        internal static object GetParValue(ParType type, string valStr) {
            switch (type) {
                case ParType.String: return valStr;
                case ParType.Boolean: return Convert.ToBoolean(valStr);
                case ParType.Integer: return Convert.ToInt32(valStr);
                case ParType.Real: return Convert.ToDouble(valStr);
                case ParType.Date: return DateTime.ParseExact(valStr, "yyyyMMdd", CultureInfo.InvariantCulture);
                default: throw new Exception("Error. Not found");
            }
        }

        internal static string GetParValueStr(ParType type, object val) {
            switch (type) {
                case ParType.String: return val.ToString();
                case ParType.Boolean: return val.ToString();
                case ParType.Integer: return val.ToString();
                case ParType.Real: return val.ToString();
                case ParType.Date: return ((DateTime)val).ToShortDateString();
                default: throw new Exception("Error. Not found");
            }
        }

        internal object GetValue() {
            switch (type) {
                case ParType.String: return val.ToString();
                case ParType.Boolean: return val.ToString();
                case ParType.Integer: return val.ToString();
                case ParType.Real: return val.ToString();
                case ParType.Date: return ((DateTime)val).ToShortDateString();
                default: throw new Exception("Error. Not found");
            }
        }
        
        #endregion

        #region Type enum

        internal enum ParType { String, Boolean, Integer, Real, Date };

        #endregion

        #region Persistence

        internal override Broker GetBroker() { return BrkrMgr.GetInstance().GetBroker((Parameter)this); }
        
        #endregion

    }
}
