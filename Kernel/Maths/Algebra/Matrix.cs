#region Imports

using System;
using System.Text;

#endregion

namespace Maths {

    //Matrix class for linear algebra
    internal class Matrix : IMatrix {

        #region Fields

        private double[][] data;
        private int rows;
        private int cols;

        private RndGenerator rndGen;

        #endregion

        #region Constructors

        //Constructs an empty matrix of the given size
        internal Matrix(int rows, int cols) {
            this.rows = rows;
            this.cols = cols;
            this.data = new double[rows][];
            for (int i = 0; i < rows; i++)  { this.data[i] = new double[cols]; }
            rndGen = new RndGenerator();
        }

        //Constructs a matrix of the given size and assigns a given value to all diagonal elements
        internal Matrix(int rows, int cols, double diagValue) {
            this.rows = rows;
            this.cols = cols;
            this.data = new double[rows][];
            for (int i = 0; i < rows; i++)  { data[i] = new double[cols];  }
            for (int i = 0; i < rows; i++)  { data[i][i] = diagValue; }
            rndGen = new RndGenerator();
        }

        //Constructs a matrix from the given array
        internal Matrix(double[][] data) {
            this.rows = data.Length;
            this.cols = data[0].Length;
            for (int i = 0; i < rows; i++)  {
                if (data[i].Length != cols)  { throw new Exception("Error: different columns quantity");  }
            }
            this.data = data;
            rndGen = new RndGenerator();
        }

        //Constructs a matrix from the given array
        internal Matrix(double[,] data) {
            this.rows = data.GetLength(0);
            this.cols = data.GetLength(1);

            this.data = new double[rows][];
            for (int i = 0; i < rows; i++) {
                this.data[i] = new double[cols]; 
                for (int j = 0; j < cols; j++) { this.data[i][j] = data[i, j]; }
            }
            rndGen = new RndGenerator();
        }

        //Construct a matrix row vector or a column vector from a vector
        internal Matrix(double[] vector, bool isRow) {
            if (isRow) {
                this.rows = 1;
                this.cols = vector.Length;
                this.data = new double[1][];
                this.data[0] = new double[cols];
                for (int j = 0; j < cols; j++) { this.data[0][j] = vector[j]; }
            }
            else {
                this.rows = vector.Length;
                this.cols = 1;
                this.data = new double[rows][];
                this.data[0] = new double[1];
                for (int i = 0; i < rows; i++) { this.data[i][0] = vector[i]; }
            }
            rndGen = new RndGenerator();
        }
        
        #endregion

        #region Properties

        double[][] Array  {
            get { return data; }
        }

        int IMatrix.Rows {
            get { return rows; }
        }

        int IMatrix.Cols {
            get { return cols; }
        }

        bool IMatrix.IsSquare {
            get { return (rows == cols); }
        }

        bool IMatrix.IsZero {
            get  {
                double s = 0.0;
                for (int i = 0; i < rows; i++) {
                    for (int j = 0; j < cols; j++) { s += data[i][j]; }
                }
                return (s == 0.0);
            }
        }

        bool IMatrix.IsSymmetric {
            get  {
                if (!((IMatrix)this).IsSquare) { return false; }
                for (int i = 0; i < rows; i++)  {
                    for (int j = 0; j <= i; j++)  {
                        if (data[i][j] != data[j][i])  { return false; }
                    }
                }
                return true;
            }
        }

        //Access the value at the given location. First index is Row, second index is column
        double IMatrix.this[int i, int j] {
            set { data[i][j] = value; }
            get { return data[i][j]; }
        }

        #endregion

        #region Internal Methods

        #region Basic Operations

        //Sum
        IMatrix IMatrix.Addition(IMatrix B) {
            if ((rows != B.Rows) || (cols != B.Cols)) { throw new Exception("Error: dimensions not match"); }
            Matrix X = new Matrix(rows, cols);
            double[][] x = X.Array;
            for (int i = 0; i < rows; i++) {
                for (int j = 0; j < cols; j++) { x[i][j] = data[i][j] + B[i, j]; }
            }
            return X;
        }

        //Subtraction
        IMatrix IMatrix.Subtraction(IMatrix B) {
            if ((rows != B.Rows) || (cols != B.Cols)) { throw new Exception("Error: dimensions not match"); }
            Matrix X = new Matrix(rows, cols);
            double[][] x = X.Array;
            for (int i = 0; i < rows; i++) {
                for (int j = 0; j < cols; j++) { x[i][j] = data[i][j] - B[i, j]; }
            }
            return X;
        }

        //Product * scalar
        IMatrix IMatrix.Multiply(double s) {
            Matrix X = new Matrix(rows, cols);
            double[][] x = X.Array;
            for (int i = 0; i < rows; i++) {
                for (int j = 0; j < cols; j++) { x[i][j] = data[i][j] * s; }
            }
            return X;
        }

        //Product * matrix
        IMatrix IMatrix.Multiply(IMatrix B) {
            if (B.Rows != this.cols) { throw new ArgumentException("Matrix dimensions are not valid"); }

            int columns = B.Cols;
            Matrix X = new Matrix(rows, columns);
            double[][] x = X.Array;

            int size = this.cols;
            double[] column = new double[size];
            for (int j = 0; j < columns; j++) {
                for (int k = 0; k < size; k++) { column[k] = B[k, j]; }
                for (int i = 0; i < rows; i++) {
                    double[] row = data[i];
                    double s = 0;
                    for (int k = 0; k < size; k++) { s += row[k] * column[k]; }
                    x[i][j] = s;
                }
            }
            return X;
        }

        //Transpose
        internal void Transp() {
            double[][] transp = new double[rows][];
            for (int i = 0; i < rows; i++) {
                transp[i] = new double[cols]; 
                for (int j = 0; j < cols; j++) { transp[j][i] = data[i][j]; }
            }
            data = transp;
        }

        //Get Transposed
        IMatrix IMatrix.Transpose() {
            Matrix X = new Matrix(cols, rows);
            double[][] x = X.Array;
            for (int i = 0; i < rows; i++) {
                for (int j = 0; j < cols; j++) { x[j][i] = data[i][j]; }
            }
            return X;
        }

        internal Matrix t() { return (Matrix)((IMatrix)this).Transpose(); }

        //sustituir por exponenciacion binaria
        internal Matrix Pow(int n) {
            if (n == 1) { return this; }
            Matrix X = new Matrix(this.data);
            Matrix Pow = new Matrix(this.data); 
            for (int i = 1; i < n; i++) {  Pow = Pow * X; }
            return Pow;
        }
        
        #endregion

        #region Submatrix Operations
        
        //Submatrix: i0: Start row, i1 End row ; j0 Start column j1 End column
        IMatrix IMatrix.Submatrix(int i0, int i1, int j0, int j1) {
            Matrix X = new Matrix(i1 - i0 + 1, j1 - j0 + 1);
            double[][] x = X.Array;
            for (int i = i0; i <= i1; i++)
                for (int j = j0; j <= j1; j++) { x[i - i0][j - j0] = data[i][j]; }
            return X;
        }

        /// <summary>Returns a sub matrix extracted from the current matrix.</summary>
        /// <param name="r">Array of row indices</param>
        /// <param name="c">Array of column indices</param>
        /// <returns> IMatrix with the result of the operation </returns>
        /// <returns> the result matrix </returns>
        IMatrix IMatrix.Submatrix(int[] r, int[] c) {
            Matrix X = new Matrix(r.Length, c.Length);
            double[][] x = X.Array;
            for (int i = 0; i < r.Length; i++)
                for (int j = 0; j < c.Length; j++) { x[i][j] = data[r[i]][c[j]]; }
            return X;
        }

        /// <summary>Returns a sub matrix extracted from the current matrix.</summary>
        /// <param name="i0">Starttial row index</param>
        /// <param name="i1">End row index</param>
        /// <param name="c">Array of row indices</param>
        /// <returns> IMatrix with the result of the operation </returns>
        IMatrix IMatrix.Submatrix(int i0, int i1, int[] c) {
            Matrix X = new Matrix(i1 - i0 + 1, c.Length);
            double[][] x = X.Array;
            for (int i = i0; i <= i1; i++)
                for (int j = 0; j < c.Length; j++) { x[i - i0][j] = data[i][c[j]]; }
            return X;
        }

        /// <summary>Returns a sub matrix extracted from the current matrix.</summary>
        /// <param name="r">Array of row indices</param>
        /// <param name="j0">Start column index</param>
        /// <param name="j1">End column index</param>
        /// <returns> IMatrix with the result of the operation </returns>
        IMatrix IMatrix.Submatrix(int[] r, int j0, int j1) {
            Matrix X = new Matrix(r.Length, j1 - j0 + 1);
            double[][] x = X.Array;
            for (int i = 0; i < r.Length; i++)
                for (int j = j0; j <= j1; j++) { x[i][j - j0] = data[r[i]][j]; }
            return X;
        }

        #endregion

        #region Advanced Operations

        //Product * Matrix transpose: Reverse: if the matrix should be reversed
        IMatrix IMatrix.MultiplyTranspose(bool reverse, int precision) {
            Matrix X;

            if (reverse) {
                X = new Matrix(this.cols, this.cols);
                for (int c1 = 0; c1 < this.cols; c1++) {
                    for (int c2 = c1; c2 < this.cols; c2++) {
                        ((IMatrix)X)[c1, c2] = 0.0;
                        for (int r = 0; r < this.rows; r++) { ((IMatrix)X)[c1, c2] += this.data[r][c1] * this.data[r][c2]; }
                    }
                }
                for (int r = 0; r < this.cols; r++) {
                    for (int c = r; c < this.cols; c++) { ((IMatrix)X)[c, r] = ((IMatrix)X)[r, c]; }
                }
            }
            else {
                X = new Matrix(this.rows, this.rows);
                for (int r1 = 0; r1 < this.cols; r1++)
                    for (int r2 = r1; r2 < this.cols; r2++)
                        for (int c = 0; c < this.cols; c++)
                            ((IMatrix)X)[r1, r2] += this.data[r1][c] * this.data[r2][c];
                for (int r = 0; r < this.rows; r++)
                    for (int c = r; c < this.rows; c++)
                        ((IMatrix)X)[c, r] = ((IMatrix)X)[r, c];
            }
            return X;
        }

        //Makes this matrix simetric by copying the right upper diagonal into the left down diagonal
        IMatrix IMatrix.MakeSimetric() {
            for (int col = 0; col < this.cols; col++) {
                for (int row = col; row < this.rows; row++) { this.data[col][row] = this.data[row][col]; }
            }
            return (this);
        }

        //Matrix-matrix triangular multiplication.Just Multiplies to get the left down diagonal and then copies to the right upper diagonal
        IMatrix IMatrix.MultiplyT(IMatrix B) {
            if (B.Rows != this.cols) { throw new Exception("Matrix dimensions are not valid"); }
            IMatrix X = ((IMatrix)this).Multiply(B);
            X.MakeSimetric();
            return X;
        }

