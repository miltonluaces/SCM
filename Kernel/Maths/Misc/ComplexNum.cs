#region Imports

using System;
using System.Collections.Generic;
using System.Text;

#endregion

namespace MonteCarlo {

    /** Method:  Data class for complex numbers  */
    internal class ComplexNum {

        #region Fields

        private double real;
        private double imag;

        #endregion

        #region Constructor

        /** Method:  Constructor  
        real -  real component 
        imag -  imaginary component */
        internal ComplexNum(double real, double imag) {
            this.real = real;
            this.imag = imag;
        }

        #endregion

        #region Properties

        /** Method:  real part  */
        internal double Real { get { return real; } }

        /** Method:  imaginary part  */
        internal double Imag { get { return imag; } }

        /** Method:  abs/modulus/magnitud  */

        internal double Abs { get { return Math.Sqrt(real * real + imag * imag); } }

        /** Method:  angle/phase/argument  between -pi and pi  */
        internal double Phase { get { return Math.Atan2(imag, real); } }

        #endregion

        #region Operations

        /** Method:  Plus: return a new Complex object whose value is (this + b)  
        b -  the complex number for the operation */
        internal ComplexNum Plus(ComplexNum b)
        {
            ComplexNum a = this;
            double real = a.real + b.real;
            double imag = a.imag + b.imag;
            return new ComplexNum(real, imag);
        }

        /** Method:  Minus:  return a new Complex object whose value is (this - b)  
        b -  the complex number for the operation */
        internal ComplexNum Minus(ComplexNum b)
        {
            ComplexNum a = this;
            double real = a.real - b.real;
            double imag = a.imag - b.imag;
            return new ComplexNum(real, imag);
        }

        /** Method:  Times:  return a new Complex object whose value is (this * b)  
        b -  the complex number for the operation */
        internal ComplexNum Times(ComplexNum b)
        {
            ComplexNum a = this;
            double real = a.real * b.real - a.imag * b.imag;
            double imag = a.real * b.imag + a.imag * b.real;
            return new ComplexNum(real, imag);
        }

        /** Method:  Scalar multiplication: returns a new object whose value is (this * alpha)  */
        internal ComplexNum Times(double alpha)
        {
            return new ComplexNum(alpha * real, alpha * imag);
        }

        /** Method:  Conjugate: returns a new Complex object whose value is the conjugate of this  */
        internal ComplexNum Conjugate() { return new ComplexNum(real, -imag); }

        /** Method:  Reciprocal: returns a new Complex object whose value is the reciprocal of this  */
        internal ComplexNum Reciprocal()
        {
            double scale = real * real + imag * imag;
            return new ComplexNum(real / scale, -imag / scale);
        }

        /** Method:  Divides: returns a / b  */
        internal ComplexNum Divides(ComplexNum b)
        {
            ComplexNum a = this;
            return a.Times(b.Reciprocal());
        }

        /** Method:  Exp: returns a new Complex object whose value is the complex exponential of this  */
        internal ComplexNum Exp()
        {
            return new ComplexNum(Math.Exp(real) * Math.Cos(imag), Math.Exp(real) * Math.Sin(imag));
        }

        /** Method:  Sin: returns a new Complex object whose value is the complex sine of this  */
        internal ComplexNum Sin()
        {
            return new ComplexNum(Math.Sin(real) * Math.Cosh(imag), Math.Cos(real) * Math.Sinh(imag));
        }

        /** Method:  Cos: returns a new Complex object whose value is the complex cosine of this  */
        internal ComplexNum Cos()
        {
            return new ComplexNum(Math.Cos(real) * Math.Cosh(imag), -Math.Sin(real) * Math.Sinh(imag));
        }

        /** Method:  Tan: returns a new Complex object whose value is the complex tangent of this  */
        internal ComplexNum Tan()
        {
            return Sin().Divides(Cos());
        }

        //Static Plus
        /** Method:  Sum of two numbers a and b, static  */
        internal static ComplexNum Plus(ComplexNum a, ComplexNum b)
        {
            double real = a.real + b.real;
            double imag = a.imag + b.imag;
            ComplexNum sum = new ComplexNum(real, imag);
            return sum;
        }

        #endregion

        #region Override To String

        /** Method:  ToString override   */
        public override String ToString() {
            if(imag == 0) { return real + ""; }
            if(real == 0) { return imag + " i"; }
            if(imag <  0) { return real + " - " + (-imag) + " i"; }
            return real + " + " + imag + " i";
        }

        #endregion

    }
}
