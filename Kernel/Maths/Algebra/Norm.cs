#region Imports

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Maths;

#endregion

namespace Maths {
    
    internal class Norm {

        #region Fields
        
        private double xMin=0; 
        private double xMax=0;
        private double xRange=0;
        private double factor=1;

        #endregion
    
        #region Constructors
        
        internal Norm(AR X) : this(X, 1, 0, 1) {
        }
      
        internal Norm(AR X, double factor, double minFactor, double maxFactor) {
            this.factor=factor;
            double min = double.MaxValue;
            double max = double.MinValue;
            for (int i = 1; i < X.Length; i++) {
                if (X[i] < min) { min = X[i]; }
                if (X[i] > max) { max = X[i]; }
            }
            this.xMin = min * (1 - minFactor); 
            this.xMax = max * (1 + maxFactor);  
            this.xRange = this.xMax - this.xMin;
            if(this.xRange == 0) { this.xRange = this.xMax; }
        }
        
        #endregion

        #region Properties
        
        internal double XMin { get { return xMin; } set { xMin = value; } }
        internal double XMax { get { return xMax; } set { xMax = value; } }
        internal double XRange { get { return xRange; } set { xRange = value; } }
        
        #endregion

        #region Public Methods

        internal double Normalize(double x) { 
            double nx = (x - this.xMin)/this.xRange;
            double fnx =  nx * this.factor;
            return fnx;
        }

        internal double UnNormalize(double nx) { 
            double x = (nx/this.factor) * this.xRange + this.xMin;
            return x;
        }

        internal AR Normalize(AR X) {
            AR nX = (X - this.xMin) / this.xRange;
            AR fnX = nX * this.factor;
            return fnX;
        }

        internal AR UnNormalize(AR nX) {
            AR X = (nX / this.factor) * this.xRange + this.xMin;
            return X;
        }

        #endregion
    }
}