        //Matrix-matrix triangular multiplication. Just Multiplies to get the left down diagonal and then copies to the right upper diagonal 
        void IMatrix.CopyToRightUpperCorner() {
            if (((IMatrix)this).Rows != this.cols) {
                throw new ArgumentException("Matrix dimensions are not valid");
            }

            for (int Row = 0; Row < this.rows; Row++)
                for (int Col = Row; Col < this.cols; Col++)
                    ((IMatrix)this)[Col, Row] = ((IMatrix)this)[Row, Col];
        }

        #endregion

        #region Norms

        //One Norm : the maximum column sum
        double IMatrix.Norm1 {
            get {
                double f = 0;
                for (int j = 0; j < cols; j++) {
                    double s = 0;
                    for (int i = 0; i < rows; i++) { s += Math.Abs(data[i][j]); }
                    f = Math.Max(f, s);
                }
                return f;
            }
        }

        //Infinity Norm for the matrix: the maximum row sum.</value>
        double IMatrix.InfinityNorm {
            get {
                double f = 0;
                for (int i = 0; i < rows; i++) {
                    double s = 0;
                    for (int j = 0; j < cols; j++) { s += Math.Abs(data[i][j]); }
                    f = Math.Max(f, s);
                }
                return f;
            }
        }

        //Frobenius Norm for the matrix: square root of sum of squares of all elements
        double IMatrix.FrobeniusNorm {
            get {
                double f = 0;
                for (int i = 0; i < rows; i++) {
                    for (int j = 0; j < cols; j++) { f = MathHelper.Hypotenuse(f, data[i][j]); }
                }
                return f;
            }
        }

        //Trace : sum of the diagonal elements
        double IMatrix.Trace {
            get {
                double trace = 0;
                for (int i = 0; i < Math.Min(rows, cols); i++) { trace += data[i][i]; }
                return trace;
            }
        }
        
        #endregion

        #region Solving

        //Solve A * X = B: rhs: >Right hand side matrix with as many rows as A and any number of columns
        IMatrix IMatrix.Solve(IMatrix rhs) { return (rows == cols) ? ((IMatrix)this).GetLuDecomposition().Solve(rhs) : ((IMatrix)this).GetQrDecomposition().Solve(rhs); }

        //Inverse for square, pseudoinverse otherwise
        IMatrix IMatrix.Inverse {
            get { return ((IMatrix)this).Solve(Diagonal(rows, rows, 1.0)); }
        }

        //Determinant (for square matrices)
        double IMatrix.Determinant {
            get { return ((IMatrix)this).GetLuDecomposition().Determinant; }
        }

        #endregion

        #region Descompositions

        //Cholesky decomposition
        ICholeskyDecomposition IMatrix.GetCholeskyDecomposition() { return new CholeskyDecomposition(this); }

        //LU decomposition for this matrix
        ILuDecomposition IMatrix.GetLuDecomposition() { return new LuDecomposition(this); }

        //Singular value decomposition
        ISingularValueDecomposition IMatrix.GetSingularValueDecomposition() { return new SingularValueDecomposition(this); }

        //QR decomposition
        IQrDecomposition IMatrix.GetQrDecomposition() { return new QrDecomposition(this); }

        //Eigenvalue decomposition for this matrix
        IEigenvalueDecomposition IMatrix.GetEigenvalueDecomposition() { return new EigenvalueDecomposition(this); }

        #endregion

        #region Setting

        //Sets a matrix from the given array
        internal void Set(double[,] data) {
            if (this.rows != data.GetLength(0) || this.cols != data.GetLength(1)) { throw new Exception("Error. Wrong dimensions"); }
            for (int i = 0; i < rows; i++) {
                for (int j = 0; j < cols; j++) { this.data[i][j] = data[i, j]; }
            }
        }
        
        //Set a matrix row vector or a column vector from a vector
        internal void Set(double[] vector, bool isRow) {
            if (isRow) {
                if(this.rows != 1 || this.cols != vector.Length) { throw new Exception("Error. Wrong dimensions"); }
                for (int j = 0; j < cols; j++) { this.data[0][j] = vector[j]; }
            }
            else {
                if( this.rows != vector.Length || this.cols != 1) { throw new Exception("Error. Wrong dimensions"); }
                for (int i = 0; i < rows; i++) { this.data[i][0] = vector[i]; }
            }
        }


        //Sets all cells of a matrix to the same value
        bool IMatrix.Set(double v) {
            for (int c = 0; c < this.cols; c++) {
                for (int r = 0; r < this.rows; r++) { this.data[r][c] = v; }
            }
            return true;
        }

        //Sets all cells with random uniform values
        internal void Set(int rows, int columns) {
            for (int i = 0; i < rows; i++) {
                for (int j = 0; j < columns; j++) { this.data[i][j] = MathHelper.Random(); }
            }
        }

        //Sets all cells with random uniform values
        internal void SetUniform(double min, double max) {
            for (int i = 0; i < rows; i++) {
                for (int j = 0; j < cols; j++) { this.data[i][j] = rndGen.NextDouble(min,max); }
            }
        }

        //Sets all cells with random normal values
        internal void SetNormal(double m, double s) {
            for (int i = 0; i < rows; i++) {
                for (int j = 0; j < cols; j++) { this.data[i][j] = rndGen.NextNormal(m, s); }
            }
        }

        //Sets all cells with random normal values
        internal void SetNormal(double[] m, double[,] C) {
            for (int i = 0; i < rows; i++) {
                for (int j = 0; j < cols; j++) { this.data[i][j] = rndGen.NextNormal(m[i], C[i,i]); }
            }
        }
        
        //Sets all cells of a matrix to value
        double[,] IMatrix.ConvertToDouble() {
            double[,] m = new double[this.rows, this.cols];
            for (int c = 0; c < this.cols; c++) {
                for (int r = 0; r < this.rows; r++) { m[r, c] = this.data[r][c]; }
            }
            return m;
        }

        //Clone
        IMatrix IMatrix.Clone() {
            Matrix X = new Matrix(rows, cols);
            double[][] x = X.Array;
            for (int i = 0; i < rows; i++)
                for (int j = 0; j < cols; j++) { x[i][j] = data[i][j]; }
            return X;
        }

        //Diagonal matrix of the given size
        internal static IMatrix Diagonal(int rows, int columns, double diagValue) {
            Matrix X = new Matrix(rows, columns);
            double[][] x = X.Array;
            for (int i = 0; i < rows; i++) {
                for (int j = 0; j < columns; j++) { x[i][j] = ((i == j) ? diagValue : 0.0); }
            }
            return X;
        }
        
        //Matrix filled with random values
        internal static IMatrix Random(int rows, int columns) {
            Matrix X = new Matrix(rows, columns);
            double[][] x = X.Array;
            for (int i = 0; i < rows; i++) {
                for (int j = 0; j < columns; j++) { x[i][j] = MathHelper.Random(); }
            }
            return X;
        }
        
        #endregion

        #region Modification

        //Unary minus: Matrix with the result of the operation
        internal IMatrix UnaryMinus() {
            int rows = this.rows;
            int columns = this.cols;
            Matrix X = new Matrix(rows, columns);
            double[][] x = X.Array;
            for (int i = 0; i < rows; i++)
                for (int j = 0; j < columns; j++) { x[i][j] = -data[i][j]; }
            return X;
        }

        //Convert to scalar
        double IMatrix.ToScalar() {
            if ((((IMatrix)this).Rows != ((IMatrix)this).Cols) || (((IMatrix)this).Rows != 1)) { throw new Exception("Matrix is not a scalar"); }
            return ((IMatrix)this)[0, 0]; 
        }

        // Rounds all values of a matrix to precision (number of decimals). If a value is less than precision, then value equals 1 / (10 ** Precision)
        IMatrix IMatrix.Round(int precision) {
            for (int r = 0; r < this.rows; r++) {
                for (int c = 0; c < this.cols; c++) {
                    if ((Math.Round(((IMatrix)this)[r, c], precision) == 0) && (((IMatrix)this)[r, c] != 0)) { ((IMatrix)this)[r, c] = 1 / Math.Pow(10, precision); }
                    else { ((IMatrix)this)[r, c] = Math.Round(((IMatrix)this)[r, c], precision); }
                }
            }
            return this;
        }

        #endregion

        #region Misc

        public override string ToString() {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < rows; i++) {
                for (int j = 0; j < cols; j++) { builder.Append(data[i][j].ToString("0.##") + "\t"); }
                builder.Append(Environment.NewLine);
            }
            return builder.ToString();
        }

        #endregion

        #endregion

        #region Operators override

        public static Matrix operator +(Matrix A, Matrix B) {
            IMatrix res = ((IMatrix) A).Addition(B);
            return (Matrix)res;
        }

        public  static Matrix operator -(Matrix A, Matrix B) {
            IMatrix res = ((IMatrix) A).Subtraction(B);
            return (Matrix)res;
        }

        public static Matrix operator *(Matrix A, Matrix B) {
            IMatrix res = ((IMatrix) A).Multiply(B);
            return (Matrix)res;
        }

        #endregion

        #region Inner Classes

        #region Clase CholeskyDecomposition

        private class CholeskyDecomposition : ICholeskyDecomposition
        {
            private Matrix L;
            private bool isSymmetric;
            private bool isPositiveDefinite;

            internal CholeskyDecomposition(Matrix A)
            {
                if (!((IMatrix)A).IsSquare)
                {
                    throw new ArgumentNullException("Matrix is not square");
                }

                int dimension = ((IMatrix)A).Rows;
                L = new Matrix(dimension, dimension);

                double[][] a = A.Array;
                double[][] l = L.Array;

                isPositiveDefinite = true;
                isSymmetric = true;

                for (int j = 0; j < dimension; j++)
                {
                    double[] Lrowj = l[j];
                    double d = 0.0;
                    for (int k = 0; k < j; k++)
                    {
                        double[] Lrowk = l[k];
                        double s = 0.0;
                        for (int i = 0; i < k; i++)
                        {
                            s += Lrowk[i] * Lrowj[i];
                        }
                        Lrowj[k] = s = (a[j][k] - s) / l[k][k];
                        d = d + s * s;
                        isSymmetric = isSymmetric & (a[k][j] == a[j][k]);
                    }

                    d = a[j][j] - d;
                    isPositiveDefinite = isPositiveDefinite & (d > 0.0);
                    l[j][j] = Math.Sqrt(Math.Max(d, 0.0));
                    for (int k = j + 1; k < dimension; k++)
                        l[j][k] = 0.0;
                }
            }

            /// <summary> If the matrix is symmetric </summary>
            bool ICholeskyDecomposition.IsSymmetric {
                get { return isSymmetric; }
            }

