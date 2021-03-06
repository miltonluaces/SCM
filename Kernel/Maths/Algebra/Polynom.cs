#region Imports

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

#endregion


namespace Maths {

    /** Method:  Class for polynomic operations */
    internal class Polynom {

        #region Constructor

        /** Method:  Constructor */
        internal Polynom() { 
        }
        
        #endregion

        #region Polynomic Regression

        /** Method: Regression
        Regresion cuadratica
        a0 + a1x + a2x2   -  Sistema : AU = Y
        
        alX - independent variable 
        alY - dependent variable
        grade - grade of the polynom */
        internal List<double> Regression(List<double> alX, List<double> alY, int grade) {
            double[][] datosA = new double[alX.Count][];
            for(int i=0;i<alX.Count;i++) {
                datosA[i] = new double[grade+1];
                datosA[i][0] =  1;
                datosA[i][1] = Convert.ToDouble(alX[i]);
                for(int g=2;g<=grade;g++) {
                    datosA[i][g] = Math.Pow(datosA[i][1], g);
                }
            }
            double[][] datosY = new double[alY.Count][];
            for(int i=0;i<alY.Count;i++) {
                datosY[i] = new double[1];
                datosY[i][0] = (double)alY[i];
            }

            Matrix A = new Matrix(datosA);
            Matrix Y = new Matrix(datosY);

            IMatrix X = ((IMatrix)A).Solve(Y);
            List<double> solucion = new List<double>();
            for(int i=0;i<X.Rows;i++) {
                solucion.Add(X[i, 0]);
            }
            return solucion;
        }

        /** Method:  Get Regression values
        X - independent variable 
        coeffs - list of coefficents*/
        internal List<double> GetRegValues(List<double> X, List<double> coeffs) {
            List<double> regValues = new List<double>();
            double y;
            foreach(double x in X) {
                y = 0.0;
                for(int i=0;i<coeffs.Count;i++) {
                    y += coeffs[i] * Math.Pow(x,i);
                }
                regValues.Add(y);
            }
            return regValues;
        }


        /** Method:  Get Regression value 
       X - independent variable 
        coeffs - list of coefficents */
        internal double GetRegValue(double x, List<double> coeffs) {
            double y = 0.0;
            for(int i=0;i<coeffs.Count;i++) { y += coeffs[i] * Math.Pow(x, i);  }
            return y;
        }
        
        #endregion

        #region M Regresion 

