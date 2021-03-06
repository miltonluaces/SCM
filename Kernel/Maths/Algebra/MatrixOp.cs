using System;
using System.Collections.Generic;
using System.Text;


namespace Maths {

    internal class MatrixOp {

        #region Funciones internal

        internal static int GetRows(double[,] mat) {
            return mat.GetLength(0);
        }

        internal static int GetCols(double[,] mat) {
            return mat.GetLength(1);
        }

        internal static double[,] Addition(double[,] A, double[,] B) {
            double[,] sum = new double[A.GetLength(0), A.GetLength(1)];
            for (int i = 0; i < A.GetLength(0); i++) {
                for (int j = 0; j < A.GetLength(1); j++) {
                    sum[i, j] = A[i, j] + B[i, j];
                }
            }
            return sum;
        }

        internal static double[,] Substraction(double[,] A, double[,] B) {
            double[,] subs = new double[A.GetLength(0), A.GetLength(1)];
            for (int i = 0; i < A.GetLength(0); i++) {
                for (int j = 0; j < A.GetLength(1); j++) {
                    subs[i, j] = A[i, j] - B[i, j];
                }
            }
            return subs;
        }

        internal static double[,] Multiply(double[,] A, double[,] B) {
            int fil = A.GetLength(0);
            int col = B.GetLength(1);
            int p = A.GetLength(1);

            double[,] prod = new double[fil, col];
            for (int i = 0; i < fil; i++) {
                for (int j = 0; j < col; j++) {
                    prod[i, j] = 0.0;
                    for (int k = 0; k < p; k++) {
                        prod[i, j] += A[i, k] * B[k, j];
                    }
                }
            }
            return prod;
        }

        internal static double[,] MultiplyT(double[,] A, double[,] B) {
            int p = A.GetLength(1);
            int fil = A.GetLength(0);
            int col = B.GetLength(1);
            double[,] prod = new double[fil, col];
            for (int i = 0; i < fil; i++)
                for (int j = i; j < col; j++) {
                    prod[i, j] = 0.0;
                    for (int k = 0; k < p; k++)
                        prod[i, j] += A[j, k] * B[k, i];
                    prod[j, i] = prod[i, j];
                }
            return prod;
        }

        internal static double[,] Transpose(double[,] mat) {
            double[,] trasp = new double[mat.GetLength(1), mat.GetLength(0)];
            for (int i = 0; i < mat.GetLength(0); i++) {
                for (int j = 0; j < mat.GetLength(1); j++) {
                    trasp[j, i] = mat[i, j];
                }
            }
            return trasp;
        }

        internal static double[,] MultiplyTranspose(double[,] A) {
            double[,] tr = Transpose(A);
            double[,] prod = Multiply(A, tr);
            return prod;
        }

        internal static double[,] Multiply(double[,] mat, double val) {
            double[,] prod = new double[mat.GetLength(0), mat.GetLength(1)];
            for (int i = 0; i < mat.GetLength(0); i++) {
                for (int j = 0; j < mat.GetLength(1); j++) {
                    prod[i, j] = mat[i, j] * val;
                }
            }
            return prod;
        }

        internal static void MakeSimetric(double[,] mat) {
            int row = mat.GetLength(0);
            int col = mat.GetLength(1);
            for (int c = 0; c < col; c++) {
                for (int r = c; r < row; r++) {
                    mat[c, r] = mat[r, c];
                }
            }
        }

        internal static double[,] Clone(double[,] mat) {
            double[,] clone = new double[mat.GetLength(0), mat.GetLength(1)];
            for (int i = 0; i < mat.GetLength(0); i++) {
                for (int j = 0; j < mat.GetLength(1); j++) {
                    clone[i, j] = mat[i, j];
                }
            }
            return clone;
        }

        internal static double ToScalar(double[,] mat) {
            if (mat.GetLength(0) != 1 || mat.GetLength(1) != 1) { throw new Exception("No_es_de_1x1"); }
            return mat[0, 0];
        }