            /// <summary> If the matrix is positive definite </summary>
            bool ICholeskyDecomposition.IsPositiveDefinite
            {
                get { return isPositiveDefinite; }
            }

            /// <summary> Get the L (triangular factor) </summary>
            IMatrix ICholeskyDecomposition.LeftTriangularFactor
            {
                get { return L; }
            }

            /// <summary> Solve system </summary>
            /// <param name="rhs"> matrix with coefficents </param>
            /// <returns> IMatrix that contains the result </returns>
            IMatrix ICholeskyDecomposition.Solve(IMatrix rhs)
            {
                if (rhs.Rows != ((IMatrix)L).Rows)
                {
                    throw new ArgumentException("Matrix dimensions do not match");
                }
                if (!isSymmetric)
                {
                    throw new InvalidOperationException("Matrix is not symmetric");
                }
                if (!isPositiveDefinite)
                {
                    throw new InvalidOperationException("Matrix is not positive definite");
                }

                int dimension = ((IMatrix)L).Rows;
                int count = rhs.Cols;

                IMatrix B = (IMatrix)rhs.Clone();
                double[][] l = L.Array;

                // Solve L*Y = B;
                for (int k = 0; k < ((IMatrix)L).Rows; k++)
                {
                    for (int i = k + 1; i < dimension; i++)
                    {
                        for (int j = 0; j < count; j++)
                        {
                            B[i, j] -= B[k, j] * l[i][k];
                        }
                    }

                    for (int j = 0; j < count; j++)
                    {
                        B[k, j] /= l[k][k];
                    }
                }

                // Solve L'*X = Y;
                for (int k = dimension - 1; k >= 0; k--)
                {
                    for (int j = 0; j < count; j++)
                    {
                        B[k, j] /= l[k][k];
                    }

                    for (int i = 0; i < k; i++)
                    {
                        for (int j = 0; j < count; j++)
                        {
                            B[i, j] -= B[k, j] * l[k][i];
                        }
                    }
                }

                return B;
            }
        }

        #endregion

        #region Clase LuDecomposition

        private class LuDecomposition : ILuDecomposition
        {
            private Matrix LU;
            private int pivotSign;
            private int[] pivotVector;

            internal LuDecomposition(Matrix A)
            {
                LU = (Matrix)((IMatrix)A).Clone();
                double[][] lu = LU.Array;
                int rows = ((IMatrix)A).Rows;
                int columns = ((IMatrix)A).Cols;
                pivotVector = new int[rows];
                for (int i = 0; i < rows; i++)
                    pivotVector[i] = i;
                pivotSign = 1;
                double[] LUrowi;
                double[] LUcolj = new double[rows];

                // Outer loop.
                for (int j = 0; j < columns; j++)
                {
                    // Make a copy of the j-th column to localize references.
                    for (int i = 0; i < rows; i++)
                        LUcolj[i] = lu[i][j];

                    // Apply previous transformations.
                    for (int i = 0; i < rows; i++)
                    {
                        LUrowi = lu[i];

                        // Most of the time is spent in the following dot product.
                        int kmax = Math.Min(i, j);
                        double s = 0.0;
                        for (int k = 0; k < kmax; k++)
                            s += LUrowi[k] * LUcolj[k];
                        LUrowi[j] = LUcolj[i] -= s;
                    }

                    // Find pivot and exchange if necessary.
                    int p = j;
                    for (int i = j + 1; i < rows; i++)
                        if (Math.Abs(LUcolj[i]) > Math.Abs(LUcolj[p]))
                            p = i;

                    if (p != j)
                    {
                        for (int k = 0; k < columns; k++)
                        {
                            double t = lu[p][k];
                            lu[p][k] = lu[j][k];
                            lu[j][k] = t;
                        }

                        int v = pivotVector[p];
                        pivotVector[p] = pivotVector[j];
                        pivotVector[j] = v;

                        pivotSign = -pivotSign;
                    }

                    // Compute multipliers.

                    if (j < rows & lu[j][j] != 0.0)
                    {
                        for (int i = j + 1; i < rows; i++)
                        {
                            lu[i][j] /= lu[j][j];
                        }
                    }
                }
            }

            bool ILuDecomposition.IsNonSingular
            {
                get
                {
                    for (int j = 0; j < ((IMatrix)LU).Cols; j++)
                        if (((IMatrix)LU)[j, j] == 0)
                            return false;
                    return true;
                }
            }

            double ILuDecomposition.Determinant
            {
                get
                {
                    if (((IMatrix)LU).Rows != ((IMatrix)LU).Cols)
                        throw new ArgumentException("Matrix must be square");
                    double determinant = (double)pivotSign;
                    for (int j = 0; j < ((IMatrix)LU).Cols; j++)
                        determinant *= ((IMatrix)LU)[j, j];
                    return determinant;
                }
            }

            IMatrix ILuDecomposition.LowerTriangularFactor
            {
                get
                {
                    int rows = ((IMatrix)LU).Rows;
                    int columns = ((IMatrix)LU).Cols;
                    Matrix X = new Matrix(rows, columns);
                    for (int i = 0; i < rows; i++)
                        for (int j = 0; j < columns; j++)
                            if (i > j)
                                ((IMatrix)X)[i, j] = ((IMatrix)LU)[i, j];
                            else if (i == j)
                                ((IMatrix)X)[i, j] = 1.0;
                            else
                                ((IMatrix)X)[i, j] = 0.0;
                    return X;
                }
            }

            IMatrix ILuDecomposition.UpperTriangularFactor
            {
                get
                {
                    int rows = ((IMatrix)LU).Rows;
                    int columns = ((IMatrix)LU).Cols;
                    Matrix X = new Matrix(rows, columns);
                    for (int i = 0; i < rows; i++)
                        for (int j = 0; j < columns; j++)
                            if (i <= j)
                                ((IMatrix)X)[i, j] = ((IMatrix)LU)[i, j];
                            else
                                ((IMatrix)X)[i, j] = 0.0;
                    return X;
                }
            }

            double[] ILuDecomposition.PivotPermutationVector
            {
                get
                {
                    int rows = ((IMatrix)LU).Rows;
                    double[] p = new double[rows];
                    for (int i = 0; i < rows; i++)
                        p[i] = (double)pivotVector[i];
                    return p;
                }
            }

            /// <summary>Solves a set of equation systems of type <c>A * X = B</c>.</summary>
            /// <param name="B"> the other matrix </param>
            /// <returns> result Matrix </returns>
            IMatrix ILuDecomposition.Solve(IMatrix B)
            {
                if (B.Rows != ((IMatrix)LU).Rows)
                    throw new ArgumentException("Invalid matrix dimensions");
                if (!((ILuDecomposition)this).IsNonSingular)
                    throw new InvalidOperationException("Matrix is singular");

                // Copy right hand side with pivoting
                int count = B.Cols;
                IMatrix X = B.Submatrix(pivotVector, 0, count - 1);

                int rows = ((IMatrix)LU).Rows;
                int columns = ((IMatrix)LU).Cols;
                double[][] lu = LU.Array;

                // Solve L*Y = B(piv,:)
                for (int k = 0; k < columns; k++)
                {
                    for (int i = k + 1; i < columns; i++)
                    {
                        for (int j = 0; j < count; j++)
                        {
                            X[i, j] -= X[k, j] * lu[i][k];
                        }
                    }
                }

                // Solve U*X = Y;
                for (int k = columns - 1; k >= 0; k--)
                {
                    for (int j = 0; j < count; j++)
                    {
                        X[k, j] /= lu[k][k];
                    }

                    for (int i = 0; i < k; i++)
                    {
                        for (int j = 0; j < count; j++)
                        {
                            X[i, j] -= X[k, j] * lu[i][k];
                        }
                    }
                }

                return X;
            }
        }
        #endregion

        #region QrDecomposition

        private class QrDecomposition : IQrDecomposition
        {
            private Matrix QR;
            private double[] Rdiag;

            internal QrDecomposition(Matrix A)
            {
                QR = (Matrix)((IMatrix)A).Clone();
                double[][] qr = QR.Array;
                int m = ((IMatrix)A).Rows;
                int n = ((IMatrix)A).Cols;
                Rdiag = new double[n];

                for (int k = 0; k < n; k++)
                {
                    // Compute 2-norm of df-th column without under/overflow.
                    double nrm = 0;
                    for (int i = k; i < m; i++)
                        nrm = MathHelper.Hypotenuse(nrm, qr[i][k]);

                    if (nrm != 0.0)
                    {
                        // Form df-th Householder vector.
                        if (qr[k][k] < 0)
                            nrm = -nrm;
                        for (int i = k; i < m; i++)
                            qr[i][k] /= nrm;
                        qr[k][k] += 1.0;

                        // Apply transformation to remaining columns.
                        for (int j = k + 1; j < n; j++)
                        {
                            double s = 0.0;
                            for (int i = k; i < m; i++)
                                s += qr[i][k] * qr[i][j];
                            s = -s / qr[k][k];
                            for (int i = k; i < m; i++)
                                qr[i][j] += s * qr[i][k];
                        }
                    }
                    Rdiag[k] = -nrm;
                }
            }

            IMatrix IQrDecomposition.Solve(IMatrix rhs)
            {
                if (rhs.Rows != ((IMatrix)QR).Rows)
                    throw new ArgumentException("Matrix row dimensions must agree");
                if (!((IQrDecomposition)this).IsFullRank)
                    throw new InvalidOperationException("Matrix is rank deficient");

                // Copy right hand side
                int count = rhs.Cols;
                IMatrix X = rhs.Clone();
                int m = ((IMatrix)QR).Rows;
                int n = ((IMatrix)QR).Cols;
                double[][] qr = QR.Array;

                // Compute Y = transpose(Q)*B
                for (int k = 0; k < n; k++)
                {
                    for (int j = 0; j < count; j++)
                    {
                        double s = 0.0;
                        for (int i = k; i < m; i++)
                            s += qr[i][k] * X[i, j];
                        s = -s / qr[k][k];
                        for (int i = k; i < m; i++)
                            X[i, j] += s * qr[i][k];
                    }
                }

                // Solve R*X = Y;
                for (int k = n - 1; k >= 0; k--)
                {
                    for (int j = 0; j < count; j++)
                        X[k, j] /= Rdiag[k];

                    for (int i = 0; i < k; i++)
                        for (int j = 0; j < count; j++)
                            X[i, j] -= X[k, j] * qr[i][k];
                }

                return X.Submatrix(0, n - 1, 0, count - 1);
            }

            bool IQrDecomposition.IsFullRank
            {
                get
                {
                    int columns = ((IMatrix)QR).Cols;
                    for (int j = 0; j < columns; j++)
                        if (Rdiag[j] == 0)
                            return false;
                    return true;
                }
            }

