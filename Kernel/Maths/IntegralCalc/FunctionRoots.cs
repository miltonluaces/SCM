#region Imports

using System;
using System.Collections.Generic;
using System.Text;

#endregion

namespace Maths {

    /** Method:  Repository of algorithms for function roots search */
    internal class RootSearch {

        #region Fields

        private double epsilon;
        private double epsilonInterval;
        private int iterations;
        private int maxIterations;

        private Dictionary<string, double> fCol;
        private Dictionary<string, double> fdxCol = null;
        private Dictionary<string, double> fdx2Col = null;

        private RndGenerator randGen;

        #endregion

        #region internal Functions

        /** Method:  Function */
        internal double f(double x) { 
            return fCol[x.ToString("0.00")];  
        }

        /** Method:  First derivative Function */
        internal double fdx(double x) { 
            return fdxCol[x.ToString("0.00")]; 
        }

        /** Method:  Second derivative Function */
        internal double fdx2(double x) { 
            return fdx2Col[x.ToString("0.00")]; 
        }

        #endregion

        #region internal Delegates

        /** Method: 
        Función de la cual se desea encontrar el cero o ceros
        Es un delegado, por tanto con el delegado encontramos el valor de la función en x
        x  - Valor para el cual deseamos obtener la funcion */
        internal delegate double function(double x);
        /** Method: 
        Derivada primera de la función de la cual se desea encontrar el cero o ceros */
        internal delegate double derivative(double x);
        /** Method: 
        Derivada segunda de la función de la cual se desea encontrar el cero o ceros */
        internal delegate double derivative2(double x);

        #endregion

        #region Constructor

        /** Method: Constructor */
        internal RootSearch(double epsilon, double epsilonInterval, int maxIterations) {
            this.epsilon = epsilon;
            this.epsilonInterval = epsilonInterval;
            this.maxIterations = maxIterations;
            this.randGen = new RndGenerator();
        }

        #endregion

        #region Properties

        /** Method:  number of iterations allowed */
        internal int Iterations {
            get { return iterations; }
        }

        /** Method:  Epsilon allowed for solution validation */
        internal double Epsilon {
            get { return epsilon; }
            set { epsilon = value; }
        }

        #endregion

        #region Range Methods

