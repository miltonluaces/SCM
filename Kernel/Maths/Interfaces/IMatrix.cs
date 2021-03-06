namespace Maths {

    internal interface IMatrix  {

        int Rows { get; }
        int Cols { get; }
        double this[int i, int j] { get; set; }
        
        bool Set(double valueForAll);  
        IMatrix Submatrix(int startRow, int endRow, int startColumn, int endColumn);
        IMatrix Submatrix(int[] rowIndexes, int[] colIndexes);
        IMatrix Submatrix(int startRow, int endRow, int[] colIndexes);
        IMatrix Submatrix(int[] rowIndexes, int startColumn, int endColumn);
        IMatrix Clone();
        double ToScalar();
        void CopyToRightUpperCorner();
        double[,] ConvertToDouble();
        IMatrix Round(int precision);
        
        IMatrix Transpose();
        IMatrix Inverse { get; }
        double Determinant { get; }
        double Norm1 { get; }
        double Trace { get; }
        double InfinityNorm { get; }
        double FrobeniusNorm { get; }
        
        bool IsSquare { get; }
        bool IsSymmetric { get; }
        bool IsZero { get; }
        
        IMatrix Addition(IMatrix B);
        IMatrix Subtraction(IMatrix B);
        IMatrix Multiply(IMatrix B);
        IMatrix MultiplyT(IMatrix B);
        IMatrix MakeSimetric();
        IMatrix MultiplyTranspose(bool reverse, int precision);
        IMatrix Multiply(double s);
        IMatrix Solve(IMatrix rhs);
        
        ICholeskyDecomposition GetCholeskyDecomposition();
        ILuDecomposition GetLuDecomposition();
        ISingularValueDecomposition GetSingularValueDecomposition();
        IQrDecomposition GetQrDecomposition();
        IEigenvalueDecomposition GetEigenvalueDecomposition();
    }
}