            IMatrix IQrDecomposition.UpperTriangularFactor
            {
                get
                {
                    int n = ((IMatrix)QR).Cols;
                    Matrix X = new Matrix(n, n);
                    double[][] x = X.Array;
                    double[][] qr = QR.Array;
                    for (int i = 0; i < n; i++)
                        for (int j = 0; j < n; j++)
                            if (i < j)
                                x[i][j] = qr[i][j];
                            else if (i == j)
                                x[i][j] = Rdiag[i];
                            else
                                x[i][j] = 0.0;

                    return X;
                }
            }

            IMatrix IQrDecomposition.OrthogonalFactor
            {
                get
                {
                    Matrix X = new Matrix(((IMatrix)QR).Rows, ((IMatrix)QR).Cols);
                    double[][] x = X.Array;
                    double[][] qr = QR.Array;
                    for (int k = ((IMatrix)QR).Cols - 1; k >= 0; k--)
                    {
                        for (int i = 0; i < ((IMatrix)QR).Rows; i++)
                            x[i][k] = 0.0;

                        x[k][k] = 1.0;
                        for (int j = k; j < ((IMatrix)QR).Cols; j++)
                        {
                            if (qr[k][k] != 0)
                            {
                                double s = 0.0;
                                for (int i = k; i < ((IMatrix)QR).Rows; i++)
                                    s += qr[i][k] * x[i][j];
                                s = -s / qr[k][k];
                                for (int i = k; i < ((IMatrix)QR).Rows; i++)
                                    x[i][j] += s * qr[i][k];
                            }
                        }
                    }
                    return X;
                }
            }
        }

        #endregion QrDecomposition

        #region SingularValueDecomposition

        private class SingularValueDecomposition : ISingularValueDecomposition
        {
            private Matrix U;
            private Matrix V;
            private double[] s; // singular values
            private int m;
            private int n;

            internal SingularValueDecomposition(Matrix A)
            {
                Matrix copy = (Matrix)((IMatrix)A).Clone();
                double[][] a = copy.Array;
                m = ((IMatrix)A).Rows;
                n = ((IMatrix)A).Cols;
                int nu = Math.Min(m, n);
                s = new double[Math.Min(m + 1, n)];
                U = new Matrix(m, nu);
                V = new Matrix(n, n);
                double[][] u = U.Array;
                double[][] v = V.Array;
                double[] e = new double[n];
                double[] work = new double[m];
                bool wantu = true;
                bool wantv = true;

                // Reduce A to bidiagonal form, storing the diagonal elements in s and the super-diagonal elements in e.
                int nct = Math.Min(m - 1, n);
                int nrt = Math.Max(0, Math.Min(n - 2, m));
                for (int k = 0; k < Math.Max(nct, nrt); k++)
                {
                    if (k < nct)
                    {
                        // Compute the transformation for the df-th column and place the df-th diagonal in s[df].
                        // Compute 2-norm of df-th column without under/overflow.
                        s[k] = 0;
                        for (int i = k; i < m; i++)
                            s[k] = MathHelper.Hypotenuse(s[k], a[i][k]);

                        if (s[k] != 0.0)
                        {
                            if (a[k][k] < 0.0)
                                s[k] = -s[k];

                            for (int i = k; i < m; i++)
                                a[i][k] /= s[k];

                            a[k][k] += 1.0;
                        }
                        s[k] = -s[k];
                    }

                    for (int j = k + 1; j < n; j++)
                    {
                        if ((k < nct) & (s[k] != 0.0))
                        {
                            // Apply the transformation.
                            double t = 0;
                            for (int i = k; i < m; i++)
                                t += a[i][k] * a[i][j];
                            t = -t / a[k][k];
                            for (int i = k; i < m; i++)
                                a[i][j] += t * a[i][k];
                        }

                        // Place the df-th row of A into e for the subsequent calculation of the row transformation.
                        e[j] = a[k][j];
                    }

                    if (wantu & (k < nct))
                    {
                        // Place the transformation in U for subsequent back
                        // multiplication.
                        for (int i = k; i < m; i++)
                            u[i][k] = a[i][k];
                    }

                    if (k < nrt)
                    {
                        // Compute the df-th row transformation and place the df-th super-diagonal in e[df].
                        // Compute 2-norm without under/overflow.
                        e[k] = 0;
                        for (int i = k + 1; i < n; i++)
                            e[k] = MathHelper.Hypotenuse(e[k], e[i]);

                        if (e[k] != 0.0)
                        {
                            if (e[k + 1] < 0.0)
                                e[k] = -e[k];

                            for (int i = k + 1; i < n; i++)
                                e[i] /= e[k];

                            e[k + 1] += 1.0;
                        }

                        e[k] = -e[k];
                        if ((k + 1 < m) & (e[k] != 0.0))
                        {
                            // Apply the transformation.
                            for (int i = k + 1; i < m; i++)
                                work[i] = 0.0;

                            for (int j = k + 1; j < n; j++)
                                for (int i = k + 1; i < m; i++)
                                    work[i] += e[j] * a[i][j];

                            for (int j = k + 1; j < n; j++)
                            {
                                double t = -e[j] / e[k + 1];
                                for (int i = k + 1; i < m; i++)
                                    a[i][j] += t * work[i];
                            }
                        }

                        if (wantv)
                        {
                            // Place the transformation in V for subsequent back multiplication.
                            for (int i = k + 1; i < n; i++)
                                v[i][k] = e[i];
                        }
                    }
                }

                // Set up the final bidiagonal matrix or order p.
                int p = Math.Min(n, m + 1);
                if (nct < n)
                    s[nct] = a[nct][nct];
                if (m < p)
                    s[p - 1] = 0.0;
                if (nrt + 1 < p)
                    e[nrt] = a[nrt][p - 1];
                e[p - 1] = 0.0;

                // If required, generate U.
                if (wantu)
                {
                    for (int j = nct; j < nu; j++)
                    {
                        for (int i = 0; i < m; i++)
                            u[i][j] = 0.0;
                        u[j][j] = 1.0;
                    }

                    for (int k = nct - 1; k >= 0; k--)
                    {
                        if (s[k] != 0.0)
                        {
                            for (int j = k + 1; j < nu; j++)
                            {
                                double t = 0;
                                for (int i = k; i < m; i++)
                                    t += u[i][k] * u[i][j];

                                t = -t / u[k][k];
                                for (int i = k; i < m; i++)
                                    u[i][j] += t * u[i][k];
                            }

                            for (int i = k; i < m; i++)
                                u[i][k] = -u[i][k];

                            u[k][k] = 1.0 + u[k][k];
                            for (int i = 0; i < k - 1; i++)
                                u[i][k] = 0.0;
                        }
                        else
                        {
                            for (int i = 0; i < m; i++)
                                u[i][k] = 0.0;
                            u[k][k] = 1.0;
                        }
                    }
                }

                // If required, generate V.
                if (wantv)
                {
                    for (int k = n - 1; k >= 0; k--)
                    {
                        if ((k < nrt) & (e[k] != 0.0))
                        {
                            for (int j = k + 1; j < nu; j++)
                            {
                                double t = 0;
                                for (int i = k + 1; i < n; i++)
                                    t += v[i][k] * v[i][j];

                                t = -t / v[k + 1][k];
                                for (int i = k + 1; i < n; i++)
                                    v[i][j] += t * v[i][k];
                            }
                        }

                        for (int i = 0; i < n; i++)
                            v[i][k] = 0.0;
                        v[k][k] = 1.0;
                    }
                }

                // Main iteration loop for the singular values.
                int pp = p - 1;
                int iter = 0;
                double eps = Math.Pow(2.0, -52.0);
                while (p > 0)
                {
                    int k, kase;

                    // Here is where a test for too many iterations would go.
                    // This section of the program inspects for
                    // negligible elements in the s and e arrays.  On
                    // completion the variables kase and df are set as follows.
                    // kase = 1     if s(p) and e[df-1] are negligible and df<p
                    // kase = 2     if s(df) is negligible and df<p
                    // kase = 3     if e[df-1] is negligible, df<p, and s(df), ..., s(p) are not negligible (qr step).
                    // kase = 4     if e(p-1) is negligible (convergence).
                    for (k = p - 2; k >= -1; k--)
                    {
                        if (k == -1)
                            break;

                        if (Math.Abs(e[k]) <= eps * (Math.Abs(s[k]) + Math.Abs(s[k + 1])))
                        {
                            e[k] = 0.0;
                            break;
                        }
                    }

                    if (k == p - 2)
                    {
                        kase = 4;
                    }
                    else
                    {
                        int ks;
                        for (ks = p - 1; ks >= k; ks--)
                        {
                            if (ks == k)
                                break;

                            double t = (ks != p ? Math.Abs(e[ks]) : 0.0) + (ks != k + 1 ? Math.Abs(e[ks - 1]) : 0.0);
                            if (Math.Abs(s[ks]) <= eps * t)
                            {
                                s[ks] = 0.0;
                                break;
                            }
                        }

                        if (ks == k)
                            kase = 3;
                        else if (ks == p - 1)
                            kase = 1;
                        else
                        {
                            kase = 2;
                            k = ks;
                        }
                    }

                    k++;

                    // Perform the task indicated by kase.
                    switch (kase)
                    {
                        // Deflate negligible s(p).
                        case 1:
                            {
                                double f = e[p - 2];
                                e[p - 2] = 0.0;
                                for (int j = p - 2; j >= k; j--)
                                {
                                    double t = MathHelper.Hypotenuse(s[j], f);
                                    double cs = s[j] / t;
                                    double sn = f / t;
                                    s[j] = t;
                                    if (j != k)
                                    {
                                        f = -sn * e[j - 1];
                                        e[j - 1] = cs * e[j - 1];
                                    }

                                    if (wantv)
                                    {
                                        for (int i = 0; i < n; i++)
                                        {
                                            t = cs * v[i][j] + sn * v[i][p - 1];
                                            v[i][p - 1] = -sn * v[i][j] + cs * v[i][p - 1];
                                            v[i][j] = t;
                                        }
                                    }
                                }
                            }
                            break;

                        // Split at negligible s(df).
                        case 2:
                            {
                                double f = e[k - 1];
                                e[k - 1] = 0.0;
                                for (int j = k; j < p; j++)
                                {
                                    double t = MathHelper.Hypotenuse(s[j], f);
                                    double cs = s[j] / t;
                                    double sn = f / t;
                                    s[j] = t;
                                    f = -sn * e[j];
                                    e[j] = cs * e[j];
                                    if (wantu)
                                    {
                                        for (int i = 0; i < m; i++)
                                        {
                                            t = cs * u[i][j] + sn * u[i][k - 1];
                                            u[i][k - 1] = -sn * u[i][j] + cs * u[i][k - 1];
                                            u[i][j] = t;
                                        }
                                    }
                                }
                            }
                            break;

                        // Perform one qr step.
                        case 3:
                            {
                                // Calculate the shift.
                                double scale = Math.Max(Math.Max(Math.Max(Math.Max(Math.Abs(s[p - 1]), Math.Abs(s[p - 2])), Math.Abs(e[p - 2])), Math.Abs(s[k])), Math.Abs(e[k]));
                                double sp = s[p - 1] / scale;
                                double spm1 = s[p - 2] / scale;
                                double epm1 = e[p - 2] / scale;
                                double sk = s[k] / scale;
                                double ek = e[k] / scale;
                                double b = ((spm1 + sp) * (spm1 - sp) + epm1 * epm1) / 2.0;
                                double c = (sp * epm1) * (sp * epm1);
                                double shift = 0.0;
                                if ((b != 0.0) | (c != 0.0))
                                {
                                    shift = Math.Sqrt(b * b + c);
                                    if (b < 0.0)
                                        shift = -shift;
                                    shift = c / (b + shift);
                                }

                                double f = (sk + sp) * (sk - sp) + shift;
                                double g = sk * ek;

                                // Chase zeros.
                                for (int j = k; j < p - 1; j++)
                                {
                                    double t = MathHelper.Hypotenuse(f, g);
                                    double cs = f / t;
                                    double sn = g / t;
                                    if (j != k)
                                        e[j - 1] = t;
                                    f = cs * s[j] + sn * e[j];
                                    e[j] = cs * e[j] - sn * s[j];
                                    g = sn * s[j + 1];
                                    s[j + 1] = cs * s[j + 1];
                                    if (wantv)
                                    {
                                        for (int i = 0; i < n; i++)
                                        {
                                            t = cs * v[i][j] + sn * v[i][j + 1];
                                            v[i][j + 1] = -sn * v[i][j] + cs * v[i][j + 1];
                                            v[i][j] = t;
                                        }
                                    }

                                    t = MathHelper.Hypotenuse(f, g);
                                    cs = f / t;
                                    sn = g / t;
                                    s[j] = t;
                                    f = cs * e[j] + sn * s[j + 1];
                                    s[j + 1] = -sn * e[j] + cs * s[j + 1];
                                    g = sn * e[j + 1];
                                    e[j + 1] = cs * e[j + 1];
                                    if (wantu && (j < m - 1))
                                    {
                                        for (int i = 0; i < m; i++)
                                        {
                                            t = cs * u[i][j] + sn * u[i][j + 1];
                                            u[i][j + 1] = -sn * u[i][j] + cs * u[i][j + 1];
                                            u[i][j] = t;
                                        }
                                    }
                                }

                                e[p - 2] = f;
                                iter = iter + 1;
                            }
                            break;

                        // Convergence.
                        case 4:
                            {
                                // Make the singular values positive.
                                if (s[k] <= 0.0)
                                {
                                    s[k] = (s[k] < 0.0 ? -s[k] : 0.0);
                                    if (wantv)
                                        for (int i = 0; i <= pp; i++)
                                            v[i][k] = -v[i][k];
                                }

                                // Order the singular values.
                                while (k < pp)
                                {
                                    if (s[k] >= s[k + 1])
                                        break;

                                    double t = s[k];
                                    s[k] = s[k + 1];
                                    s[k + 1] = t;
                                    if (wantv && (k < n - 1))
                                        for (int i = 0; i < n; i++)
                                        {
                                            t = v[i][k + 1];
                                            v[i][k + 1] = v[i][k];
                                            v[i][k] = t;
                                        }

                                    if (wantu && (k < m - 1))
                                        for (int i = 0; i < m; i++)
                                        {
                                            t = u[i][k + 1];
                                            u[i][k + 1] = u[i][k];
                                            u[i][k] = t;
                                        }

                                    k++;
                                }

                                iter = 0;
                                p--;
                            }
                            break;
                    }
                }
            }

