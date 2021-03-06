#region Imports

using System;
using System.Linq;
using System.Collections.Generic;

#endregion

namespace MachineLearning  {

    [Serializable]
	internal class Parameter : ICloneable  {

        #region Fields

        private SvmType svmType;
        private KernelType kernelType;
        private int degree;
        private double gamma;
        private double coefficient0;

        private double cacheSize;
        private double c;
        private double eps;

        private Dictionary<int, double> weights;
        private double nu;
        private double p;
        private bool shrinking;
        private bool probability;

        #endregion

        #region Constructor
        
        internal Parameter()  {
            svmType = SvmType.C_SVC;
            kernelType = KernelType.RBF;
            degree = 3;
            gamma = 0; // 1/k
            coefficient0 = 0;
            nu = 0.5;
            cacheSize = 40;
            c = 1;
            eps = 1e-3;
            p = 0.1;
            shrinking = true;
            probability = false;
            weights = new Dictionary<int, double>();
        }

        #endregion

        #region Properties

        //Type of SVM to perform
        internal SvmType SvmType  {
            get { return svmType; }
            set { svmType = value; }
        }

        internal KernelType KernelType {
            get { return kernelType; }
            set { kernelType = value; }
        }
        
        internal int Degree  {
            get { return degree; }
            set { degree = value; }
        }
        
        // Gamma in kernel function (default 1/k)
        internal double Gamma {
            get { return gamma; }
            set { gamma = value; }
        }
        
        // Zeroeth coefficient in kernel function (default 0)
        internal double Coefficient0  {
            get  { return coefficient0; }
            set  { coefficient0 = value; }
        }
		

        // Cache memory size in MB (default 100)
        internal double CacheSize {
            get { return cacheSize; }
            set { cacheSize = value; }
        }
        
        // Tolerance of termination criterion (default 0.001)
        internal double EPS {
            get { return eps; }
            set { eps = value; }
        }
        
        // The parameter C of C-SVC, epsilon-SVR, and nu-SVR (default 1)
        internal double C {
            get  { return c; }
            set  { c = value; }
        }

        //Contains custom weights for class labels.  Default weight value is 1.
        internal Dictionary<int,double> Weights {
            get{ return weights; }
        }

        // The parameter nu of nu-SVC, one-class SVM, and nu-SVR (default 0.5)
        internal double Nu {
            get { return nu; }
            set { nu = value; }
        }
        
        // The epsilon in loss function of epsilon-SVR (default 0.1)
        internal double P {
            get {  return p; }
            set { p = value; }
        }
        
        // Whether to use the shrinking heuristics, (default True)
        internal bool Shrinking {
            get { return shrinking; }
            set { shrinking = value; }
        }
        
        // Whether to train an SVC or SVR model for probability estimates, (default False)
        internal bool Probability {
            get { return probability; }
            set { probability = value; }
        }

        #endregion

        #region ICloneable Members
        /// <summary>
        /// Creates a memberwise clone of this parameters object.
        /// </summary>
        /// <returns>The clone (as type Parameter)</returns>
        public object Clone()
        {
            return base.MemberwiseClone();
        }

        #endregion
    }

    #region Enums
    //Contains all of the types of SVM this library can model.
    internal enum SvmType { C_SVC, NU_SVC, ONE_CLASS, EPSILON_SVR, NU_SVR };

    // Linear: u'*v Polynomial: (gamma*u'*v + coef0)^degree  Radial basis function: exp(-gamma*|u-v|^2)  Sigmoid: tanh(gamma*u'*v + coef0)   Precomputed kernel
    internal enum KernelType { LINEAR, POLY, RBF, SIGMOID, PRECOMPUTED };

    #endregion

}