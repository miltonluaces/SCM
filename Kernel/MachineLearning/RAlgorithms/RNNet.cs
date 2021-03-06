#region Imports

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RDotNet;
using Maths;

#endregion

namespace MachineLearning {
    
    internal class RNNet {

        #region Fields

        private RNet rNet;
        private SymbolicExpression nn;
        
        #endregion

        #region Constructor

        internal RNNet(string path, string version) {
            this.rNet = new RNet(path, version);
            rNet.LoadLibrary("nnet");
        }

        #endregion

        #region Internal Methods

        #region RNet Methods

        internal void CreateNN(DF X, AR Y, int nHidden, double dec, int maxIt) {
            IList<RNet.Params> pars = new List<RNet.Params>();
            pars.Add(new RNet.Params("X", RNet.ParamType.Dataframe, X));
            pars.Add(new RNet.Params("Y", RNet.ParamType.NumericVector, Y));
            pars.Add(new RNet.Params("nHidden", RNet.ParamType.Numeric, nHidden));
            pars.Add(new RNet.Params("dec", RNet.ParamType.Numeric, dec));
            pars.Add(new RNet.Params("maxIt", RNet.ParamType.Numeric, maxIt));
            rNet.Execute("nnet(X, Y, size=nHidden, linout=T, trace=F, decay=dec, range=0.5, maxIt=maxIt)", pars);
            nn = rNet.GetReturn();
            //rNet.LoadObject("nneT");
        }

        internal double Predict(AR x) {
            IList<RNet.Params> pars = new List<RNet.Params>();
            pars.Add(new RNet.Params("nn", RNet.ParamType.SymbolicExpression, nn));
            pars.Add(new RNet.Params("x", RNet.ParamType.NumericVector, x));
            rNet.Execute("predict(nn, x)", pars);
            double y = rNet.GetDoubleReturn()[0];
            return y;
        }

        #endregion

        #endregion
    }
}
