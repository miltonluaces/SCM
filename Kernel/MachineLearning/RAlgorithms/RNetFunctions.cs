#region Imports

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RDotNet;
using Maths;

#endregion

namespace MachineLearning {
    
    internal class RNetFunctions {

        #region Fields
        
        private RNet rnet;

        #endregion

        #region Constructor

        internal RNetFunctions(string path, string version) {
            rnet = new RNet(path, version);
        }

        #endregion

        #region Internal Methods

         internal double WicoxTest(AR X, AR Y) {
            IList<RNet.Params> pars = new List<RNet.Params>();
            pars.Add(new RNet.Params("X", RNet.ParamType.NumericVector, X));
            pars.Add(new RNet.Params("Y", RNet.ParamType.NumericVector, Y));
            rnet.Execute("wilcox.test(X, Y)$p.value", pars);
            return rnet.GetDoubleReturn()[0];
         }
           
        #endregion
    }
}