            double ISingularValueDecomposition.Condition
            {
                get { return s[0] / s[Math.Min(m, n) - 1]; }
            }

            double ISingularValueDecomposition.Norm2
            {
                get { return s[0]; }
            }

            int ISingularValueDecomposition.Rank
            {
                get
                {
                    double eps = Math.Pow(2.0, -52.0);
                    double tol = Math.Max(m, n) * s[0] * eps;
                    int r = 0;
                    for (int i = 0; i < s.Length; i++)
                        if (s[i] > tol)
                            r++;
                    return r;
                }
            }

            double[] ISingularValueDecomposition.Diagonal
            {
                get { return s; }
            }
        }

        #endregion SingularValueDecomposition

        #region EigenvalueDecomposition

        private class EigenvalueDecomposition : IEigenvalueDecomposition
        {
            private int n;           	// matrix dimension
            private double[] d, e; 		// storage of eigenvalues.
            private Matrix V; 			// storage of eigenvectors.
            private Matrix H;  			// storage of nonsymmetric Hessenberg form.
            private double[] ort;    	// storage for nonsymmetric algorithm.
            private double cdivr, cdivi;
            private bool isSymmetric;

            internal EigenvalueDecomposition(Matrix A)
            {
                if (((IMatrix)A).Rows != ((IMatrix)A).Cols)
                    throw new ArgumentException("Matrix is not a square matrix");

                n = ((IMatrix)A).Cols;
                V = new Matrix(n, n);
                d = new double[n];
                e = new double[n];

                // Check for symmetry.
                isSymmetric = ((IMatrix)A).IsSymmetric;

                if (isSymmetric)
                {
                    for (int i = 0; i < n; i++)
                        for (int j = 0; j < n; j++)
                            ((IMatrix)V)[i, j] = ((IMatrix)A)[i, j];

                    // Tridiagonalize.
                    tred2();
                    // Diagonalize.
                    tql2();
                }
                else
                {
                    H = new Matrix(n, n);
                    ort = new double[n];

                    for (int j = 0; j < n; j++)
                        for (int i = 0; i < n; i++)
                            ((IMatrix)H)[i, j] = ((IMatrix)A)[i, j];

                    // Reduce to Hessenberg form.
                    orthes();

                    // Reduce Hessenberg to real Schur form.
                    hqr2();
                }
            }

            void tred2()
            {
                // Symmetric Householder reduction to tridiagonal form.
                // This is derived from the Algol procedures tred2 by Bowdler, Martin, Reinsch, and Wilkinson, 
                // Handbook for Auto. Comp., Vol.ii-Linear Algebra, and the corresponding Fortran subroutine in EISPACK.
                for (int j = 0; j < n; j++)
                    d[j] = ((IMatrix)V)[n - 1, j];

                // Householder reduction to tridiagonal form.
                for (int i = n - 1; i > 0; i--)
                {
                    // Scale to avoid under/overflow.
                    double scale = 0.0;
                    double h = 0.0;
                    for (int k = 0; k < i; k++)
                        scale = scale + Math.Abs(d[k]);

                    if (scale == 0.0)
                    {
                        e[i] = d[i - 1];
                        for (int j = 0; j < i; j++)
                        {
                            d[j] = ((IMatrix)V)[i - 1, j];
                            ((IMatrix)V)[i, j] = 0.0;
                            ((IMatrix)V)[j, i] = 0.0;
                        }
                    }
                    else
                    {
                        // Generate Householder vector.
                        for (int k = 0; k < i; k++)
                        {
                            d[k] /= scale;
                            h += d[k] * d[k];
                        }

                        double f = d[i - 1];
                        double g = Math.Sqrt(h);
                        if (f > 0)
                            g = -g;

                        e[i] = scale * g;
                        h = h - f * g;
                        d[i - 1] = f - g;
                        for (int j = 0; j < i; j++)
                            e[j] = 0.0;

                        // Apply similarity transformation to remaining columns.
                        for (int j = 0; j < i; j++)
                        {
                            f = d[j];
                            ((IMatrix)V)[j, i] = f;
                            g = e[j] + ((IMatrix)V)[j, j] * f;
                            for (int k = j + 1; k <= i - 1; k++)
                            {
                                g += ((IMatrix)V)[k, j] * d[k];
                                e[k] += ((IMatrix)V)[k, j] * f;
                            }
                            e[j] = g;
                        }

                        f = 0.0;
                        for (int j = 0; j < i; j++)
                        {
                            e[j] /= h;
                            f += e[j] * d[j];
                        }

                        double hh = f / (h + h);
                        for (int j = 0; j < i; j++)
                            e[j] -= hh * d[j];

                        for (int j = 0; j < i; j++)
                        {
                            f = d[j];
                            g = e[j];
                            for (int k = j; k <= i - 1; k++)
                                ((IMatrix)V)[k, j] -= (f * e[k] + g * d[k]);

                            d[j] = ((IMatrix)V)[i - 1, j];
                            ((IMatrix)V)[i, j] = 0.0;
                        }
                    }
                    d[i] = h;
                }

                // Accumulate transformations.
                for (int i = 0; i < n - 1; i++)
                {
                    ((IMatrix)V)[n - 1, i] = ((IMatrix)V)[i, i];
                    ((IMatrix)V)[i, i] = 1.0;
                    double h = d[i + 1];
                    if (h != 0.0)
                    {
                        for (int k = 0; k <= i; k++)
                            d[k] = ((IMatrix)V)[k, i + 1] / h;

                        for (int j = 0; j <= i; j++)
                        {
                            double g = 0.0;
                            for (int k = 0; k <= i; k++)
                                g += ((IMatrix)V)[k, i + 1] * ((IMatrix)V)[k, j];
                            for (int k = 0; k <= i; k++)
                                ((IMatrix)V)[k, j] -= g * d[k];
                        }
                    }

                    for (int k = 0; k <= i; k++)
                        ((IMatrix)V)[k, i + 1] = 0.0;
                }

                for (int j = 0; j < n; j++)
                {
                    d[j] = ((IMatrix)V)[n - 1, j];
                    ((IMatrix)V)[n - 1, j] = 0.0;
                }

                ((IMatrix)V)[n - 1, n - 1] = 1.0;
                e[0] = 0.0;
            }

