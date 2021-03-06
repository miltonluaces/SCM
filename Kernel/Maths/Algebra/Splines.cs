#region Imports

using System;
using System.Collections;
using System.Collections.Generic;

#endregion

namespace Maths {
    
   /** Method:  Linear and cubic b-splines Class */
    internal class Splines {

        #region Fields

        private List<Point> points;
        private double[] y2;
        private List<Point> lines;
        private bool isLinear;

        #endregion

        #region Constructors

        internal Splines() {
        }

        internal Splines(List<Point> points, bool isLinear) {
            this.points = points;
            this.isLinear = isLinear;
            if(isLinear) { PrecalculoLinear(); } else { PrecalculoCubica(); }
        }

        /** Method: */
        internal Splines(List<double> x, List<double> y, bool isLinear) {
            if(x.Count != y.Count) { throw new Exception("arrays must have same size"); }
            this.points = new List<Point>();
            for(int i=0;i<x.Count;i++) {
                points.Add(new Point(Convert.ToDouble(x[i]), Convert.ToDouble(y[i])));
            }
            this.isLinear = isLinear;
            if(isLinear) { PrecalculoLinear(); } 
            else { PrecalculoCubica(); }
        }

        #endregion

        #region internal Methods

        /** Method: 
       Metodo Principal de interpolacion
        Para cada valor de x, rectas[x] tiene los coeficientes a y b de la recta (en x e y) */
        internal double Interpolar(double x) {
            double interp = 0.0;
            if(isLinear) {
                if(x >= ((Point)points[points.Count-1]).x) { return ((Point)points[points.Count-1]).y; } 
                else { return ((Point)lines[(int)x]).x * x + ((Point)lines[(int)x]).y; }
            } 
            else {
                int n = this.points.Count;
                int klo = 0;
                int khi = n-1;

                //biseccion
                while(khi-klo > 1) {
                    int k = (khi + klo) >> 1;
                    if(GetX(k) > x) { khi = k; } else { klo = k; }
                }

                double h = GetX(khi)- GetX(klo);
                double a = (GetX(khi) - x) / h;
                double b = (x - GetX(klo)) / h;

                //Evaluacion del spline cubico
                interp =  a * GetY(klo) + b * GetY(khi) + ((a * a * a - a) * y2[klo] + (b * b * b - b) * y2[khi]) * (h * h) / 6.0;
            }
            return interp;
        }

        /** Method: 
        Interpolacion puntual de un punto en un intervalo
        x - independent variable for interpolation
        x1 - first defining point (x axis value) 
        y1 - first defining point (y axis value) 
        x2 - second defining point (x axis value) 
        y2 - second defining point (y axis value) */
        internal double Interpolar(double x, double x1, double y1, double x2, double y2) {
            double a = (y2 - y1) / (x2 - x1);
            double b = y1 - (x1 * (y2 - y1) / (x2 - x1));
            return a * x + b;
        }

        /** Method:  Obtener las n inversas de los splines 
        x = (y - b) / a
        y - dependent variable */
        internal List<double> Inversas(double y) {
            List<double> inversas = new List<double>();
            Point recta;
            double x;
            for(int i=0;i<lines.Count;i++) {
                recta = (Point)lines[i];
                if(recta.x == 0) {  // si tiene derivada nula
                    if(y == recta.y) { /*Console.WriteLine("Intervalo [" + i + " , " + (i+1) + "] tiene infinitas inversas");*/ }
                } else {
                    x = (y - recta.y) / recta.x;  //si intercepta el liston
                    if(x >= i && x < i+1) { inversas.Add(x); }
                }
            }
            return inversas;
        }