        internal static bool IsZero(double[,] mat) {
            double sum = 0.0;
            for (int i = 0; i < mat.GetLength(0); i++) {
                for (int j = 0; j < mat.GetLength(1); j++) {
                    sum += mat[i, j];
                }
            }
            return sum == 0;
        }

        #endregion

        #region Funciones Privadas

        internal static double[,] MultiplyStrassen(double[,] A, double[,] B) {
            double[,] prod = new double[A.GetLength(0), B.GetLength(1)];

            //caso base
            if (A.GetLength(0) == 2) {
                prod = Multiply(A, B);
            }
            //caso recursivo
            else {
                double[,] A11 = GetCuadrante(A, 1);
                double[,] B11 = GetCuadrante(B, 1);
                double[,] A12 = GetCuadrante(A, 2);
                double[,] B12 = GetCuadrante(B, 2);
                double[,] A21 = GetCuadrante(A, 3);
                double[,] B21 = GetCuadrante(B, 3);
                double[,] A22 = GetCuadrante(A, 4);
                double[,] B22 = GetCuadrante(B, 4);

                double[,] P = MultiplyStrassen(Addition(A11, A22), Addition(B11, B22));
                double[,] Q = MultiplyStrassen(Addition(A21, A22), B11);
                double[,] R = MultiplyStrassen(A11, Substraction(B12, B22));
                double[,] S = MultiplyStrassen(A22, Substraction(B21, B11));
                double[,] T = MultiplyStrassen(Addition(A11, A12), B22);
                double[,] U = MultiplyStrassen(Substraction(A21, A11), Addition(B11, B12));
                double[,] V = MultiplyStrassen(Substraction(A12, A22), Addition(B21, B22));

                double[,] C11 = Substraction(Addition(Addition(P, S), V), T);
                double[,] C12 = Addition(R, T);
                double[,] C21 = Addition(Q, S);
                double[,] C22 = Substraction(Addition(Addition(P, R), U), Q);

                prod = Concat(C11, C12, C21, C22);
            }
            return prod;
        }

        private static double[,] GetCuadrante(double[,] mat, int pos) {
            int lado2 = mat.GetLength(0);
            int lado = lado2 / 2;
            double[,] cuad = new double[lado, lado];

            switch (pos) {
                case 1:
                    for (int i = 0; i < lado; i++) {
                        for (int j = 0; j < lado; j++) {
                            cuad[i, j] = mat[i, j];
                        }
                    }
                    break;
                case 2:
                    for (int i = 0; i < lado; i++) {
                        for (int j = lado; j < lado2; j++) {
                            cuad[i, j - lado] = mat[i, j];
                        }
                    }
                    break;
                case 3:
                    for (int i = lado; i < lado2; i++) {
                        for (int j = 0; j < lado; j++) {
                            cuad[i - lado, j] = mat[i, j];
                        }
                    }
                    break;
                case 4:
                    for (int i = lado; i < lado2; i++) {
                        for (int j = lado; j < lado2; j++) {
                            cuad[i - lado, j - lado] = mat[i, j];
                        }
                    }
                    break;
            }
            return cuad;
        }

        private static double[,] Concat(double[,] A, double[,] B, double[,] C, double[,] D) {
            int lado = A.GetLength(0);
            double[,] union = new double[lado * 2, lado * 2];

            for (int i = 0; i < lado; i++) {
                for (int j = 0; j < lado; j++) {
                    union[i, j] = A[i, j];
                    union[i, j + lado] = B[i, j];
                    union[i + lado, j] = C[i, j];
                    union[i + lado, j + lado] = D[i, j];
                }
            }
            return union;
        }

        private static bool IsStrassenLike(double[,] mat) {
            int i = 1;
            while (i <= 30) {
                i *= 2;
                if (mat.GetLength(0) == mat.GetLength(1) && mat.GetLength(0) == i) {
                    return true;
                }
            }
            return false;
        }


        #endregion

    }
}