        internal Dictionary<double, double> RangeSearch(function f, double xMin, double xMax, double yMin, double yMax, double step, double eps, ref int its, int maxIt) {
            int hits = 0;
            int it = 0;
            double relEpsX = 0.05;
            int nRanges = -1;
            double x, y, xIni, xEnd, yIni, yEnd, keY;
            randGen.Reset();
            Dictionary<double, double> yxValues = new Dictionary<double, double>();
            try {
                //initial values
                yxValues.Add(yMin, xMin);
                y = yMin + step;
                while (y < yMax) {
                    yxValues.Add(y, -1);
                    y = Math.Round(y + step, 1);
                }
                yxValues.Add(yMax, xMax);

                hits += FillValues(f, yxValues, xMin, xMax, ref it, maxIt, 1);
                its += it;
                xIni = xMin;
                yIni = yMin;
                int nMiss = 0;
                List<Range> ranges = new List<Range>();
                int wIts = 0;
                int maxWIts = 10;
                while (true) {
                    foreach (double key in yxValues.Keys) {
                        y = key;
                        x = yxValues[key];
                        if (x == -1) { 
                            nMiss++;
                        }
                        else {
                            if (nMiss == 0)  {
                                xIni = x;
                                yIni = y;
                            }
                            else  {
                                xEnd = x;
                                yEnd = y;
                                ranges.Add(new Range(xIni, xEnd, yIni, yEnd, nMiss));
                                nMiss = 0;
                                xIni = x;
                                yIni = y;
                            }
                        }
                    }
                    if (ranges.Count == 0) { break; }
                    foreach (Range range in ranges) {
                        if (range.nValues == 1)  {
                            keY = Math.Round(range.yIni + step, 1);
                            if (keY <= yMax) { yxValues[keY] = (range.xIni + range.xEnd) / 2.0; }
                            hits++;
                        }
                        else  {
                            if (wIts > maxWIts || ranges.Count == nRanges || (range.xEnd - range.xIni) / range.xEnd < relEpsX) {
                                y = range.yIni;
                                x = (range.xIni + range.xEnd) / 2.0;
                                while (y <= range.yEnd) {
                                    y = y + step;
                                    keY = Math.Round(y, 1);
                                    if (keY == 100.0 && !yxValues.ContainsKey(100.0)) { yxValues.Add(keY, yxValues[99.9]); hits++; }
                                    if (yxValues.ContainsKey(keY) && yxValues[keY] == -1) { yxValues[keY] = x; hits++; }
                                }
                            }
                            else {
                                it = 0;
                                hits += FillValues(f, yxValues, range.xIni, range.xEnd, ref it , range.nValues * 10, 1);
                                its += it;

                            }
                        }
                    }
                    if (yxValues[99.9] == -1)  {
                        yxValues[99.9] = yxValues[100];
                        hits++;
                    }
                    nRanges = ranges.Count;
                    ranges.Clear();

                    wIts++;
                }
            } catch (Exception ex) {
                throw new Exception("Error in range calculation: " + ex.Message, ex);
            }
            double prevValue = -1;
            Dictionary<double, double> monotoneYxValues = new Dictionary<double, double>();
            foreach (double key in yxValues.Keys)  {
               if (yxValues[key] >= prevValue) { monotoneYxValues.Add(key, yxValues[key]); }
               else { monotoneYxValues.Add(key, prevValue); }
               prevValue = yxValues[key];
            }
            return monotoneYxValues;
        }

        private int FillValues(function f, Dictionary<double, double> yxValues, double xMin, double xMax, ref int it, int maxIt, int dec) {
            if (xMin == xMax) { return 0; }
            double x, y;
            int hits = 0;
            for (it = 0; it < maxIt; it++) {
                x = randGen.NextDouble(xMin, xMax);
                y = Math.Round(f(x)*100, dec);
                if (yxValues.ContainsKey(y) && yxValues[y] == -1) { 
                    yxValues[y] = x;
                    hits++;
                }
            }
            return hits;
        }

        
        #endregion

        #region Bisection Methods

        /** Method:  Bisection root search numerical method 
        f -  function 
        d -  first derivative 
        min - min value of the interval
        max -  max value of the interval 
        val -  value to add (0 if pure root search) 
        eps - epsilon for solution validation 
        maxIt - maximum of iterations */
        internal double MonotoneBisection(function f, derivative d, double min, double max, double val, double eps, ref int it, int maxIt) {
            double dMin = d(min);
            double dMax = d(max);
            if (dMin * dMax < 0) { throw new ArgumentException("This method is only for monotone intervals");  }
            return MonotoneBisectionRec(f, min, max, val, (dMin > 0), eps, ref it, maxIt);
        }

        /** Method:  Bisection root search numerical method 
       f -  function 
        ascendant -  if the function is monotone, ascendant or not
        min - min value of the interval
        max -  max value of the interval 
        val -  value to add (0 if pure root search) 
        eps - epsilon for solution validation 
        maxIt - maximum of iterations */
        internal double MonotoneBisection(function f, bool ascendant, double min, double max, double val, double eps, ref int it, int maxIt) {
            return MonotoneBisectionRec(f, min, max, val, ascendant, eps, ref it, maxIt);
        }