        /*
        internal Sttatic List<double> GetResiduals(List<double> X, List<double> Y, List<double> coeffs) {
            if(X.Count != Y.Count) { throw new Exception("X and Y must have the same size.");  }

            List<double> residuals = new List<double>();
            List<double> regValues = GetRegValues(X, coeffs);
            for(int i=0;i<Y.Count;i++) {
                residuals.Add(y[i] - regValues[i]);
            }
            return residuals;
        }

        internal List<double> BmReg(List<double> xList, List<double> yList, int iter, double bend) {

            //conversion de listas a matrices
            Matrix x = new Matrix(xList.Count, 1);
            for(int i=0;i<xList.Count;i++) { x[0, i] = xList[i]; }
            Matrix y = new Matrix(yList.Count, 1);
            for(int i=0;i<yList.Count;i++) { y[0, i] = yList[i]; }
            
            //valores por defecto
            if(iter == -1) { iter = 20; }
            if(bend == -1) { bend = 2 * Math.Sqrt(x.Columns + 1)/ x.Rows; }

            //calculo de coeficientes y residuo inicialº
            List<double> initCoeffs = Regression(xList, yList, 1);
            List<double> residuals = GetResiduals(xList, yList, initCoeffs);
            
            
            //parametros
            Matrix x1 = CBind(x, 1);
            double nu = Math.Sqrt(1 - Hat(x1));
            int low = x.Columns + 1;
            double eps = 0.0001;
            double maxAbs = 0.0;
            double newResid, wt;
            List<double> newCoeffs = new List<double>();
            List<double> ev = new List<double>();
            List<double> rov;

            //bucle principal
            for(int i=0;i<iter;i++) {
                
                ev.Add(Math.Abs(resid));
                ev.Sort();
    
                //calculo nuevo ajuste
                List<double> scale = 0.0;
                List<double> vals = new List<double>();
                for(int i=low;i<=yList.Count;i++) { vals.Add(ev[i]); }
                vals.Sort();
                double median = vals[vals.Count/2];
                scale = median/InvNormal_acum(0.75);  //scale = Median(ev[c(low:length(y))])/qnorm(0.75);
                rov = (resid/scale)/nu; 
                double psi = (Math.Abs(rov) <= bend)?  rov : bend * Math.Sign(rov);
                List<double> wt = new List<double>();
                for(int i=0;i<residuals.Count;i++) { wt.Add(nu * psi / (residuals[i]/scale)); }
                newCoeffs = Regression(xList, yList, wt);
                newResid = GetResiduals(xList, yList, newCoeffs);
                
                //criterio de parada: maxima diferencia < eps
                double abs;
                for(int j=0;j<initCoeffs.Count;j++) {
                    abs = Math.Abs(newCoeffs[j] - initCoeffs[j]);
                    if(abs > maxAbs) { maxAbs = abs; }
                }
                if(maxAbs < eps) { break; }
            
                //siguiente paso iterativo
                initCoeffs = newCoeffs;
                resid = newResid;
            }

            //obtención del nuevo residuo y newCoeffs
            IMatrix yMenosX1 = y.Subtraction(x1);
            IMatrix newCoeffsMat = ToMatrix(newCoeffs);
            IMatrix residMat = yMenosX1.Multiply(newCoeffsMat);
            resid = residMat.Toscalar();
           
            if (maxAbs >= eps) { throw new Exception("failed to converge in" + iter + "steps"); }
            
            //coefficents	
            return newCoeffs;
        }

        private double LsFitResiduals(List<double> x, List<double> y, List<double> coeffs) {
            
            double residuals = 0.0;
            return residuals;
        }

        private List<double> LsFitCoeffs(Matrix x, Matrix y, double wt) {
            List<double> coeffs = new List<double>();
            return coeffs;
        }

        private List<double> LsFitCoeffs(Matrix x, Matrix y) {
            List<double> coeffs = new List<double>();
            return coeffs;
        }

        private List<double> LsFitCoeffs(Matrix y) {
            List<double> coeffs = new List<double>();
            return coeffs;
        }

        private double LsFitResiduals(Matrix x, Matrix y, double wt) {
            double resid = 0.0;
            return resid;
        }

        private double Hat(Matrix x) {
            double hat = -1;
            x = CBind(x, 1);
            n = x.Rows;
            IQrDecomposition q = x.GetQrDecomposition();
            Matrix qr = q.UpperTriangularFactor.Addition(q.OrthogonalFactor);
            x = qr;
            //apply(qr.qy(x, diag(1, nrow = n, ncol = x$rank))^2, 1, sum)
            return hat;
        }

        private double Qy(Matrix qrqr, Matrix y) { 
            //if (!is.qr(qr)) stop("argument is not a QR decomposition")
            
            int n = qrqr.Rows;                  //n<- as.integer(nrow(qr$qr))
            int p = qrqr.Columns;               //p <- as.integer(ncol(qr$qr))
            int df = qrRank;                     //df <- as.integer(qr$rank)
            double ny = (double)y.Columns;      //ny <- as.integer(NCOL(y)) storage.mode(y) <- "double"
            if(y.Rows != n) { throw new Exception("qr and y must have the same number of rows"); }
            
            Matrix qy = new Matrix(y.Rows, y.Columns);
            for(int i=0;i<y.Rows;i++) {
                for(int j=0;j<y.Columns;j++) {
                    qy[i,j] = n * ny;
                }
            }
 
            //.Fortran("dqrqy", as.double(qr$qr), n, df, as.double(qr$qraux),y, ny, qy = qy, PACKAGE = "base")$qy
        }
        
        private Matrix CBind(Matrix x, double val) {
            Matrix binded = new Matrix(x.Rows, x.Columns + 1);
            for(int i=0;i<x.Rows;i++) {
                binded[i, 0] = x[i, 0];
                binded[i, 1] = val;
            }
            return binded;
        }

        private IMatrix ToMatrix(List<double> x) {
            IMatrix xMat = new Matrix(x.Count, 1);
            for(int i=0;i<x.Count;i++) { xMat[i, 0] = x[i]; }
            return xMat;
        }

        internal Sttatic double InvNormal_acum(double p) {
            double q, r;

            if(p < 0 || p > 1) {
                return 0.0;
            } 
            else if(p == 0) {
                return 999999999;
            } 
            else if(p == 1) {
                return 999999999;
            }
            else if(p < LOW) {
                //Rational approximation for lower region 
                q = Math.Sqrt(-2*Math.Log(p));
                return (((((c[0]*q+c[1])*q+c[2])*q+c[3])*q+c[4])*q+c[5]) /
					((((d[0]*q+d[1])*q+d[2])*q+d[3])*q+1);
            } 
            else if(p > HIGH) {
                //Rational approximation for upper region 
                q  = Math.Sqrt(-2*Math.Log(1-p));
                return -(((((c[0]*q+c[1])*q+c[2])*q+c[3])*q+c[4])*q+c[5]) /
					((((d[0]*q+d[1])*q+d[2])*q+d[3])*q+1);
            } 
            else {
                //Rational approximation for central region 
                q = p - 0.5;
                r = q*q;
                return (((((a[0]*r+a[1])*r+a[2])*r+a[3])*r+a[4])*r+a[5])*q /
					(((((b[0]*r+b[1])*r+b[2])*r+b[3])*r+b[4])*r+1);
            }
        }
        */

