#region Imports

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace MachineLearning {

    #region Interface IQMatrix

    internal interface IQMatrix  {
        float[] GetQ(int column, int len);
        float[] GetQD();
        void SwapIndex(int i, int j);
    }

    #endregion

    #region Abstract class Kernel

    internal abstract class Kernel : IQMatrix  {

        #region Fields

        private Node[][] x;
        private double[] xSq;

        private KernelType type;
        private int degree;
        private double gamma;
        private double coeff0;

        #endregion

        #region Constructor

        internal Kernel(int l, Node[][] x_, Parameter param)  {
            this.type = param.KernelType;
            this.degree = param.Degree;
            this.gamma = param.Gamma;
            this.coeff0 = param.Coefficient0;

            this.x = (Node[][])x_.Clone();
            if (type == KernelType.RBF)  {
                this.xSq = new double[l];
                for (int i = 0; i < l; i++)
                    this.xSq[i] = dot(x[i], x[i]);
            }
            else this.xSq = null;
        }

        #endregion

        #region Methods

        public abstract float[] GetQ(int column, int len);
        public abstract float[] GetQD();

        public virtual void SwapIndex(int i, int j)  {
            x.SwapIndex(i, j);
            if (xSq != null) { xSq.SwapIndex(i, j);  }
        }

        private static double powi(double value, int times) {
            double tmp = value, ret = 1.0;

            for (int t = times; t > 0; t /= 2)
            {
                if (t % 2 == 1) ret *= tmp;
                tmp = tmp * tmp;
            }
            return ret;
        }

        internal double KernelFunction(int i, int j)  {
            switch (type)
            {
                case KernelType.LINEAR:       return dot(x[i], x[j]);
                case KernelType.POLY:         return powi(gamma * dot(x[i], x[j]) + coeff0, degree);
                case KernelType.RBF:          return Math.Exp(-gamma * (xSq[i] + xSq[j] - 2 * dot(x[i], x[j])));
                case KernelType.SIGMOID:     return Math.Tanh(gamma * dot(x[i], x[j]) + coeff0);
                case KernelType.PRECOMPUTED: return x[i][(int)(x[j][0].Value)].Value;
                default: return 0;
            }
        }

        private static double dot(Node[] xNodes, Node[] yNodes)  {
            double sum = 0;
            int xlen = xNodes.Length;
            int ylen = yNodes.Length;
            int i = 0;
            int j = 0;
            Node x = xNodes[0];
            Node y = yNodes[0];
            while (true)  {
                if (x.index == y.index)  {
                    sum += x.value * y.value;
                    i++;
                    j++;
                    if (i < xlen && j < ylen)    {
                        x = xNodes[i];
                        y = yNodes[j];
                    }
                    else if (i < xlen)   {
                        x = xNodes[i];
                        break;
                    }
                    else if (j < ylen)   {
                        y = yNodes[j];
                        break;
                    }
                    else break;
                }
                else  {
                    if (x.index > y.index)  {
                        ++j;
                        if (j < ylen)
                            y = yNodes[j];
                        else break;
                    }
                    else  {
                        ++i;
                        if (i < xlen)
                            x = xNodes[i];
                        else break;
                    }
                }
            }
            return sum;
        }

        private static double computeSquaredDistance(Node[] xNodes, Node[] yNodes)   {
            Node x = xNodes[0];
            Node y = yNodes[0];
            int xLength = xNodes.Length;
            int yLength = yNodes.Length;
            int xIndex = 0;
            int yIndex = 0;
            double sum = 0;

            while (true) {
                if (x.index == y.index)  {
                    double d = x.value - y.value;
                    sum += d * d;
                    xIndex++;
                    yIndex++;
                    if (xIndex < xLength && yIndex < yLength)  {
                        x = xNodes[xIndex];
                        y = yNodes[yIndex];
                    }
                    else if(xIndex < xLength){
                        x = xNodes[xIndex];
                        break;
                    }
                    else if(yIndex < yLength){
                        y = yNodes[yIndex];
                        break;
                    }else break;
                }
                else if (x.index > y.index)  {
                    sum += y.value * y.value;
                    if (++yIndex < yLength)
                        y = yNodes[yIndex];
                    else break;
                }
                else   {
                    sum += x.value * x.value;
                    if (++xIndex < xLength)
                        x = xNodes[xIndex];
                    else break;
                }
            }

            for (; xIndex < xLength; xIndex++)   {
                double d = xNodes[xIndex].value;
                sum += d * d;
            }

            for (; yIndex < yLength; yIndex++) {
                double d = yNodes[yIndex].value;
                sum += d * d;
            }

            return sum;
        }

        public static double KernelFunction(Node[] x, Node[] y, Parameter param)  {
            switch (param.KernelType)   {
                case KernelType.LINEAR:
                    return dot(x, y);
                case KernelType.POLY:
                    return powi(param.Degree * dot(x, y) + param.Coefficient0, param.Degree);
                case KernelType.RBF:
                    {
                        double sum = computeSquaredDistance(x, y);
                        return Math.Exp(-param.Gamma * sum);
                    }
                case KernelType.SIGMOID:
                    return Math.Tanh(param.Gamma * dot(x, y) + param.Coefficient0);
                case KernelType.PRECOMPUTED:
                    return x[(int)(y[0].Value)].Value;
                default:
                    return 0;
            }
        }

        #endregion

     }

    #endregion
}