        /** Method:  Bisection root search numerical method 
       f -  function 
        ascendant -  if the function is monotone, ascendant or not
        min - min value of the interval
        max -  max value of the interval 
        val -  value to add (0 if pure root search) 
        eps - epsilon for solution validation 
        maxIt - maximum of iterations 
        it - current iteration */
        private double MonotoneBisectionRec(function f, double min, double max, double val, bool ascendant, double eps, ref int it, int maxIt) {
            it++;
            double inter = min + (max - min) / 2.0;
            if(it > maxIt || inter == min || inter == max) {
                //Console.WriteLine("Error.More than 30 iterations.");
                return inter;
            }
            double fInter = f(inter);
            if(Math.Abs(val - fInter) < eps) { return inter; }
            if(ascendant) {
               if(val < fInter) { return MonotoneBisectionRec(f, min, inter, val, ascendant, eps, ref it, maxIt); }
               if(val > fInter) { return MonotoneBisectionRec(f, inter, max, val, ascendant, eps, ref it, maxIt); }
            } 
            else {
                if(val > fInter) { return MonotoneBisectionRec(f, min, inter, val, ascendant, eps, ref it, maxIt); }
                if(val < fInter) { return MonotoneBisectionRec(f, inter, max, val, ascendant, eps, ref it, maxIt); }
            }
            //Console.WriteLine("Error inter = " + inter);
            return inter;
        }
        
        #endregion

        #region Newton Raphson Methods

        /** Method: 
        Metodo Newton Raphson para encontrar una raiz o cero de una función
        Si val es cero entonces encuantra la raiz de la funcion 
        si no, encuentra la inversa de la función <paramref name="f"/> en val
      f -  function 
        d -  first derivative 
        d2 -  second derivative 
        min - min value of the interval
        max -  max value of the interval 
        val -  value to add (0 if pure root search) 
        eps - epsilon for solution validation 
        maxIt - maximum of iterations */
        internal double NewtonRaphsonOneRoot(function f, derivative d, derivative2 d2, double min, double max, double val) {
            if ((f(min) - val) * f(max) - val > 0) { throw new ArgumentException("No zero between these values"); }
            double nr = 0;
            double x;
            int i=0;
            while(nr== 0) {
                x = GetFourierValue(f, d2, min, max, val);
                nr = NewtonRaphson(f, d, x, val);
                if (i > maxIterations) { throw new Exception("Exceed the maximun iterations"); }
                i++;
            }
            return nr;
        }

        /** Method: 
        Metodo Newton Raphson para encontrar una raiz o cero de una función</para>
        Si val es cero entonces encuantra la raiz de la funcion,
        si no, encuentra la inversa de la función 
      f -  function 
        d -  first derivative 
        x -  independent variable
        val - Valor para el cual se desea buscar la inversa de la función */
        internal double NewtonRaphson(function f, derivative d, double x, double val) {
            int i=0;
            double gx = f(x) - val;
            while(Math.Abs(gx) > epsilon) {
                if(d(x) == 0) { return 0; }
                x = x - gx / d(x);
                gx = f(x) - val;
                i++;
                if (i > maxIterations) { throw new Exception("Exceed the maximun iterations"); }
            }
            return x;
        }

        #endregion

        #region Steffensen Acceleration - Aitken Iteration

        private double GetAitkenIteration(double xiMin2, double xiMin1, double xi) {
            double num = xi * xiMin2 - quad(xiMin1);
            double den = (xi - 2 * xiMin1 + xiMin2);
            double res = num/den;
            return (xi * xiMin2 - quad(xiMin1)) / (xi - 2 * xiMin1 + xiMin2);
        }