            void tql2()
            {
                // Symmetric tridiagonal QL algorithm.
                // This is derived from the Algol procedures tql2, by Bowdler, Martin, Reinsch, and Wilkinson, 
                // Handbook for Auto. Comp., Vol.ii-Linear Algebra, and the corresponding Fortran subroutine in EISPACK.
                for (int i = 1; i < n; i++)
                    e[i - 1] = e[i];

                e[n - 1] = 0.0;

                double f = 0.0;
                double tst1 = 0.0;
                double eps = Math.Pow(2.0, -52.0);

                for (int l = 0; l < n; l++)
                {
                    // Find small subdiagonal element.
                    tst1 = Math.Max(tst1, Math.Abs(d[l]) + Math.Abs(e[l]));
                    int m = l;
                    while (m < n)
                    {
                        if (Math.Abs(e[m]) <= eps * tst1)
                            break;
                        m++;
                    }

                    // If m == l, d[l] is an eigenvalue, otherwise, iterate.
                    if (m > l)
                    {
                        int iter = 0;
                        do
                        {
                            iter = iter + 1;  // (Could check iteration count here.)

                            // Compute implicit shift
                            double g = d[l];
                            double p = (d[l + 1] - g) / (2.0 * e[l]);
                            double r = MathHelper.Hypotenuse(p, 1.0);
                            if (p < 0)
                                r = -r;

                            d[l] = e[l] / (p + r);
                            d[l + 1] = e[l] * (p + r);
                            double dl1 = d[l + 1];
                            double h = g - d[l];
                            for (int i = l + 2; i < n; i++)
                                d[i] -= h;
                            f = f + h;

                            // Implicit QL transformation.
                            p = d[m];
                            double c = 1.0;
                            double c2 = c;
                            double c3 = c;
                            double el1 = e[l + 1];
                            double s = 0.0;
                            double s2 = 0.0;
                            for (int i = m - 1; i >= l; i--)
                            {
                                c3 = c2;
                                c2 = c;
                                s2 = s;
                                g = c * e[i];
                                h = c * p;
                                r = MathHelper.Hypotenuse(p, e[i]);
                                e[i + 1] = s * r;
                                s = e[i] / r;
                                c = p / r;
                                p = c * d[i] - s * g;
                                d[i + 1] = h + s * (c * g + s * d[i]);

                                // Accumulate transformation.
                                for (int k = 0; k < n; k++)
                                {
                                    h = ((IMatrix)V)[k, i + 1];
                                    ((IMatrix)V)[k, i + 1] = s * ((IMatrix)V)[k, i] + c * h;
                                    ((IMatrix)V)[k, i] = c * ((IMatrix)V)[k, i] - s * h;
                                }
                            }

                            p = -s * s2 * c3 * el1 * e[l] / dl1;
                            e[l] = s * p;
                            d[l] = c * p;

                            // Check for convergence.
                        }
                        while (Math.Abs(e[l]) > eps * tst1);
                    }
                    d[l] = d[l] + f;
                    e[l] = 0.0;
                }

                // Sort eigenvalues and corresponding vectors.
                for (int i = 0; i < n - 1; i++)
                {
                    int k = i;
                    double p = d[i];
                    for (int j = i + 1; j < n; j++)
                    {
                        if (d[j] < p)
                        {
                            k = j;
                            p = d[j];
                        }
                    }

                    if (k != i)
                    {
                        d[k] = d[i];
                        d[i] = p;
                        for (int j = 0; j < n; j++)
                        {
                            p = ((IMatrix)V)[j, i];
                            ((IMatrix)V)[j, i] = ((IMatrix)V)[j, k];
                            ((IMatrix)V)[j, k] = p;
                        }
                    }
                }
            }

            void orthes()
            {
                // Nonsymmetric reduction to Hessenberg form.
                // This is derived from the Algol procedures orthes and ortran, by Martin and Wilkinson, 
                // Handbook for Auto. Comp., Vol.ii-Linear Algebra, and the corresponding Fortran subroutines in EISPACK.
                int low = 0;
                int high = n - 1;

                for (int m = low + 1; m <= high - 1; m++)
                {
                    // Scale column.

                    double scale = 0.0;
                    for (int i = m; i <= high; i++)
                        scale = scale + Math.Abs(((IMatrix)H)[i, m - 1]);

                    if (scale != 0.0)
                    {
                        // Compute Householder transformation.
                        double h = 0.0;
                        for (int i = high; i >= m; i--)
                        {
                            ort[i] = ((IMatrix)H)[i, m - 1] / scale;
                            h += ort[i] * ort[i];
                        }

                        double g = Math.Sqrt(h);
                        if (ort[m] > 0)
                            g = -g;

                        h = h - ort[m] * g;
                        ort[m] = ort[m] - g;

                        // Apply Householder similarity transformation
                        // H = (I - u * u' / h) * H * (I - u * u') / h)
                        for (int j = m; j < n; j++)
                        {
                            double f = 0.0;
                            for (int i = high; i >= m; i--)
                                f += ort[i] * ((IMatrix)H)[i, j];

                            f = f / h;
                            for (int i = m; i <= high; i++)
                                ((IMatrix)H)[i, j] -= f * ort[i];
                        }

                        for (int i = 0; i <= high; i++)
                        {
                            double f = 0.0;
                            for (int j = high; j >= m; j--)
                                f += ort[j] * ((IMatrix)H)[i, j];

                            f = f / h;
                            for (int j = m; j <= high; j++)
                                ((IMatrix)H)[i, j] -= f * ort[j];
                        }

                        ort[m] = scale * ort[m];
                        ((IMatrix)H)[m, m - 1] = scale * g;
                    }
                }

                // Accumulate transformations (Algol's ortran).
                for (int i = 0; i < n; i++)
                    for (int j = 0; j < n; j++)
                        ((IMatrix)V)[i, j] = (i == j ? 1.0 : 0.0);

                for (int m = high - 1; m >= low + 1; m--)
                {
                    if (((IMatrix)H)[m, m - 1] != 0.0)
                    {
                        for (int i = m + 1; i <= high; i++)
                            ort[i] = ((IMatrix)H)[i, m - 1];

                        for (int j = m; j <= high; j++)
                        {
                            double g = 0.0;
                            for (int i = m; i <= high; i++)
                                g += ort[i] * ((IMatrix)V)[i, j];

                            // Double division avoids possible underflow.
                            g = (g / ort[m]) / ((IMatrix)H)[m, m - 1];
                            for (int i = m; i <= high; i++)
                                ((IMatrix)V)[i, j] += g * ort[i];
                        }
                    }
                }
            }

            void cdiv(double xr, double xi, double yr, double yi)
            {
                // Complex scalar division.
                double r, d;
                if (Math.Abs(yr) > Math.Abs(yi))
                {
                    r = yi / yr;
                    d = yr + r * yi;
                    cdivr = (xr + r * xi) / d;
                    cdivi = (xi - r * xr) / d;
                }
                else
                {
                    r = yr / yi;
                    d = yi + r * yr;
                    cdivr = (r * xr + xi) / d;
                    cdivi = (r * xi - xr) / d;
                }
            }

