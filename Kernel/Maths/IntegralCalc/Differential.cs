using System;
using System.Collections.Generic;
using System.Text;

namespace Maths {
    
    /** Class:  Repository of derivative calculation methods */
    internal class Differential {

        #region Constructor 

        internal Differential() {

        }

        #endregion 

        #region internal Methods

        /** Method:  Get the derivative of any order, of an array of data (Y) with the corresponding independent variable (X) 
        X - independent variable
        Y - data (dependent variable)
        order - derivative order
        list of calculated data */
        internal List<double> GetDerivative(List<double> X, List<double> Y, int order) {
            if(X.Count != Y.Count) { throw new Exception("must have the same size"); }

            List<double> D = Y;
            for(int i=0;i<order;i++) {
                D = GetDerivative(X, D);
            }
            return D;
        }

        /** Method:  Get the derivative of any order, of an array of data (Y) 
        Y - data (dependent variable)
        order - derivative order
        list of calculated data */
        internal List<double> GetDerivative(List<double> Y, int order) {
            
            List<double> D = Y;
            for(int i=0;i<order;i++) {
                D = GetDerivative(D);
            }
            return D;
        }

        #endregion 

        #region Private Methods

        private List<double> GetDerivative(List<double> X, List<double> Y) {
            if(X.Count != Y.Count) { throw new Exception("must have the same size"); }

            List<double> deriv = new List<double>();
            double diffQuotient = 0.0;
            for(int i=0;i<X.Count-1;i++) {
                diffQuotient = (Y[i+1] - Y[i]) / (X[i+1] - X[i]);
                deriv.Add(diffQuotient);
            }
            deriv.Add(diffQuotient);
            return deriv;
        }
        
        private List<double> GetDerivative(List<double> Y) {
            
            List<double> deriv = new List<double>();
            double diffQuotient = 0.0;
            for(int i=0;i<Y.Count-1;i++) {
                diffQuotient = (Y[i+1] - Y[i]);
                deriv.Add(diffQuotient);
            }
            deriv.Add(diffQuotient);
            return deriv;
        }

        #endregion

    }
}
