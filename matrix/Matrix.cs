using System.Numerics;

namespace matrix;

public class Matrix<T> : RootClass<T> where T : INumber<T>
{
    public T[][] R
    {
        get
        {
            T[][] result = new T[size.Rows][];
            for (int i = 0; i < size.Rows; i++)
                result[i] = GetRow(i);

            return result;
        }
    }

    public T[][] C
    {
        get
        {
            T[][] result = new T[size.Columns][];
            for (int j = 0; j < size.Columns; j++)
                result[j] = GetColumn(j);

            return result;
        }
    }

    public T this[int r, int c]
    {
        get => matrix[r, c];
        set => matrix[r, c] = value;
    }

    public new T[,] source => base.source;

    public bool IsSquare { get; init; }

    public Matrix(ObjectSize size) : base(size) => IsSquare = size.Rows == size.Columns;

    public Matrix(T[,] matrix) : base(matrix) => IsSquare = matrix.GetLength(0) == matrix.GetLength(1);

    public Matrix(params T[][] matrix) : this(matrix.Length, matrix.Min(array => array.Length))
    {
        for (int i = 0; i < base.matrix.GetLength(0); i++)
            for (int j = 0; j < base.matrix.GetLength(1); j++)
                base.matrix[i, j] = matrix[i][j];
    }

    public Matrix(int rows, int columns) : base(rows, columns) => IsSquare = rows == columns;

    private Matrix(RootClass<T> root) : this(root.size)
        => matrix = root.source;

    public static Matrix<T> operator *(Matrix<T> a, Matrix<T> b)
    {
        if (a.size.Columns != b.size.Rows)
            throw new InvalidOperationException("Number of columns in the first matrix must match the number of rows in the second matrix.");

        Matrix<T> result = new(a.size.Rows, b.size.Columns);

        for (int i = 0; i < a.size.Rows; i++)
            for (int j = 0; j < b.size.Columns; j++)
                for (int k = 0; k < a.size.Columns; k++)
                    result[i, j] += a[i, k] * b[k, j];

        return result;
    }

    public static Matrix<T> operator ^(Matrix<T> a, int power)
    {
        if (!a.IsSquare)
            throw new InvalidOperationException("Matrix must be square for exponentiation.");

        if (power < 0)
            throw new ArgumentOutOfRangeException(nameof(power), "Power must be non-negative.");

        Matrix<T> result = I(a.size.Rows);

        for (int i = 0; i < power; i++)
            result *= a;

        return result;
    }

    public static Matrix<T> operator /(Matrix<T> matrix, T scalar) => new((RootClass<T>)matrix / scalar);
    public static Matrix<T> operator +(Matrix<T> a, Matrix<T> b) => new(a + (RootClass<T>)b);
    public static Matrix<T> operator -(Matrix<T> a, Matrix<T> b) => new(a - (RootClass<T>)b);

    public T trace()
    {
        if (!IsSquare)
            throw new InvalidOperationException("Trace is only defined for square matrices.");

        T trace = T.Zero;
        for (int i = 0; i < size.Rows; i++)
            trace += matrix[i, i];

        return trace;
    }

    public Matrix<T> transpose()
    {
        Matrix<T> transposed = new(size.Columns, size.Rows);

        for (int i = 0; i < size.Rows; i++)
            for (int j = 0; j < size.Columns; j++)
                transposed[j, i] = matrix[i, j];

        return transposed;
    }

    private static LocalT[,] get_sub<LocalT>(LocalT[,] matrix, int row, int col) where LocalT : INumber<LocalT>
    {
        LocalT[,] sub_matrix = new LocalT[matrix.GetLength(0) - 1, matrix.GetLength(1) - 1];
        int sub_row = 0, sub_col = 0;

        for (int i = 0; i < matrix.GetLength(0); i++)
        {
            if (i == row) continue;

            for (int j = 0; j < matrix.GetLength(1); j++)
            {
                if (j == col) continue;

                sub_matrix[sub_row, sub_col++] = matrix[i, j];
            }
            sub_col = 0;
            sub_row++;
        }

        return sub_matrix;
    }

    private static LocalT det<LocalT>(LocalT[,] matrix) where LocalT : INumber<LocalT>
    {
        if (matrix.GetLength(0) != matrix.GetLength(1))
            throw new InvalidOperationException("Determinant is only defined for square matrices.");

        if (matrix.GetLength(0) == 2 && matrix.GetLength(1) == 2)
            return matrix[0, 0] * matrix[1, 1] - matrix[0, 1] * matrix[1, 0];

        LocalT determinant = LocalT.Zero;

        for (int col = 0; col < matrix.GetLength(1); col++)
            determinant += LocalT.CreateChecked(Math.Pow(-1, col)) * matrix[0, col] * det(get_sub(matrix, 0, col));

        return determinant;
    }
    public static LocalT determinant<LocalT>(Matrix<LocalT> matrix) where LocalT : INumber<LocalT> => det(matrix.matrix);
    public T determinant() => det(matrix);

    public static Matrix<LocalT> adj<LocalT>(Matrix<LocalT> matrix) where LocalT : INumber<LocalT>
    {
        Matrix<LocalT> adjuvate = new(matrix.size);

        for (int row = 0; row < matrix.size.Rows; row++)
            for (int col = 0; col < matrix.size.Columns; col++)
                adjuvate[col, row] = LocalT.CreateChecked(Math.Pow(-1, row + col)) * det(get_sub(matrix.matrix, row, col));

        return adjuvate;
    }
    public Matrix<T> adj() => adj(this);

    public static Matrix<LocalT>? invert<LocalT>(Matrix<LocalT> matrix) where LocalT : INumber<LocalT>
    {
        LocalT determinant = det(matrix.matrix);
        if (determinant == LocalT.Zero)
            return null;

        return adj(matrix) / determinant;
    }
    public Matrix<T>? inv() => invert(this);

    public static Matrix<T> I(int s)
    {
        Matrix<T> identity = new(s, s);

        for (int i = 0; i < s; i++)
            identity[i, i] = T.One;

        return identity;
    }

    public static Matrix<double> Multiply<LocalT>(Matrix<LocalT> a, Matrix<double> b) where LocalT : INumber<LocalT>
    {
        if (a.size.Columns != b.size.Rows)
            throw new InvalidOperationException("Number of columns in the first matrix must match the number of rows in the second matrix.");

        Matrix<double> result = new(a.size.Rows, b.size.Columns);

        for (int i = 0; i < a.size.Rows; i++)
            for (int j = 0; j < b.size.Columns; j++)
                for (int k = 0; k < a.size.Columns; k++)
                    result[i, j] += double.CreateChecked(a[i, k]) * b[k, j];

        return result;
    }

    public static Matrix<double> Multiply<LocalT>(Matrix<double> a, Matrix<LocalT> b) where LocalT : INumber<LocalT> => Multiply(b, a);

    public Vector<T> ToVector() => new(R.First());

    public static Matrix<LocalT>[] Base<LocalT>(Matrix<LocalT>[] matrices) where LocalT : INumber<LocalT>
        => BaseOfBase(matrices).Select(v => v.FromOneD(matrices.First().size)).ToArray();
}