        /** Method:  Positive intervals calculation 
        y - dependent variable */
        internal List<Point> IntervalosPositivos(double y) {
            List<Point> intervalos = new List<Point>();
            Point recta;
            double x;
            double delta = 0.00001;
            bool meseta = false;
            Point intervalo = new Point();
            for(int i=0;i<lines.Count;i++) {
                recta = (Point)lines[i];
                if(recta.x == 0) {                                  //derivada cero, es meseta
                    if(y == recta.y) {
                        intervalo = new Point(i, i+1);
                        intervalos.Add(intervalo);
                        meseta = true;
                    }
                } else {
                    x = (y - recta.y) / recta.x;                    //es punto de interseccion
                    if(x  >= i - delta &&  x <= (i+1)+ delta) {     //es valido
                        if(recta.x > 0) {                           //derivada positiva  
                            if(meseta) { meseta = false; }
                            intervalo = new Point();
                            intervalo.x = x;                        //el punto de interseccion es apertura de intervalo
                        } else {                                      //derivada negativa
                            if(meseta) { meseta = false; } else {
                                intervalo.y = x;                    //el punto de interseccion es cierre de intervalo
                                intervalos.Add(intervalo);
                            }
                        }
                    }
                }
            }
            if(intervalos.Count == 0) { return intervalos; }

            //join de intervalos
            List<Point> intervalosJoin = new List<Point>();
            Point act = new Point();
            Point next = new Point();
            act = (Point)intervalos[0];
            double a = act.x;
            double b;
            meseta = false;
            for(int i=0;i<intervalos.Count;i++) {
                act = (Point)intervalos[i];
                if(i<intervalos.Count-1) {
                    next = (Point)intervalos[i+1];
                    if(Math.Abs(act.y - next.x) < delta) {
                        if(!meseta) {
                            a = act.x;
                            meseta = true;
                        }
                        continue;
                    }
                }
                if(!meseta) { a = act.x; }
                b = act.y;
                intervalosJoin.Add(new Point(a, b));
                meseta = false;
            }
            return intervalosJoin;
        }

        /** Method:  Agregar un valor */
        internal void Add(double x, double y) {
            Point p = new Point(x, y);
            points.Add(p);
        }

        /** Method: Reset Vector de valores  */
        internal void Clear() {
            this.points.Clear();
        }

        #endregion

        #region Private Methods

        private void PrecalculoLinear() {
            lines = new List<Point>();
            Point line;
            double a;
            double b;
            for(int i=0;i<points.Count-1;i++) {
                a = (GetY(i+1) - GetY(i)) / (GetX(i+1) - GetX(i));
                b = GetY(i) - (GetX(i) * (GetY(i+1) - GetY(i)) / (GetX(i+1) - GetX(i)));
                line = new Point(a, b);
                lines.Add(line);
            }
        }

        private void PrecalculoCubica() {
            int n = this.points.Count;
            double[] u = new double[n];
            this.y2 = new double[n];
            u[0] = 0;
            this.y2[0] = 0;

            //Descomposicion tridiagonal. y2 y u variables temporales.
            for(int i=1;i<n-1;++i) {
                double wx = GetX(i+1) - GetX(i-1);
                double sig = (GetX(i) - GetX(i-1)) / wx;
                double p = sig * y2[i-1] + 2.0;

                this.y2[i] = (sig-1.0) / p;

                double ddydx = (GetY(i+1) - GetY(i)) / (GetX(i+1) - GetX(i)) - ((GetY(i) - GetY(i-1)) / (GetX(i) - GetX(i-1)));
                u[i] = (6.0 * ddydx / wx - sig * u[i-1]) / p;
            }
            this.y2[n-1] = 0;

            // This is the backsubstitution loop of the tridiagonal algorithm
            for(int i=n-2;i>=0;--i) {
                this.y2[i] = this.y2[i] * this.y2[i+1] + u[i];
            }
        }

        private double GetX(int index) {
            return (double)((Point)points[index]).x;
        }

        private double GetY(int index) {
            return (double)((Point)points[index]).y;
        }

        #endregion

        #region Struct Point

        /** Method:  Point struct (x,y) */
        internal struct Point {
            
            /** Method:  x axis value */
            internal double x;
            /** Method:  y axis value */
            internal double y;
            
            /** Method:  Constructor */
            /// <param name="x"> x axis value </param>
            /// <param name="y"> y axis value </param>
            internal Point(double x, double y) {
                this.x = x;
                this.y = y;
            }
        }

        #endregion
    }
}
