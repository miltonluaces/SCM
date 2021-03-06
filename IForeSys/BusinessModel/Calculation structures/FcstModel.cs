#region Imports

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Maths;

#endregion

namespace AibuSet {

    internal class FcstModel : DbObject  {

        #region Fields

        private Sku sku;
        private FcstMethodType fcstMethod;
        private DateTime lastUpdate;
        private string modelStr;
        private char[] sep;

        #endregion
        
        #region Constructors

        internal FcstModel(Sku sku, FcstMethodType fcstMethod) : this() {
            this.sku = sku;
            this.fcstMethod = fcstMethod;
        }
        
        internal FcstModel() : base() {
            sep = new char[3];
            sep[0] = ';';
            sep[1] = ',';
            sep[2] = ' ';
        }

        #endregion

        #region Properties

        public Sku Sku {
            get { return sku; }
            set { sku = value; }
        }

        public FcstMethodType FcstMethod {
            get { return fcstMethod; }
            set { fcstMethod = value; }
        }

        public DateTime LastUpdate  {
            get { return lastUpdate; }
            set { lastUpdate = value; }
        }

        public string ModelStr {
            get { return modelStr; }
            set { modelStr = value; }
        }

        #endregion

        #region Internal Methods

        internal void SetModel(object model)  {
            switch (fcstMethod) {
                case FcstMethodType.Naive: 
                    //do nothing; 
                    break;
                case FcstMethodType.Regression:
                    //do nothing; 
                    break;
                case FcstMethodType.ZChart:
                    //do nothing; 
                    break;
                case FcstMethodType.HoltWinters:
                    //do nothing; 
                    break;
                case FcstMethodType.ARIMA:
                    //do nothing; 
                    break;
                case FcstMethodType.DLMKalman:
                    //do nothing; 
                    break;
                case FcstMethodType.NeuNet:
                    List<List<List<double>>> weights = (List<List<List<double>>>)model;
                    SetNeuNetModel(weights);
                    break;
            }
        }

        internal object GetModel(string modelStr)  {
            switch (fcstMethod)  {
                case FcstMethodType.Naive:
                    return null;
                case FcstMethodType.Regression:
                    return null;
                case FcstMethodType.ZChart:
                    return null;
                case FcstMethodType.HoltWinters:
                    return null;
                case FcstMethodType.ARIMA:
                    return null;
                case FcstMethodType.DLMKalman:
                    return null;
                case FcstMethodType.NeuNet:
                    return GetNeuNetModel(modelStr);
                default: return null;
            }
        }

        #endregion

        #region Private Methods

        private void SetNeuNetModel(List<List<List<double>>> weights) {
            StringBuilder weightsStr = new StringBuilder();
            for (int i = 0; i < weights.Count; i++) {
                for (int j = 0; j < weights[i].Count; j++)  {
                    for (int k = 0; k < weights[i][j].Count; k++) {
                        weightsStr.Append(weights[i][j][k].ToString()); 
                        if(k < weights[i][j].Count-1) { weightsStr.Append(sep[2]); }
                    }
                    if (j < weights[i].Count - 1) { weightsStr.Append(sep[1]); }
                }
                if (i < weights.Count - 1) { weightsStr.Append(sep[0]); }
            }
            modelStr = weightsStr.ToString();
        }

        private List<List<List<double>>> GetNeuNetModel(string modelStr) {
            List<List<List<double>>> weights = new List<List<List<double>>>();
            List<List<double>> hiddenWeights = new List<List<double>>(); weights.Add(hiddenWeights);
            List<List<double>> outputWeights = new List<List<double>>(); weights.Add(outputWeights);
            string[] layers = modelStr.Split(sep[0]);
            string[] hiddenWeightsStr = layers[0].Split(sep[1]);
            foreach (string neuStr in hiddenWeightsStr) {
                List<double> neuWeights = new List<double>(); hiddenWeights.Add(neuWeights);
                string[] wStr = neuStr.Split(sep[2]);
                foreach (string w in wStr) { neuWeights.Add(GetDouble(w)); }
            }
            string[] outputWeightsStr = layers[1].Split(sep[1]);
            foreach (string neuStr in outputWeightsStr) {
                List<double> neuWeights = new List<double>(); outputWeights.Add(neuWeights);
                string[] wStr = neuStr.Split(sep[2]);
                foreach (string w in wStr) { neuWeights.Add(GetDouble(w)); }
            }
            return weights;
        }

        private double GetDouble(string doubleStr)  {
            double value = -1;
            try { value = Convert.ToDouble(doubleStr); }
            catch { value = -1; }
            return value;
        }

        #endregion

        #region Persistence

        internal override Broker GetBroker() { return BrkrMgr.GetInstance().GetBroker((FcstModel)this); }
    
        #endregion
    }
}