        #endregion

        #region Weighted regression


        /** Method:  Weighted Least Square Reg Known weights 
        X -  independent variable
        Y -  dependent variable
        W -  list of weights */
        internal Result WLSRegression(List<double> X, List<double> Y, List<double> W) {
            if(X.Count != Y.Count) { throw new Exception("X and Y must have the same size"); }
            if(X.Count != W.Count) { throw new Exception("X and W must have the same size"); }

            int n = X.Count;
            Result res = new Result(n, W);

            int i;
            double ss = 0.0;
            double sx = 0.0;
            double sy = 0.0;
            double mx, t;
            double st2 = 0.0;

            for(i=0;i<n;i++) {
                ss += res.W[i];
                sx += X[i] * res.W[i];
                sy += Y[i] * res.W[i];
            }
            mx = sx/ss;

            res.c1 = 0.0;

            for(i=0;i<n;i++) {
                t = X[i] - mx;
                st2 += Math.Pow(t,2) * res.W[i];
                res.c1 += t * Y[i] * res.W[i];
            }

            res.c1 /= st2;
            res.c0 = (sy - sx * res.c1) / ss;

            res.sig0 = Math.Sqrt((1.0 + sx * sx / (n * st2)) / ss);
            res.sig1 = Math.Sqrt(1.0 / st2);

            res.chi2 = 0.0;
            for(i=0;i<n;i++) {
                t = Y[i] - res.c0 - res.c1 * X[i];
                res.chi2 += Math.Pow(t,2) * res.W[i];
            }
            return res;
        }


        /** Method:   Weighted Least Square Reg Unknown weights 
              X -  independent variable
              Y -  dependent variable */
        internal Result WLSRegression(List<double> X, List<double> Y) {
            if(X.Count != Y.Count) { throw new Exception("X and Y must have the same size"); }

            int n = X.Count;
            Result res = new Result(n);
            res = WLSRegression(X, Y, res.W);

            res.ResetW();

            double fit, result, err;
            double max = 0.0;

            //find the datum with the largest abs(residual) 
            for(int i=0;i<n;i++) {
                fit = X[i] * res.c1 + res.c0;
                result = Y[i] - fit;
                err = Math.Abs(result);
                res.W[i] = err;
                if(res.W[i] > max) { max = res.W[i]; }
            }

            //scale the weights so that the most distant outlier is ignored altogether, and other weights are proportional to error 
            for(int i=0;i<n;i++) { res.W[i] = 1.0 - res.W[i]/max; }

            //use the calculated weights
            return WLSRegression(X, Y, res.W);
        }

        /** Method:  Weighted Least Square Reg Unknown weights only Y values */
        internal Result WLSRegression(List<double> Y) {
            List<double> X = new List<double>();
            for(int i=0;i<Y.Count;i++) {
                X.Add((double)i);
            }
            return WLSRegression(X, Y);
        }