            void hqr2()
            {
                // Nonsymmetric reduction from Hessenberg to real Schur form.   
                // This is derived from the Algol procedure hqr2, by Martin and Wilkinson, Handbook for Auto. Comp.,
                // Vol.ii-Linear Algebra, and the corresponding  Fortran subroutine in EISPACK.
                int nn = this.n;
                int n = nn - 1;
                int low = 0;
                int high = nn - 1;
                double eps = Math.Pow(2.0, -52.0);
                double exshift = 0.0;
                double p = 0, q = 0, r = 0, s = 0, z = 0, t, w, x, y;

                // Store roots isolated by balanc and compute matrix norm
                double norm = 0.0;
                for (int i = 0; i < nn; i++)
                {
                    if (i < low | i > high)
                    {
                        d[i] = ((IMatrix)H)[i, i];
                        e[i] = 0.0;
                    }

                    for (int j = Math.Max(i - 1, 0); j < nn; j++)
                        norm = norm + Math.Abs(((IMatrix)H)[i, j]);
                }

                // Outer loop over eigenvalue index
                int iter = 0;
                while (n >= low)
                {
                    // Look for single small sub-diagonal element
                    int l = n;
                    while (l > low)
                    {
                        s = Math.Abs(((IMatrix)H)[l - 1, l - 1]) + Math.Abs(((IMatrix)H)[l, l]);
                        if (s == 0.0)
                            s = norm;
                        if (Math.Abs(((IMatrix)H)[l, l - 1]) < eps * s)
                            break;

                        l--;
                    }

                    // Check for convergence
                    if (l == n)
                    {
                        // One root found
                        ((IMatrix)H)[n, n] = ((IMatrix)H)[n, n] + exshift;
                        d[n] = ((IMatrix)H)[n, n];
                        e[n] = 0.0;
                        n--;
                        iter = 0;
                    }
                    else if (l == n - 1)
                    {
                        // Two roots found
                        w = ((IMatrix)H)[n, n - 1] * ((IMatrix)H)[n - 1, n];
                        p = (((IMatrix)H)[n - 1, n - 1] - ((IMatrix)H)[n, n]) / 2.0;
                        q = p * p + w;
                        z = Math.Sqrt(Math.Abs(q));
                        ((IMatrix)H)[n, n] = ((IMatrix)H)[n, n] + exshift;
                        ((IMatrix)H)[n - 1, n - 1] = ((IMatrix)H)[n - 1, n - 1] + exshift;
                        x = ((IMatrix)H)[n, n];

                        if (q >= 0)
                        {
                            // Real pair
                            z = (p >= 0) ? (p + z) : (p - z);
                            d[n - 1] = x + z;
                            d[n] = d[n - 1];
                            if (z != 0.0)
                                d[n] = x - w / z;
                            e[n - 1] = 0.0;
                            e[n] = 0.0;
                            x = ((IMatrix)H)[n, n - 1];
                            s = Math.Abs(x) + Math.Abs(z);
                            p = x / s;
                            q = z / s;
                            r = Math.Sqrt(p * p + q * q);
                            p = p / r;
                            q = q / r;

                            // Row modification
                            for (int j = n - 1; j < nn; j++)
                            {
                                z = ((IMatrix)H)[n - 1, j];
                                ((IMatrix)H)[n - 1, j] = q * z + p * ((IMatrix)H)[n, j];
                                ((IMatrix)H)[n, j] = q * ((IMatrix)H)[n, j] - p * z;
                            }

                            // Column modification
                            for (int i = 0; i <= n; i++)
                            {
                                z = ((IMatrix)H)[i, n - 1];
                                ((IMatrix)H)[i, n - 1] = q * z + p * ((IMatrix)H)[i, n];
                                ((IMatrix)H)[i, n] = q * ((IMatrix)H)[i, n] - p * z;
                            }

                            // Accumulate transformations
                            for (int i = low; i <= high; i++)
                            {
                                z = ((IMatrix)V)[i, n - 1];
                                ((IMatrix)V)[i, n - 1] = q * z + p * ((IMatrix)V)[i, n];
                                ((IMatrix)V)[i, n] = q * ((IMatrix)V)[i, n] - p * z;
                            }
                        }
                        else
                        {
                            // Complex pair
                            d[n - 1] = x + p;
                            d[n] = x + p;
                            e[n - 1] = z;
                            e[n] = -z;
                        }

                        n = n - 2;
                        iter = 0;
                    }
                    else
                    {
                        // No convergence yet	 

                        // Form shift
                        x = ((IMatrix)H)[n, n];
                        y = 0.0;
                        w = 0.0;
                        if (l < n)
                        {
                            y = ((IMatrix)H)[n - 1, n - 1];
                            w = ((IMatrix)H)[n, n - 1] * ((IMatrix)H)[n - 1, n];
                        }

                        // Wilkinson's original ad hoc shift
                        if (iter == 10)
                        {
                            exshift += x;
                            for (int i = low; i <= n; i++)
                                ((IMatrix)H)[i, i] -= x;

                            s = Math.Abs(((IMatrix)H)[n, n - 1]) + Math.Abs(((IMatrix)H)[n - 1, n - 2]);
                            x = y = 0.75 * s;
                            w = -0.4375 * s * s;
                        }

                        // MATLAB's new ad hoc shift
                        if (iter == 30)
                        {
                            s = (y - x) / 2.0;
                            s = s * s + w;
                            if (s > 0)
                            {
                                s = Math.Sqrt(s);
                                if (y < x)
                                    s = -s;
                                s = x - w / ((y - x) / 2.0 + s);
                                for (int i = low; i <= n; i++)
                                    ((IMatrix)H)[i, i] -= s;
                                exshift += s;
                                x = y = w = 0.964;
                            }
                        }

                        iter = iter + 1;

                        // Look for two consecutive small sub-diagonal elements
                        int m = n - 2;
                        while (m >= l)
                        {
                            z = ((IMatrix)H)[m, m];
                            r = x - z;
                            s = y - z;
                            p = (r * s - w) / ((IMatrix)H)[m + 1, m] + ((IMatrix)H)[m, m + 1];
                            q = ((IMatrix)H)[m + 1, m + 1] - z - r - s;
                            r = ((IMatrix)H)[m + 2, m + 1];
                            s = Math.Abs(p) + Math.Abs(q) + Math.Abs(r);
                            p = p / s;
                            q = q / s;
                            r = r / s;
                            if (m == l)
                                break;
                            if (Math.Abs(((IMatrix)H)[m, m - 1]) * (Math.Abs(q) + Math.Abs(r)) < eps * (Math.Abs(p) * (Math.Abs(((IMatrix)H)[m - 1, m - 1]) + Math.Abs(z) + Math.Abs(((IMatrix)H)[m + 1, m + 1]))))
                                break;
                            m--;
                        }

                        for (int i = m + 2; i <= n; i++)
                        {
                            ((IMatrix)H)[i, i - 2] = 0.0;
                            if (i > m + 2)
                                ((IMatrix)H)[i, i - 3] = 0.0;
                        }

                        // Double QR step involving rows l:n and columns m:n
                        for (int k = m; k <= n - 1; k++)
                        {
                            bool notlast = (k != n - 1);
                            if (k != m)
                            {
                                p = ((IMatrix)H)[k, k - 1];
                                q = ((IMatrix)H)[k + 1, k - 1];
                                r = (notlast ? ((IMatrix)H)[k + 2, k - 1] : 0.0);
                                x = Math.Abs(p) + Math.Abs(q) + Math.Abs(r);
                                if (x != 0.0)
                                {
                                    p = p / x;
                                    q = q / x;
                                    r = r / x;
                                }
                            }

                            if (x == 0.0)
                                break;

                            s = Math.Sqrt(p * p + q * q + r * r);
                            if (p < 0)
                                s = -s;

                            if (s != 0)
                            {
                                if (k != m)
                                    ((IMatrix)H)[k, k - 1] = -s * x;
                                else
                                    if (l != m)
                                        ((IMatrix)H)[k, k - 1] = -((IMatrix)H)[k, k - 1];

                                p = p + s;
                                x = p / s;
                                y = q / s;
                                z = r / s;
                                q = q / p;
                                r = r / p;

                                // Row modification
                                for (int j = k; j < nn; j++)
                                {
                                    p = ((IMatrix)H)[k, j] + q * ((IMatrix)H)[k + 1, j];
                                    if (notlast)
                                    {
                                        p = p + r * ((IMatrix)H)[k + 2, j];
                                        ((IMatrix)H)[k + 2, j] = ((IMatrix)H)[k + 2, j] - p * z;
                                    }

                                    ((IMatrix)H)[k, j] = ((IMatrix)H)[k, j] - p * x;
                                    ((IMatrix)H)[k + 1, j] = ((IMatrix)H)[k + 1, j] - p * y;
                                }

                                // Column modification
                                for (int i = 0; i <= Math.Min(n, k + 3); i++)
                                {
                                    p = x * ((IMatrix)H)[i, k] + y * ((IMatrix)H)[i, k + 1];
                                    if (notlast)
                                    {
                                        p = p + z * ((IMatrix)H)[i, k + 2];
                                        ((IMatrix)H)[i, k + 2] = ((IMatrix)H)[i, k + 2] - p * r;
                                    }

                                    ((IMatrix)H)[i, k] = ((IMatrix)H)[i, k] - p;
                                    ((IMatrix)H)[i, k + 1] = ((IMatrix)H)[i, k + 1] - p * q;
                                }

                                // Accumulate transformations
                                for (int i = low; i <= high; i++)
                                {
                                    p = x * ((IMatrix)V)[i, k] + y * ((IMatrix)V)[i, k + 1];
                                    if (notlast)
                                    {
                                        p = p + z * ((IMatrix)V)[i, k + 2];
                                        ((IMatrix)V)[i, k + 2] = ((IMatrix)V)[i, k + 2] - p * r;
                                    }

                                    ((IMatrix)V)[i, k] = ((IMatrix)V)[i, k] - p;
                                    ((IMatrix)V)[i, k + 1] = ((IMatrix)V)[i, k + 1] - p * q;
                                }
                            }
                        }
                    }
                }

                // Backsubstitute to find vectors of upper triangular form
                if (norm == 0.0)
                    return;

                for (n = nn - 1; n >= 0; n--)
                {
                    p = d[n];
                    q = e[n];

                    // Real vector
                    if (q == 0)
                    {
                        int l = n;
                        ((IMatrix)H)[n, n] = 1.0;
                        for (int i = n - 1; i >= 0; i--)
                        {
                            w = ((IMatrix)H)[i, i] - p;
                            r = 0.0;
                            for (int j = l; j <= n; j++)
                                r = r + ((IMatrix)H)[i, j] * ((IMatrix)H)[j, n];

                            if (e[i] < 0.0)
                            {
                                z = w;
                                s = r;
                            }
                            else
                            {
                                l = i;
                                if (e[i] == 0.0)
                                {
                                    ((IMatrix)H)[i, n] = (w != 0.0) ? (-r / w) : (-r / (eps * norm));
                                }
                                else
                                {
                                    // Solve real equations
                                    x = ((IMatrix)H)[i, i + 1];
                                    y = ((IMatrix)H)[i + 1, i];
                                    q = (d[i] - p) * (d[i] - p) + e[i] * e[i];
                                    t = (x * s - z * r) / q;
                                    ((IMatrix)H)[i, n] = t;
                                    ((IMatrix)H)[i + 1, n] = (Math.Abs(x) > Math.Abs(z)) ? ((-r - w * t) / x) : ((-s - y * t) / z);
                                }

                                // Overflow control
                                t = Math.Abs(((IMatrix)H)[i, n]);
                                if ((eps * t) * t > 1)
                                    for (int j = i; j <= n; j++)
                                        ((IMatrix)H)[j, n] = ((IMatrix)H)[j, n] / t;
                            }
                        }
                    }
                    else if (q < 0)
                    {
                        // Complex vector
                        int l = n - 1;

                        // Last vector component imaginary so matrix is triangular
                        if (Math.Abs(((IMatrix)H)[n, n - 1]) > Math.Abs(((IMatrix)H)[n - 1, n]))
                        {
                            ((IMatrix)H)[n - 1, n - 1] = q / ((IMatrix)H)[n, n - 1];
                            ((IMatrix)H)[n - 1, n] = -(((IMatrix)H)[n, n] - p) / ((IMatrix)H)[n, n - 1];
                        }
                        else
                        {
                            cdiv(0.0, -((IMatrix)H)[n - 1, n], ((IMatrix)H)[n - 1, n - 1] - p, q);
                            ((IMatrix)H)[n - 1, n - 1] = cdivr;
                            ((IMatrix)H)[n - 1, n] = cdivi;
                        }

                        ((IMatrix)H)[n, n - 1] = 0.0;
                        ((IMatrix)H)[n, n] = 1.0;
                        for (int i = n - 2; i >= 0; i--)
                        {
                            double ra, sa, vr, vi;
                            ra = 0.0;
                            sa = 0.0;
                            for (int j = l; j <= n; j++)
                            {
                                ra = ra + ((IMatrix)H)[i, j] * ((IMatrix)H)[j, n - 1];
                                sa = sa + ((IMatrix)H)[i, j] * ((IMatrix)H)[j, n];
                            }

                            w = ((IMatrix)H)[i, i] - p;

                            if (e[i] < 0.0)
                            {
                                z = w;
                                r = ra;
                                s = sa;
                            }
                            else
                            {
                                l = i;
                                if (e[i] == 0)
                                {
                                    cdiv(-ra, -sa, w, q);
                                    ((IMatrix)H)[i, n - 1] = cdivr;
                                    ((IMatrix)H)[i, n] = cdivi;
                                }
                                else
                                {
                                    // Solve complex equations
                                    x = ((IMatrix)H)[i, i + 1];
                                    y = ((IMatrix)H)[i + 1, i];
                                    vr = (d[i] - p) * (d[i] - p) + e[i] * e[i] - q * q;
                                    vi = (d[i] - p) * 2.0 * q;
                                    if (vr == 0.0 & vi == 0.0)
                                        vr = eps * norm * (Math.Abs(w) + Math.Abs(q) + Math.Abs(x) + Math.Abs(y) + Math.Abs(z));
                                    cdiv(x * r - z * ra + q * sa, x * s - z * sa - q * ra, vr, vi);
                                    ((IMatrix)H)[i, n - 1] = cdivr;
                                    ((IMatrix)H)[i, n] = cdivi;
                                    if (Math.Abs(x) > (Math.Abs(z) + Math.Abs(q)))
                                    {
                                        ((IMatrix)H)[i + 1, n - 1] = (-ra - w * ((IMatrix)H)[i, n - 1] + q * ((IMatrix)H)[i, n]) / x;
                                        ((IMatrix)H)[i + 1, n] = (-sa - w * ((IMatrix)H)[i, n] - q * ((IMatrix)H)[i, n - 1]) / x;
                                    }
                                    else
                                    {
                                        cdiv(-r - y * ((IMatrix)H)[i, n - 1], -s - y * ((IMatrix)H)[i, n], z, q);
                                        ((IMatrix)H)[i + 1, n - 1] = cdivr;
                                        ((IMatrix)H)[i + 1, n] = cdivi;
                                    }
                                }

                                // Overflow control
                                t = Math.Max(Math.Abs(((IMatrix)H)[i, n - 1]), Math.Abs(((IMatrix)H)[i, n]));
                                if ((eps * t) * t > 1)
                                    for (int j = i; j <= n; j++)
                                    {
                                        ((IMatrix)H)[j, n - 1] = ((IMatrix)H)[j, n - 1] / t;
                                        ((IMatrix)H)[j, n] = ((IMatrix)H)[j, n] / t;
                                    }
                            }
                        }
                    }
                }

                // Vectors of isolated roots
                for (int i = 0; i < nn; i++)
                    if (i < low | i > high)
                        for (int j = i; j < nn; j++)
                            ((IMatrix)V)[i, j] = ((IMatrix)H)[i, j];

                // Back transformation to get eigenvectors of original matrix
                for (int j = nn - 1; j >= low; j--)
                    for (int i = low; i <= high; i++)
                    {
                        z = 0.0;
                        for (int k = low; k <= Math.Min(j, high); k++)
                            z = z + ((IMatrix)V)[i, k] * ((IMatrix)H)[k, j];
                        ((IMatrix)V)[i, j] = z;
                    }
            }