        /** Method: 
        Metodo Steffenson, que es una forma mas rapida de encontrar una raiz o cero de una función
        Si va es cero entonces encuantra la raiz de la funcion
        si no, encuentra la inversa de la función 
      f -  function 
        d -  first derivative 
        x -  independent variable
        val - Valor para el cual se desea buscar la inversa de la función */
        internal double SteffensenAcceleration(function f, derivative d, double x, double val) {
            double gxAnt = double.MaxValue;
            epsilon = 0.005;
            double gx = f(x) - val;
            int i=0;
            double xi = -1;
            double xiMin1 = -1;
            double xiMin2 = -1;

            while(Math.Abs(gx) > epsilon) {
                if(d(x) == 0) { return 0; }
                if(i<4 || i%2 == 0 || xi - 2 * xiMin1 + xiMin2 == 0) {
                    x = x - gx / d(x);
                } 
                else {
                    x = GetAitkenIteration(xiMin2, xiMin1, xi);
                }
                gx = f(x) - val;
                if(Math.Abs(gx) > Math.Abs(gxAnt)) { return xi; }
                gxAnt = gx;
                xiMin2 = xiMin1;
                xiMin1 = xi;
                xi = x;
                i++;
                if(i > maxIterations) { return 0; }
            }
            iterations = i;
            return x;
        }

        /** Method: 
       Metodo Steffenson, que es una forma mas rapida de encontrar una raiz o cero de una función
        Si val es cero entonces encuantra la raiz de la funcion
        si no, encuentra la inversa de la función 
         f -  function 
        d -  first derivative 
        d2 -  second derivative 
        min - min value of the interval
        max -  max value of the interval 
        val -  value to add (0 if pure root search) */
        internal double SteffensenAccOneRoot(function f, derivative d, derivative2 d2, double min, double max, double val)
        {
            if ((f(min) - val) * f(max) - val > 0) { throw new ArgumentException("No zero between these values"); }
            double st = 0;
            double x;
            int i=0;
            while(st == 0) {
                x = GetFourierValue(f, d2, min, max, val);  
                st = SteffensenAcceleration(f, d, x, val);
                i++;
                if (i > maxIterations) { throw new Exception("Exceeded maximun iterations"); }
            }
            //Console.WriteLine("val = " + val + " , st = " + st + " , it = " + i);
            return st;
        }

        /** Method: 
        Metodo Steffenson, que es una forma mas rapida de encontrar una raiz o cero de una función
        Si val es cero entonces encuantra la raiz de la funcion 
        si no, encuentra la inversa de la función
      f -  function 
        d -  first derivative 
        d2 -  second derivative 
        min - min value of the interval
        max -  max value of the interval 
        val -  value to add (0 if pure root search) 
        maxIterations -  maximum of iterations */
        internal double SteffensenAccOneRoot(function f, derivative d, derivative2 d2, double min, double max, double val, int maxIterations) {
            if ((f(min) - val) * f(max) - val > 0) { throw new Exception("No zero between these values"); }
            double st = 0;
            double x;
            int i=0;
            while(st == 0) {
                x = GetFourierValue(f, d2, min, max, val);
                st = SteffensenAcceleration(f, d, x, val);
                i++;
                if (i > maxIterations) { throw new Exception("Exceed maximun iterations"); }
            }
            //Console.WriteLine("val = " + val + " , st = " + st + " , it = " + i);
            return st;
        }
        
        /** Method:  Try steffensen acceleration (if error, return -1) 
        f -  function 
        x - independent variable 
        min - min value of the interval
        max -  max value of the interval 
        val -  value to add (0 if pure root search) 
       iterations - number of iterations */
        internal double TrySteffensenAcceleration(function f, derivative d, double x, double val, double iterations) {
            double gx = f(x) - val;
            int i=0;
            double xi = -1;
            double xiMin1 = -1;
            double xiMin2 = -1;

            while(Math.Abs(gx) > epsilon) {
                if(d(x) == 0) { return 0; }
                if(i<4 || i%2 == 0 || xi - 2 * xiMin1 + xiMin2 == 0) {
                    x = x - gx / d(x);
                } else {
                    x = GetAitkenIteration(xiMin2, xiMin1, xi);
                }
                gx = f(x) - val;
                xiMin2 = xiMin1;
                xiMin1 = xi;
                xi = x;
                i++;
                if (i > maxIterations) { throw new Exception("Exceed maximun iterations"); }
            }
            iterations = i;
            return x;
        }
        
        #endregion

