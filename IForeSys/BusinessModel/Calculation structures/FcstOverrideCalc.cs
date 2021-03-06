#region Imports

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace AibuSet {

    internal class FcstOverrideCalc {

        #region Fields

        private Sku sku;
        private List<double> fcst;
        private Dictionary<string, FcstOverride> fovs;
        private DateTime firstDate;
        private int horizon;

        #endregion

        #region Constructor

        internal FcstOverrideCalc() {
        }

        #endregion

        #region Propiertes

        internal Sku Sku {
            get { return sku; }
        }

        internal List<double> Fcst {
            get { return fcst; }
        }

        internal DateTime FirstDate {
            get { return firstDate; }
        }

        internal int Horizon {
            get { return horizon; }
        }

        internal FcstOverride GetFcstOverride(string code) {
            if (!fovs.ContainsKey(code)) { throw new Exception("Error. Fcst Override " + code + " is not registered."); }
            return fovs[code];
        }

        internal List<string> FovCodes {
            get { return fovs.Keys.ToList<string>(); }
        }

        internal Dictionary<string, FcstOverride> Fovs {
            get { return fovs; }
        }
        
        #endregion

        #region internal Methods

        internal void LoadData(Sku sku) {
            this.sku = sku;
            //firstDate
            //load fcst;
            //load fovs;
        }

        internal void AddFcstOverride(string code, DateTime firstDate, DateTime lastDate) {
            if (fovs.ContainsKey(code)) { throw new Exception("Error. " + code + " already used."); }
            FcstOverride fov = new FcstOverride(sku, firstDate, lastDate);
            fov.Code = code;
        }

        internal void AddFcstOverrideValue(string code, DateTime date, double value) { 
            if(!fovs.ContainsKey(code)) { throw new Exception("Error. Fcst Override " + code + " is not registered."); }
            fovs[code].AddFcstOverride(date, value);
        }

        #endregion


    }
}