            double[] IEigenvalueDecomposition.RealEigenvalues
            {
                get { return d; }
            }

            double[] IEigenvalueDecomposition.ImaginaryEigenvalues
            {
                get { return e; }
            }

            IMatrix IEigenvalueDecomposition.EigenvectorMatrix
            {
                get { return V; }
            }

            IMatrix IEigenvalueDecomposition.DiagonalMatrix
            {
                get
                {
                    Matrix X = new Matrix(n, n);
                    double[][] x = X.Array;

                    for (int i = 0; i < n; i++)
                    {
                        for (int j = 0; j < n; j++)
                            x[i][j] = 0.0;

                        x[i][i] = d[i];
                        if (e[i] > 0)
                        {
                            x[i][i + 1] = e[i];
                        }
                        else if (e[i] < 0)
                        {
                            x[i][i - 1] = e[i];
                        }
                    }

                    return X;
                }
            }
        }

        #endregion EigenvalueDecomposition

        #region MathHelper

        private class MathHelper
        {
            private static Random random = new Random();
        
            internal static double Random()
            {
                return random.NextDouble();
            }

            internal static double Hypotenuse(double a, double b)
            {
                if (Math.Abs(a) > Math.Abs(b))
                {
                    double r = b / a;
                    return Math.Abs(a) * Math.Sqrt(1 + r * r);
                }

                if (b != 0)
                {
                    double r = a / b;
                    return Math.Abs(b) * Math.Sqrt(1 + r * r);
                }

                return 0.0;
            }
        }
        #endregion MathHelper

        #endregion

    }

    #region Interfaces

    #region Interface ICholeskyDecomposition

    /// <summary>
    ///		Cholesky Decomposition of a symmetric, positive definite matrix.
    ///	</summary>
    /// <remarks>
    ///		For a symmetric, positive definite matrix <c>A</c>, the Cholesky decomposition is a
    ///		lower triangular matrix <c>L</c> so that <c>A = L * L'</c>.
    ///		If the matrix is not symmetric or positive definite, the constructor returns a partial 
    ///		decomposition and sets two internal variables that can be queried using the
    ///		<see cref="IsSymmetric"/> and <see cref="IsPositiveDefinite"/> properties.
    ///	</remarks>
    internal interface ICholeskyDecomposition {
        /// <summary>Solves a set of equation systems of type <c>A * X = B</c>.</summary>
        /// <param name="rhs">Right hand side matrix with as many rows as <c>A</c> and any number of columns.</param>
        /// <returns>Matrix <c>X</c> so that <c>L * L' * X = B</c>.</returns>
        /// <exception cref="T:System.ArgumentException">Matrix dimensions do not match.</exception>
        /// <exception cref="T:System.InvalidOperationException">Matrix is not symmetrix and positive definite.</exception>
        IMatrix Solve(IMatrix rhs);

        /// <summary>Returns <see langword="true"/> if the matrix is positive definite.</summary>
        Boolean IsPositiveDefinite { get; }

        /// <summary>Returns <see langword="true"/> if the matrix is symmetric.</summary>
        Boolean IsSymmetric { get; }

        /// <summary>Returns the left triangular factor <c>L</c> so that <c>A = L * L'</c>.</summary>
        IMatrix LeftTriangularFactor { get; }
    }
    #endregion ICholeskyDecomposition

    #region Interface ILuDecomposition

    /// <summary>
    ///   LU decomposition of a rectangular matrix.
    /// </summary>
    /// <remarks>
    ///   For an m-by-n matrix <c>A</c> with m >= n, the LU decomposition is an m-by-n
    ///   unit lower triangular matrix <c>L</c>, an n-by-n upper triangular matrix <c>U</c>,
    ///   and a permutation vector <c>piv</c> of length m so that <c>A(piv)=L*U</c>.
    ///   If m &lt; n, then <c>L</c> is m-by-m and <c>U</c> is m-by-n.
    ///   The LU decompostion with pivoting always exists, even if the matrix is
    ///   singular, so the constructor will never fail.  The primary use of the
    ///   LU decomposition is in the solution of square systems of simultaneous
    ///   linear equations. This will fail if <see cref="IsNonSingular"/> returns <see langword="false"/>.
    /// </remarks>
    
    internal interface ILuDecomposition {
        /// <summary>Returns if the matrix is non-singular.</summary>
        bool IsNonSingular { get; }

        /// <summary>Returns the determinant of the matrix.</summary>
        double Determinant { get; }

        /// <summary>Returns the lower triangular factor <c>L</c> with <c>A=LU</c>.</summary>
        IMatrix LowerTriangularFactor { get; }

        /// <summary>Returns the lower triangular factor <c>L</c> with <c>A=LU</c>.</summary>
        IMatrix UpperTriangularFactor { get; }

        /// <summary>Returns the pivot permuation vector.</summary>
        double[] PivotPermutationVector { get; }

        /// <summary>Solves a set of equation systems of type <c>A * X = B</c>.</summary>
        /// <param name="rhs">Right hand side matrix with as many rows as <c>A</c> and any number of columns.</param>
        /// <returns>Matrix <c>X</c> so that <c>L * U * X = B</c>.</returns>
        IMatrix Solve(IMatrix rhs);
    }
    #endregion ILuDecomposition

    #region Interface IQrDecomposition

    /// <summary>
    ///	  QR decomposition for a rectangular matrix.
    /// </summary>
    /// <remarks>
    ///   For an m-by-n matrix <c>A</c> with <c>m &gt;= n</c>, the QR decomposition is an m-by-n
    ///   orthogonal matrix <c>Q</c> and an n-by-n upper triangular 
    ///   matrix <c>R</c> so that <c>A = Q * R</c>.
    ///   The QR decompostion always exists, even if the matrix does not have
    ///   full rank, so the constructor will never fail.  The primary use of the
    ///   QR decomposition is in the least squares solution of nonsquare systems
    ///   of simultaneous linear equations.
    ///   This will fail if <see cref="IsFullRank"/> returns <see langword="false"/>.
    /// </remarks>
    internal interface IQrDecomposition {
        /// <summary>Shows if the matrix <c>A</c> is of full rank.</summary>
        /// <value>The value is <see langword="true"/> if <c>R</c>, and hence <c>A</c>, has full rank.</value>
        bool IsFullRank { get; }

        /// <summary>Returns the upper triangular factor <c>R</c>.</summary>
        IMatrix UpperTriangularFactor { get; }

        /// <summary>Returns the orthogonal factor <c>Q</c>.</summary>
        IMatrix OrthogonalFactor { get; }

        /// <summary>Least squares solution of <c>A * X = B</c></summary>
        /// <param name="rhs">Right-hand-side matrix with as many rows as <c>A</c> and any number of columns.</param>
        /// <returns>A matrix that minimized the two norm of <c>Q * R * X - B</c>.</returns>
        /// <exception cref="T:System.ArgumentException">Matrix row dimensions must be the same.</exception>
        /// <exception cref="T:System.InvalidOperationException">Matrix is rank deficient.</exception>
        IMatrix Solve(IMatrix rhs);
    }
    #endregion IQrDecomposition

    #region Interface ISingularValueDecomposition

    /// <summary>
    /// 	Singular Value Decomposition for a rectangular matrix.
    /// </summary>
    /// <remarks>
    ///	  For an m-by-n matrix <c>A</c> with <c>m >= n</c>, the singular value decomposition is
    ///   an m-by-n orthogonal matrix <c>U</c>, an n-by-n diagonal matrix <c>S</c>, and
    ///   an n-by-n orthogonal matrix <c>V</c> so that <c>A = U * S * V'</c>.
    ///   The singular values, <c>sigma[df] = S[df,df]</c>, are ordered so that
    ///   <c>sigma[0] >= sigma[1] >= ... >= sigma[n-1]</c>.
    ///   The singular value decompostion always exists, so the constructor will
    ///   never fail. The matrix condition number and the effective numerical
    ///   rank can be computed from this decomposition.
    /// </remarks>
    internal interface ISingularValueDecomposition {
        /// <summary>Returns the condition number <c>max(S) / min(S)</c>.</summary>
        double Condition { get; }

        /// <summary>Returns the Two norm.</summary>
        double Norm2 { get; }

        /// <summary>Returns the effective numerical matrix rank.</summary>
        /// <value>Number of non-negligible singular values.</value>
        int Rank { get; }

        /// <summary>Return the one-dimensional array of singular values.</summary>
        double[] Diagonal { get; }
    }
    #endregion ISingularValueDecomposition

    #region Interface IEigenvalueDecomposition

    /// <summary>
    ///		Determines the eigenvalues and eigenvectors of a real square matrix.
    ///	</summary>
    /// <remarks>
    ///		If <c>A</c> is symmetric, then <c>A = V * D * V'</c> and <c>A = V * V'</c>
    /// 	where the eigenvalue matrix <c>D</c> is diagonal and the eigenvector matrix <c>V</c> is orthogonal.
    /// 	If <c>A</c> is not symmetric, the eigenvalue matrix <c>D</c> is block diagonal
    /// 	with the real eigenvalues in 1-by-1 blocks and any complex eigenvalues,
    /// 	<c>lambda+i*mu</c>, in 2-by-2 blocks, <c>[lambda, mu; -mu, lambda]</c>.
    ///		The columns of <c>V</c> represent the eigenvectors in the sense that <c>A * V = V * D</c>.
    /// 	The matrix V may be badly conditioned, or even singular, so the validity of the equation
    /// 	<c>A=V*D*inverse(V)</c> depends upon the condition of <c>V</c>.
    ///	</remarks>
    internal interface IEigenvalueDecomposition {
        /// <summary>Returns the real parts of the eigenvalues.</summary>
        double[] RealEigenvalues { get; }

        /// <summary>Returns the imaginary parts of the eigenvalues.</summary>
        double[] ImaginaryEigenvalues { get; }

        /// <summary>Returns the eigenvector matrix.</summary>
        IMatrix EigenvectorMatrix { get; }

        /// <summary>Returns the block diagonal eigenvalue matrix.</summary>
        IMatrix DiagonalMatrix { get; }
    }
    #endregion IEigenvalueDecomposition

    #endregion
    
    

}