        #region Search Root with derivatives calculation

        /** Method:  Search one root in an interval 
        X"> independent variable time series 
        Y"> dependent variable time series 
        min"> min value of interval 
        max"> max value of interval 
        val"> value to search */
        internal double SearchOneRoot(List<double> X, List<double> Y, double min, double max, double val) {
            if (X.Count != Y.Count) { throw new ArgumentException("Error. X must have same size as Y"); }

            Splines sp = new Splines(X, Y, true);
            fCol = new Dictionary<string, double>();
            //cargar la f con los sp.Interpolar

            Differential de = new Differential();
            //fdxCol = de.GetDerivative(X,Y,1); cargar diccionario
            //fdx2Col = de.GetDerivative(X,Y,2); cargar diccionario
            return SteffensenAccOneRoot(f,fdx, fdx2, min, max, val);
        }

        #endregion

        #region Fill Percentiles

        
        /** Method:  Set all values within an interval 
        values"> dictionary containing values (return parameter) 
        f - function 
        min - min value of independent variable 
        max - max value of independent variable 
        fmin - min value of dependent variable 
        fmax - max value of dependent variable */
        internal int SetAllValues(Dictionary<int, double> values, function f, double min, double max, double fmin, double fmax, bool bisection) {
            if (bisection) { return SetAllValuesBisection(values, f, min, max, fmin, fmax);  }
            else { return SetAllValuesMontecarlo(values, f, min, max, fmin, fmax); }
        }
        
        private int SetAllValuesMontecarlo(Dictionary<int, double> values, function f, double min, double max, double fmin, double fmax) {
            int its = 0;
            Dictionary<double, double> rs = RangeSearch(f, fmin, fmax, min, max, 0.1, epsilon, ref its, maxIterations);
            foreach (double key in rs.Keys) { values[GetKey(key)] = rs[key]; }
            return its;
        }
        
        private int SetAllValuesBisection(Dictionary<int, double> values, function f, double min, double max, double fmin, double fmax) {
            int it = 0;
            int its = 0;
            if (max - min <= 0.1)
            {
                values[GetKey(min)] = fmin;
                values[GetKey(max)] = fmax;
            }
            else if(fmax - fmin < 1) {
                for(int i=GetKey(min);i<=GetKey(max);i++) {
                    values[i] = fmax;
                }
            }
            else {
                double fmid = (fmin + fmax)/2.0;
                double mid = Math.Round(f(fmid) * 100, 1);
                if(mid <= min || mid >= max) {
                    mid = (min + max) /2.0;
                    fmid = MonotoneBisection(f, true, fmin, fmax, mid/100.0, 0.001, ref it, 100);
                    its += it;
                }
                its += SetAllValuesBisection(values, f, min, mid, fmin, fmid);
                its += SetAllValuesBisection(values, f, mid, max, fmid, fmax);
            }
            return its;
        }

        private int GetKey(double val) {
            return Convert.ToInt32(Math.Round(val, 1)* 10);
        }


        #endregion

        #region Fourier Methods

        /** Method: 
        Devuelve el punto de fourier con el que el metodo de Newton Raphson converge muy rapidamente
        Fourier Convergence Conditions value. Returns zero if does not fit conditions
        f(a)*f(b) less than 0   2) f" greater than 0 en [a,b]  val is for non roots (otherwise 0)
        f - funcion mediante la cual se calcula el valor de dicha funcion
        d2 - funcion mediante la cual se calcula el valor de la derivada segunda de dicha funcion
        a - Valor minimo del intervalo en el cual busca la raiz o cero
        b - Valor maximo del intervalo en el cual busca la raiz o cero
        val - Valor para el cual se desea buscar la inversa de la función */
        private double GetValueInFourierInterval(function f, derivative2 d2, double a, double b, double val)  {
            double posValue = 0;
            double negValue = 0;
            double ga = f(a) - val;
            double gb = f(b) - val;
            if(ga * gb >= 0) { return 0; }  
            if(ga > 0) { posValue = a; negValue = b; }
            else { posValue = b; negValue = a; }

            //Second derivative constant in the interval (warning: sign is not verified on intermediate values)
            if(d2(a) > 0 && d2(b) > 0) { return posValue; }  
            else { return negValue; }
        }