        /** Method:  Least Squared Regression with unknown weights (only Y values) */
        internal Result LSRegression(List<double> Y) {
            List<double> X = new List<double>();
            for(int i=0;i<Y.Count;i++) {
                X.Add((double)i);
            }
            List<double> coeffs = Regression(X, Y, 1);
            Result res = new Result(Y.Count);
            res.c0 = coeffs[0];
            res.c1 = coeffs[1];
            return res;
        }

        #endregion

        #region Polynoms derivatives

        /** Method:  Derive from a list of coefficents */
        private List<double> Derive(List<double> coeffs) {
            List<double> derived = new List<double>();
            for(int i=1;i<coeffs.Count;i++) {
                derived.Add(i * coeffs[i]);
            }
            return derived;
        }

        /** Method:  Derive on any order from a list of coefficents 
        coeffs - list of coefficents 
        order - derivative order */
        internal List<double> Derive(List<double> coeffs, int order) {
            List<double> derived = new List<double>();
            for(int i=0;i<order;i++) {
                coeffs = Derive(coeffs);
            }
            return coeffs;
        }

        /** Method:  Derive from a list of values 
        serie -  list of values
        order - derivative order */
        internal List<double> DeriveSerie(List<double> serie, int order) {
            if(serie.Count == 1) { 
                List<double> res = new List<double>();
                for(int i=0;i<serie.Count;i++) { res.Add(0.0); }
                return res;
            }
            int grade = serie.Count-1;
            if(grade >= 7) { grade = 6; }
            List<double> x = new List<double>();
            for(int i=0;i<serie.Count;i++) { x.Add((double)i); }
            List<double> coeffs = Regression(x, serie, grade);
            coeffs = Derive(coeffs, order);
            return GetRegValues(x, coeffs);
        }

        /** Method:  Calculate indexes of points where a change from negative to positive takes place */
        internal List<int> ZerosNegPosIndexes(List<double> serie) {
            bool neg = false;
            List<int> zerosIndexes = new List<int>();
            for(int i=0;i<serie.Count;i++) {
                if(serie[i] >= 0)  {
                    if(neg == true && i<serie.Count-1) { zerosIndexes.Add(i+1); }
                    neg = false;
                }
                else  {
                    neg = true;
                }
            }
            return zerosIndexes;
        }

        /** Method:  Calculate indexes of points where a change from positive to negative takes place */
        internal List<int> ZerosPosNegIndexes(List<double> serie) {
            bool pos = false;
            List<int> zerosIndexes = new List<int>();
            for(int i=0;i<serie.Count;i++) {
                if(serie[i] >= 0) {
                    if(pos == true && i<serie.Count-1) { zerosIndexes.Add(i+1); }
                    pos = false;
                } 
                else {
                    pos = true;
                }
            }
            return zerosIndexes;
        }
        
        #endregion


    }

    #region Inner class Result

    /** Class:  Result of a Regression calculation */
    internal class Result {

        #region Fields

        /** Method:  first coefficent */
        internal double c0;
        
        /** Method:  second coefficent */
        internal double c1;
        
        /** Method:  following first coefficent */
        internal double sig0;
        
        /** Method:  following second coefficent */
        internal double sig1;
        
        /** Method:  Squared chi coefficent */
        internal double chi2;
        
        /** Method:  Determination coefficent */
        internal double r2;
        
        /** Method:  list of weights */
        internal List<double> W;

        private int n;

        #endregion

        #region Constructors

        /** Method:  Constructor 
       n - number of data */
        internal Result(int n) {
            this.n = n;
            this.c0 = 0;
            this.c1 = 0;
            this.sig0 = 0;
            this.sig1 = 0;
            this.chi2 = 0;
            this.r2 = 0;
            this.W = new List<double>();
            for(int i=0;i<n;i++) { W.Add(1.0); }
        }

        /** Method:  Constructor
        n - number of data.
        W - list of weights. */
        internal Result(int n, List<double> W) {
            this.n = n;
            this.c0 = 0;
            this.c1 = 0;
            this.sig0 = 0;
            this.sig1 = 0;
            this.chi2 = 0;
            this.r2 = 0;
            this.W = W;
        }

        #endregion

        #region internal Methods

        /** Method:  Resets all weights to 1.0 (to avoid weighting effect) */
        internal void ResetW() {
            for(int i=0;i<n;i++) { W[i] = 1.0;  }
        }

        #endregion
    }

    #endregion
}
