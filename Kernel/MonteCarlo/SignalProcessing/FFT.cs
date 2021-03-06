using System;
using System.Collections.Generic;
using System.Text;
using Maths;

namespace MonteCarlo  {

    /** Class:  Fast Fourier Transform and Inverse Fast Fourier Transform (N = power of 2.)  */
    internal class FFT : SignalProc {

        #region Constructor

        /** Method:  Constructor  */
        internal FFT() {
        }

        #endregion

        #region Real List Methods

        /** Method:  Fast fourier transform  
        X -  list of original data (independent vector variable)  */
        internal List<double> FftReg(List<double> X) {
            ComplexNum[] XComp = new ComplexNum[X.Count];
            for(int i=0;i<X.Count;i++) { XComp[i] = new ComplexNum(X[i], 0);  }
            ComplexNum[] YComp = Fft(XComp);
            XComp = Ifft(YComp);
            List<double> Y = new List<double>();
            foreach(ComplexNum xComp in XComp) { Y.Add(xComp.Real); }
            return Y;
        }

 	    #endregion

        #region Complex Array Methods

        /** Method:  Fast Fourier Transform : Cooley-Tukey algorithm 
        X -  list of complex values */
        internal ComplexNum[] Fft(ComplexNum[] X) {
            int N = X.Length;

            if(N == 1) { return new ComplexNum[] { X[0] }; }
            if(N % 2 != 0) { throw new Exception("N is not a power of 2"); }

            //generate fft of even and odd integers
            ComplexNum[] even = new ComplexNum[N/2];
            for(int k = 0;k < N/2;k++) { even[k] = X[2*k]; }
            ComplexNum[] fftEven = Fft(even);
            ComplexNum[] odd  = new ComplexNum[N/2];
            for(int k = 0;k < N/2;k++) { odd[k] = X[2*k + 1]; }
            ComplexNum[] fftOdd = Fft(odd);

            //combine terms
            ComplexNum[] Y = new ComplexNum[N];
            for(int k = 0;k < N/2;k++) {
                double kth = -2 * k * Math.PI / N;
                ComplexNum wk = new ComplexNum(Math.Cos(kth), Math.Sin(kth));
                Y[k] = fftEven[k].Plus(wk.Times(fftOdd[k]));
                Y[k + N/2] = fftEven[k].Minus(wk.Times(fftOdd[k]));
            }
            return Y;
        }


        /** Method:  Inverse Fast Fourier Transform  
        Y -  transformed function */
        internal ComplexNum[] Ifft(ComplexNum[] Y)
        {
            int N = Y.Length;
            ComplexNum[] X = new ComplexNum[N];

            //take conjugate
            for(int i = 0;i < N;i++) { X[i] = Y[i].Conjugate(); }

            //compute forward FFT
            X = Fft(X);

            //take conjugate again
            for(int i = 0;i < N;i++) { X[i] = X[i].Conjugate(); }

            //divide by N
            for(int i = 0;i < N;i++) { X[i] = X[i].Times(1.0/N); }

            return X;
        }

        /** Method:  Circular convolution of x and y  
        Χ -  list of complex independent variable vector data 
        Υ -   list of complex dependent variable vector data */
        internal ComplexNum[] CircularConvolve(ComplexNum[] Χ, ComplexNum[] Υ)
        {

            //should probably pad x and y with 0s so that they have same length and are powers of 2
            if(Χ.Length != Υ.Length) { throw new Exception("Dimensions do not agree"); }

            int N = Χ.Length;

            //compute FFT of each sequence
            ComplexNum[] A = Fft(Χ);
            ComplexNum[] B = Fft(Υ);

            //point-wise multiply
            ComplexNum[] C = new ComplexNum[N];
            for(int i = 0;i < N;i++) { C[i] = A[i].Times(B[i]); }

            //compute inverse FFT
            return Ifft(C);
        }


        //Linear convolution of x and y
        /** Method:  Linear convolution  
        Χ -  list of complex independent variable vector data
        Y -  list of complex dependent variable vector data */
        internal ComplexNum[] LinearConvolve(ComplexNum[] Χ, ComplexNum[] Y)
        {
            ComplexNum zero = new ComplexNum(0, 0);

            ComplexNum[] A = new ComplexNum[2 * Χ.Length];
            for(int i=0;i<Χ.Length;i++) { A[i] = Χ[i]; }
            for(int i=Χ.Length;i<2*Χ.Length;i++) { A[i] = zero;  }

            ComplexNum[] B = new ComplexNum[2 * Y.Length];
            for(int i=0;i<Y.Length;i++) { B[i] = Y[i]; }
            for(int i=Y.Length;i< 2*Y.Length;i++) { B[i] = zero; }

            return CircularConvolve(A,B);
        }

        /** Method:  Circular convolution of x and y  
        Χ -  list of complex independent variable vector data */
        internal ComplexNum[] CircularConvolve(ComplexNum[] Χ)
        {

            int N = Χ.Length;

            //compute FFT of the sequence
            ComplexNum[] A = Fft(Χ);
            
            //point-wise multiply
            ComplexNum[] C = new ComplexNum[N];
            for(int i = 0;i < N;i++) { C[i] = A[i].Times(A[i]); }

            //compute inverse FFT
            return Ifft(C);
        }

        //Linear convolution of x and y
        /** Method:  Linear convolution 
        Χ -  list of complex independent variable vector data */
        internal ComplexNum[] LinearConvolve(ComplexNum[] Χ)
        {
            ComplexNum zero = new ComplexNum(0, 0);

            ComplexNum[] A = new ComplexNum[2 * Χ.Length];
            for(int i=0;i<Χ.Length;i++) { A[i] = Χ[i]; }
            for(int i=Χ.Length;i<2*Χ.Length;i++) { A[i] = zero; }

            return CircularConvolve(A);
        }

        /** Method:  Multiple fft convolution  
        Data -  data complex array 
        n -  number of convolution */
        internal ComplexNum[] LinearConvolve(ComplexNum[] Data, int n)
        {
            ComplexNum zero = new ComplexNum(0, 0);
            ComplexNum two = new ComplexNum(2, 0);
            ComplexNum[] A = null;
            int length = Data.Length;
            ComplexNum[] C = null;
            ComplexNum[] CAnt = new ComplexNum[Data.Length * 2];
            for(int i=0;i<Data.Length;i++) { CAnt[i] = Data[i]; }
            for(int i=Data.Length;i<Data.Length*2;i++) { CAnt[i] = zero; }
            for(int i=0;i<n;i++) {
                length *= 2;
                A = new ComplexNum[length];
                for(int j=0;j<Data.Length;j++) { A[j] = Data[j]; }
                for(int j=Data.Length;j<length;j++) { A[j] = zero; }
                if(C!=null) { CAnt = C; }
                C = new ComplexNum[length];
                for(int j=0;j<CAnt.Length;j++) { C[j] = CAnt[j]; }
                for(int j=CAnt.Length;j<length;j++) { C[j] = zero; }
                C = CircularConvolve(C, A);

            }
            return C;     
        }

        #endregion
    }
}
