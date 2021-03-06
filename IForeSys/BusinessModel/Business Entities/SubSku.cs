#region Imports

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace AibuSet  {
  
    internal class SubSku : DbObject {

        #region Fields

        private Sku sku;
        private string barcode;
        private string category;
        private double freq;
        private double normFreq;
        private double dmd;
        private DateTime updated;

        #endregion

        #region Constructors

        internal SubSku() {
            this.id = 0;
        }

        internal SubSku(ulong id) {
            this.id = id;
        }
        
        internal SubSku(ulong id, string code, Sku sku) {
            this.sku = sku;
            this.id = id;
            this.code = code;
            this.freq = 0;
            this.dmd = 0;
            this.updated = DateTime.Now;
        }

        #endregion

        #region Properties

        internal Sku Sku {
            get { return sku; }
            set { sku = value; }
        }
        
        internal string Barcode {
            get { return barcode; }
            set { barcode = value; }
        }

        internal string Category {
            get { return category; }
            set { category = value; }
        }
        
        internal double Freq {
            get { return freq; }
            set { freq = value; }
        }

        internal double NormFreq {
            get { return normFreq; }
            set { normFreq = value; }
        }

        internal double Dmd {
            get { return dmd; }
            set { dmd = value; }
        }

        internal DateTime Updated {
            get { return updated; }
            set { updated = value; }
        }
      
        #endregion

        #region GetSetters

        internal double GetPercFreq() { return Math.Round(freq * 100, 2); }

        #endregion

        #region Persistence

        internal override Broker GetBroker() { return BrkrMgr.GetInstance().GetBroker((SubSku)this); }

        #endregion
    }
}