        /** Method: 
       Encuentra el punto de fourier con el que el metodo de Newton Raphson converge muy rapidamente
        Fourier Convergence Conditions value for functions with one root on interval
        f - funcion mediante la cual se calcula el valor de dicha funcion
        d2 - funcion mediante la cual se calcula el valor de la derivada segunda de dicha funcion
        min - Valor minimo del intervalo en el cual busca la raiz o cero
        max - Valor maximo del intervalo en el cual busca la raiz o cero
        val - Valor para el cual se desea buscar la inversa de la función
        Si no existe una raiz, entonces genera error
        the calculated fourier value */
        internal double GetFourierVal(function f, derivative2 d2, double min, double max, double val) {
            double a = randGen.NextDouble(min, max);
            double b = randGen.NextDouble(min, max);
            int i=0;
            if(i>maxIterations-1) {
                a = min;
                b = max;
            }
            double ga = f(a) - val;
            double gb = f(b) - val;
            if(ga * gb < 0) {
                return GetValueInFourierInterval(f, d2, a, b, val);
            }
                
            while(true) {
                a = randGen.NextDouble(min, max);
                b = randGen.NextDouble(min, max);
                ga = f(a) - val;
                gb = f(b) - val;
                if(ga * gb < 0) {
                    return GetValueInFourierInterval(f, d2, a, b, val);
                }
                if (i > maxIterations) { throw new Exception("Exceed the maximun iterations"); }
                i++;
            }
        }

        /** Method: 
        Encuentra el punto de fourier con el que el metodo de Newton Raphson converge muy rapidamente</para>
        Fourier Convergence Conditions value for functions with one root on interval</para>
          f - funcion mediante la cual se calcula el valor de dicha funcion
        d2 - funcion mediante la cual se calcula el valor de la derivada segunda de dicha funcion
        min - Valor minimo del intervalo en el cual busca la raiz o cero
        max - Valor maximo del intervalo en el cual busca la raiz o cero
        val - Valor para el cual se desea buscar la inversa de la función
        Si no existe una raiz, entonces genera error
        the calculated fourier value */
        internal double GetFourierValue(function f, derivative2 d2, double min, double max, double val) {
            return GetFourierValue(f, d2, min, max, val, 0);
        }

        private double GetFourierValue(function f, derivative2 d2, double min, double max, double val, int it) {
            it++;
            if(it>maxIterations) {
                //Console.WriteLine("Obtener Fourier Value: Mas de 30 iteraciones");
                return GetValueInFourierInterval(f, d2, min, max, val);
            }
            double gMin = f(min) - val;
            double gMax = f(max) - val;
            double inter = min + (max - min) / 2.0;
            double gInter = f(inter) - val;
            if(Math.Abs(gInter) < 0.02) { return inter; }
            if(gMin * gInter < 0) { return GetFourierValue(f, d2, min, inter, val, it); } 
            else { return GetFourierValue(f, d2, inter, max, val, it); }
        }
        
        #endregion

        #region Auxiliar Methods

        /** Method:  Quadratic function, just for performance */
        internal double quad(double val) {
            return val * val;    
        }

        #endregion

        #region Class Cell

        internal class Range {
            internal double xIni;
            internal double xEnd;
            internal double yIni;
            internal double yEnd;
            internal int nValues;

            internal Range(double xIni, double xEnd, double yIni, double yEnd, int nValues) {
                this.xIni = xIni;
                this.xEnd = xEnd;
                this.yIni = yIni;
                this.yEnd = yEnd;
                this.nValues = nValues;
            }
        }
        
        #endregion

    }
}
