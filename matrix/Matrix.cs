namespace matrix;

public class Matrix<T> : RootClass<T> where T : System.Numerics.INumber<T>
{
    public T[][] R
    {
        get
        {
            T[][] result = new T[Size.Rows][];
            for (int i = 0; i < Size.Rows; i++)
                result[i] = GetRow(i);

            return result;
        }
    }

    public T[][] C
    {
        get
        {
            T[][] result = new T[Size.Columns][];
            for (int j = 0; j < Size.Columns; j++)
                result[j] = GetColumn(j);

            return result;
        }
    }

    public T this[int r, int c]
    {
        get => _matrix[r, c];
        set => _matrix[r, c] = value;
    }

    public T[,] source => (T[,]) _matrix.Clone();

    public bool IsSquare { get; init; }

    public Matrix(T[,] matrix) : base(matrix)  => IsSquare = matrix.GetLength(0) == matrix.GetLength(1);

    public Matrix(params T[][] matrix) : this(matrix.Length, matrix.Min(array => array.Length))
    {
        for (int i = 0; i < _matrix.GetLength(0); i++)
            for (int j = 0; j < _matrix.GetLength(1); j++)
                _matrix[i, j] = matrix[i][j];
    }

    public Matrix(ObjectSize size) : base(size) => IsSquare = size.Rows == size.Columns;

    public Matrix(int rows, int columns) : base(rows, columns) => IsSquare = rows == columns;

    private Matrix(RootClass<T> root) : this(root.Size)
        => _matrix = ((Matrix<T>)root).source;

    public static Matrix<T> operator *(Matrix<T> a, Matrix<T> b)
    {
        if (a.Size.Columns != b.Size.Rows)
            throw new InvalidOperationException("Number of columns in the first matrix must match the number of rows in the second matrix.");

        Matrix<T> result = new (a.Size.Rows, b.Size.Columns);

        for (int i = 0; i < a.Size.Rows; i++)
            for (int j = 0; j < b.Size.Columns; j++)
                for (int k = 0; k < a.Size.Columns; k++)
                    result[i, j] += a[i, k] * b[k, j];

        return result;
    }

    public static Matrix<T> operator ^(Matrix<T> a, int power)
    {
        if (!a.IsSquare)
            throw new InvalidOperationException("Matrix must be square for exponentiation.");

        if (power < 0)
            throw new ArgumentOutOfRangeException(nameof(power), "Power must be non-negative.");

        Matrix<T> result = I(a.Size.Rows);

        for (int i = 0; i < power; i++)
            result *= a;

        return result;
    }

    public static Matrix<T> operator /(Matrix<T> matrix, T scalar) => new((RootClass<T>)matrix / scalar);
    public static Matrix<T> operator +(Matrix<T> a, Matrix<T> b) => new((RootClass<T>)a + (RootClass<T>)b);
    public static Matrix<T> operator -(Matrix<T> a, Matrix<T> b) => new((RootClass<T>)a - (RootClass<T>)b);


    public void transpose_on() => _matrix = transpose().source;
    public void adj_on() => _matrix = adj().source;
    public void inv_on()
    {
        Matrix<T> inverted = inv() ??
            throw new InvalidOperationException("Matrix is singular and cannot be inverted.");

        _matrix = inverted.source;
    }

    public T det() => det(this._matrix);
    public Matrix<T> adj() => adj(this);
    public Matrix<T>? inv() => invert(this);

    public T trace()
    {
        if (!IsSquare)
            throw new InvalidOperationException("Trace is only defined for square matrices.");

        T trace = T.Zero;
        for (int i = 0; i < Size.Rows; i++)
            trace += _matrix[i, i];

        return trace;
    }

    public Matrix<T> transpose()
    {
        Matrix<T> transposed = new (Size.Columns, Size.Rows);

        for (int i = 0; i < Size.Rows; i++)
            for (int j = 0; j < Size.Columns; j++)
                transposed[j, i] = _matrix[i, j];

        return transposed;
    }

    private static T[,] get_sub(T[,] matrix, int row, int col)
    {
        T[,] sub_matrix = new T[matrix.GetLength(0) - 1, matrix.GetLength(1) - 1];
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

    private static T det(T[,] matrix)
    {
        if(matrix.GetLength(0) != matrix.GetLength(1))
            throw new InvalidOperationException("Determinant is only defined for square matrices.");

        if (matrix.GetLength(0) == 2 && matrix.GetLength(1) == 2)
            return matrix[0, 0] * matrix[1, 1] - matrix[0, 1] * matrix[1, 0];

        T determinant = T.Zero;

        for (int col = 0; col < matrix.GetLength(1); col++)
            determinant += T.CreateChecked(Math.Pow(-1, col)) * matrix[0, col] * det(get_sub(matrix, 0, col));

        return determinant;
    }

    public static T determinant(Matrix<T> matrix) => det(matrix._matrix);

    public static Matrix<T> adj(Matrix<T> matrix)
    {
        Matrix<T> adjuvate = new(matrix.Size);

        for (int row = 0; row < matrix.Size.Rows; row++)
            for (int col = 0; col < matrix.Size.Columns; col++)
                adjuvate[col, row] = T.CreateChecked(Math.Pow(-1, row + col)) * det(get_sub(matrix._matrix, row, col));

        return adjuvate;
    }

    public static Matrix<T>? invert(Matrix<T> matrix)
    {
        T determinant = det(matrix._matrix);
        if (determinant == T.Zero)
            return null;

        return adj(matrix) / determinant;
    }

    public static Matrix<T> I(int s)
    {
        Matrix<T> identity = new(s, s);

        for (int i = 0; i < s; i++)
            identity[i, i] = T.One;

        return identity;
    }

    public static Matrix<double> Multiply(Matrix<T> a, Matrix<double> b)
    {
        if (a.Size.Columns != b.Size.Rows)
            throw new InvalidOperationException("Number of columns in the first matrix must match the number of rows in the second matrix.");

        Matrix<double> result = new(a.Size.Rows, b.Size.Columns);

        for (int i = 0; i < a.Size.Rows; i++)
            for (int j = 0; j < b.Size.Columns; j++)
                for (int k = 0; k < a.Size.Columns; k++)
                    result[i, j] += double.CreateChecked(a[i, k]) * b[k, j];

        return result;
    }

    public static Matrix<double> Multiply(Matrix<double> a, Matrix<T> b) => Multiply(b, a);

    public Vector<T> ToVector() => new(R[0]);
}
