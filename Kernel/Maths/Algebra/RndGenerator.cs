#region Imports

using System;
using System.Collections.Generic;

#endregion

namespace Maths {

    /** Method: 
     RandomProvider.  Provides random numbers of all data types in specified ranges.  It also contains a couple of methods
     from Normally (Gaussian) distributed random numbers and Exponentially distributed random numbers.
     */
    internal class RndGenerator {

        #region Fields

        private Random rand;
        private double storedUniformDeviate;
        private bool storedUniformDeviateIsGood = false;
        private Functions func;

        #endregion

        #region Constructor

        /** Method:  Constructor */
        internal RndGenerator()
        {
            func = new Functions();
            Reset();
        }

        /** Method:  Reset seed */
        internal void Reset()
        {
            rand = new Random(Environment.TickCount);
        }

        #endregion

        #region Distributions

        #region Uniform

        /** Method:  Devuelve un double randómico en el rango [0,1) (cero inc) */
        internal double NextDouble()
        {
            double nextDouble = 0.0;
            lock (rand) { nextDouble = rand.NextDouble(); }
            return nextDouble;
        }

        /** Method:  Devuelve un booleano randómico */
        internal bool NextBoolean()
        {
            double nextBoolean = 0.0;
            lock (rand) { nextBoolean = rand.Next(0, 2); }
            return nextBoolean != 0;
        }

        /** Method:  Devuelve un int en el rango [min, max) */
        internal int NextInt(int min, int max)
        {
            if (max == min) { return min; } else if (max < min) { throw new ArgumentException("Max must be greater than min"); }
            double nextDouble = 0.0;
            lock (rand) { nextDouble = rand.NextDouble(); }
            int randInt = Convert.ToInt32(min + nextDouble * (max - min));
            return randInt;
        }

        /** Method:  Devuelve un double en el rango [min, max) */
        internal double NextDouble(double min, double max)
        {
            if (max <= min) { throw new ArgumentException("Max must be greater than min"); }
            double nextDouble = 0.0;
            lock (rand) { nextDouble = rand.NextDouble(); }
            double randDbl = min + nextDouble * (max - min);
            return randDbl;
        }

        /** Method:  Devuelve un double en el rango [0,1) con distribucion Uniforme */
        internal double NextUniform()   {
            return NextDouble();
        }

        /** Method:  Devuelve un double en el rango dado con distribucion Uniforme */
        internal double NextUniform(double min, double max)   {
            return NextDouble(min, max);
        }

        /** Method:  Generar array de randomicos */
        internal double[] GenerarArrayRandom(int cantElementos)   {
            double[] array = new double[cantElementos];
            double nextDouble = 0.0;
            lock (rand) { nextDouble = rand.Next(); }
            for (int i = 0; i < cantElementos; i++) { array[i] = nextDouble; }
            return array;
        }

        #endregion

        #region Normal

        /** Method:  Devuelve variables tipificadas N(0,1) (sesgos) */
        internal double NextNormal()  {
            // basado en algoritmo de Numerical Recipes
            if (storedUniformDeviateIsGood)
            {
                storedUniformDeviateIsGood = false;
                return storedUniformDeviate;
            }
            else
            {
                double rsq = 0.0;
                double v1 = 0.0, v2 = 0.0, fac = 0.0;
                while (rsq == 0.0 || rsq >= 1.0)
                {
                    v1 = NextDouble() * 2.0 - 1.0;
                    v2 = NextDouble() * 2.0 - 1.0;
                    rsq = Math.Pow(v1, 2) + Math.Pow(v2, 2);
                }
                fac = Math.Sqrt(Math.Log(rsq, Math.E) / rsq * -2.0);
                storedUniformDeviate = v1 * fac;
                storedUniformDeviateIsGood = true;
                return v2 * fac;
            }
        }

        /** Method:  Devuelve variables N(m,s) (sesgos) 
        m – mean.
        s - standard deviation. */
        internal double NextNormal(double m, double s)  {
            double z = NextNormal();
            double x = s * z + m;
            return x;
        }


        #endregion

        #region Exponential

        /** Method:  Devuelve sesgos randómicos positivos con media = 1, con distribucion Exponencial */
        internal double NextExponential() {
            double dum = 0.0;
            while (dum == 0.0) { dum = NextUniform(); }
            return -Math.Log(dum);
        }

        /** Method:  Devuelve sesgos randómicos positivos con media = m, con distribucion Exponencial */
        internal double NextExponential(double m)  {
            return NextExponential() + m;
        }

        #endregion

        #endregion

    }
}
