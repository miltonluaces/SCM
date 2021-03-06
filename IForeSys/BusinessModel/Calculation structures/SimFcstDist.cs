#region Imports

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace AibuSet {

    internal class SimFcstDist : DbObject {

        #region Fields

        private Sku sku;
        private Dictionary<int, double> percentiles;
        private double lower;
        private double step;
        private int factor;

        #endregion

        #region Constructors

        internal SimFcstDist()
            : base() {
            this.lower = 80;
            this.step = 0.5;
            this.factor = 2;
        }

        internal SimFcstDist(ulong id, string code, Sku sku, double lower, double step)
            : base(id) {
            this.code = code;
            this.sku = sku;
            this.lower = lower;
            this.step = step;
            this.factor = (int)(1.0 / step);
            this.percentiles = new Dictionary<int, double>();
        }

        #endregion

        #region Properties, Setters & Getters

        internal Sku Sku {
            get { return sku; }
            set { sku = value; }
        }

        internal double Lower {
            get { return lower; }
            set { lower = value; }
        }

        internal double Step {
            get { return step; }
            set { step = value; }
        }

        internal Dictionary<int, double> Percentiles {
            get { return percentiles; }
            set { percentiles = value; }
        }

        internal void AddPercentil(double prob, double perc) {
            if (prob < lower || prob > 100) { throw new Exception("Error. Out of range"); }
            int probKey = GetKey(prob);
            if (!percentiles.ContainsKey(probKey)) { percentiles.Add(probKey, perc); }
            else { percentiles[probKey] = perc; }
        }

        internal List<double> GetProbs() {
            List<double> probs = new List<double>();
            double p = lower;
            while (p <= 100) {
                probs.Add(p);
                p += step;
            }
            return probs;
        }

        internal List<double> GetPercs() {
            List<double> percs = new List<double>();
            if (percentiles == null) { return percs;}
            double p = lower;
            while (p <= 100) {
                percs.Add(GetPercentil(p));
                p += step;
            }
            return percs;
        }

        #endregion

        #region internal Methods

        internal double GetPercentil(double prob)
        {
            int probKey = GetKey(prob);
            if (!percentiles.ContainsKey(probKey)) { return -1; }
            return percentiles[probKey];
        }

        internal double GetProbability(double perc)
        {
            //busqueda por biseccion
            return -1;
        }

        #endregion

        #region Private Methods

        private int GetKey(double probValue)  {
            return (int)(Math.Round(probValue * factor));
        }

        private double GetValue(int probKey)  {
            return (double)probKey / (double)factor;
        }

        #endregion

        #region ToString override

        public override string ToString()  {
            List<double> probs = GetProbs();
            List<double> percs = GetPercs();
            string str = "Prob\tPerc\n\n";
            for(int i=0;i<probs.Count;i++) {
                str += probs[i].ToString("0.0") + "\t" + percs[i].ToString("0.00") + "\n";
            }
            return str;
        }

        #endregion

        #region Persistence

        internal override Broker GetBroker() { return BrkrMgr.GetInstance().GetBroker((SimFcstDist)this); }

        internal string GetString() {
            if (percentiles == null) { return ""; }
            StringBuilder dataStr = new StringBuilder();
            double p = lower;
            int key;
            while (p <= 100)  {
                key = GetKey(p);
                dataStr.Append(percentiles[key] + " ");
                p += step;
            }
            return dataStr.ToString();
        }

        internal Dictionary<int, double> GetValues(string percentilesStr) {
            char[] sep = { ' ' };
            Dictionary<int, double> percentiles = new Dictionary<int, double>();
            string[] tokens = percentilesStr.Split(sep);
            int key;
            double p = lower;
            double perc;
            for (int i = 0; i < tokens.Length; i++) {
                if (tokens[i] == "") { continue; }
                perc = Convert.ToDouble(tokens[i]);
                key = GetKey(p);
                percentiles.Add(key, perc);
                p += step;
            }
            return percentiles;
        }


        #endregion
    }
 }
