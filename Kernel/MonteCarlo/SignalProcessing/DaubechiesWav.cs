#region Imports

using System;
using System.Collections.Generic;
using System.Text;
using Maths;

#endregion

namespace MonteCarlo
{

    /** Class:  Algorithmic repository class for Daubechies Wavelet calculation  */
    internal class Daubechies
    {

        #region Fields

        //scaling coeffs
        private double h0;
        private double h1;
        private double h2;
        private double h3;

        //wavelet coeffs
        private double g0;
        private double g1;
        private double g2;
        private double g3;

        #endregion

        #region Constructor

        /** Method:  Constructor  */
        internal Daubechies()
        {

            double sqrt2 = Math.Sqrt(2);
            double sqrt3 = Math.Sqrt(3);

            h0 = (1 + sqrt3) / (4 * sqrt2);
            h1 = (3 + sqrt3) / (4 * sqrt2);
            h2 = (3 - sqrt3) / (4 * sqrt2);
            h3 = (1 - sqrt3) / (4 * sqrt2);

            g0 = h3;
            g1 = -h2;
            g2 = h1;
            g3 = -h0;
        }

        #endregion

        #region Internal Methods

        //Forward Daubechies D4 transform
        /** Method:  Daubechies tranformed function  */
        internal List<double> daubTrans(List<double> serie)
        {
            List<double> trans = new List<double>(serie);
            for (int n = serie.Count; n >= 4; n = n / 2) { transform(trans, n); }
            return trans;
        }


        /** Method:  Inverse Daubechies D4 transform  from a list of coefficents coeff */
        internal List<double> invDaubTrans(List<double> coeff)
        {
            List<double> coeffs = new List<double>(coeff);
            for (int n = 4; n <= coeff.Count; n = n * 2) { invTransform(coeffs, n); }
            return coeffs;
        }

        #endregion

        #region Private Methods

        //Forward wavelet transform. 
        private void transform(List<double> serie, int n)
        {
            if (n < 4) { return; }
            double[] trans = new double[n];
            int i;

            //funciones D4 scaling y wavelet
            for (i = 0; i <= (n - 4) / 2; i++)
            {
                trans[i] = h0 * serie[2 * i] + h1 * serie[2 * i + 1] + h2 * serie[2 * i + 2] + h3 * serie[2 * i + 3]; //scaling
                trans[i + n / 2] = g0 * serie[2 * i] + g1 * serie[2 * i + 1] + g2 * serie[2 * i + 2] + g3 * serie[2 * i + 3]; //wavelet
            }
            //iteración final: como no hay n, n+1 se toma 0 y 1 para completar
            trans[i] = h0 * serie[n - 2] + h1 * serie[n - 1] + h2 * serie[0] + h3 * serie[1];
            trans[i + n / 2] = g0 * serie[n - 2] + g1 * serie[n - 1] + g2 * serie[0] + g3 * serie[1];

            //se copian valores en la serie original
            for (i = 0; i < n; i++) { serie[i] = trans[i]; }

        }


        private void invTransform(List<double> coeff, int n)
        {
            if (n >= 4)
            {

                double[] invTrans = new double[n];

                //iteracion inicial: como no hay -1 y -2, se toma ult y penultimo
                invTrans[0] = h2 * coeff[n / 2 - 1] + g2 * coeff[n - 1] + h0 * coeff[0] + g0 * coeff[n / 2];
                invTrans[1] = h3 * coeff[n / 2 - 1] + g3 * coeff[n - 1] + h1 * coeff[0] + g1 * coeff[n / 2];

                for (int i = 0; i < n / 2 - 1; i++)
                {
                    invTrans[2 * (i + 1)] = h2 * coeff[i] + g2 * coeff[i + n / 2] + h0 * coeff[i + 1] + g0 * coeff[i + n / 2 + 1]; //scaling
                    invTrans[2 * (i + 1) + 1] = h3 * coeff[i] + g3 * coeff[i + n / 2] + h1 * coeff[i + 1] + g1 * coeff[i + n / 2 + 1]; //wavelet
                }

                //se copian valores en la serie original
                for (int i = 0; i < n; i++) { coeff[i] = invTrans[i]; }
            }
        }


        #endregion

    }
}
