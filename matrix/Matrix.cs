using System.Numerics;

namespace matrix;

public class Matrix<T> : RootClass<T>,
    IMultiplyOperators<Matrix<T>, Matrix<T>, Matrix<T>>,
    IAdditionOperators<Matrix<T>, Matrix<T>, Matrix<T>>,
    ISubtractionOperators<Matrix<T>, Matrix<T>, Matrix<T>>,
    IDivisionOperators<Matrix<T>, T, Matrix<T>>
    where T : INumber<T>
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

    public new T this[int r, int c]
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
        for (int i = 0; i < size.Rows; i++)
            for (int j = 0; j < size.Columns; j++)
                this.matrix[i, j] = matrix[i][j];
    }

    public Matrix(int rows, int columns) : base(rows, columns) => IsSquare = rows == columns;

    public Matrix(RootClass<T> root) : this(root.size) => matrix = root.source;

    public static Matrix<T> operator *(Matrix<T> a, Matrix<T> b) => new(RootClassExtensions.Multiply<T, T, T>(a, b));

    public static Matrix<T> operator /(Matrix<T> matrix, T scalar) => new((RootClass<T>)matrix / scalar);
    public static Matrix<T> operator +(Matrix<T> a, Matrix<T> b) => new(RootClassExtensions.Add(a, b));
    public static Matrix<T> operator -(Matrix<T> a, Matrix<T> b) => new(RootClassExtensions.Subtract(a, b));

    public static Matrix<T> Pow(Matrix<T> matrix, int power)
    {
        if (!matrix.IsSquare)
            throw new InvalidOperationException("Matrix must be square for exponentiation.");

        Matrix<T> result = Identity(matrix.size.Rows);

        if (power < 0)
        {
            if (matrix.Determinant() == T.Zero)
                throw new InvalidOperationException("Matrix is singular and cannot be inverted.");

            matrix = matrix.Invert()!;
            power = -power;
        }


        for (int i = 0; i < power; i++)
            result *= matrix;

        return result;
    }

    public static Matrix<T> Identity(int s)
    {
        Matrix<T> identity = new(s, s);

        for (int i = 0; i < s; i++)
            identity[i, i] = T.One;

        return identity;
    }

    public Vector<T> ToVector() => new(R.First());

    public static List<Matrix<T>> Base(Matrix<T>[] matrices)
        => BaseOfBase(matrices).Select(v => v.FromOneD(matrices.First().size)).ToList();
}

public static class MatrixExtensions
{
    

    public static Matrix<T>? Invert<T>(this Matrix<T> matrix) where T : INumber<T>
    {
        T determinant = det(matrix.matrix);
        if (determinant == T.Zero)
            return null;

        return Adjuvate(matrix) / determinant;
    }

    public static Matrix<T> Adjuvate<T>(this Matrix<T> matrix) where T : INumber<T>
    {
        Matrix<T> adjuvate = new(matrix.size);

        Func<int, T> sign = x => (x % 2 == 0) ? T.One : -T.One;

        for (int row = 0; row < matrix.size.Rows; row++)
            for (int col = 0; col < matrix.size.Columns; col++)
                adjuvate[col, row] = sign(row + col) * det(get_sub(matrix.matrix, row, col));

        adjuvate.Transpose();

        return adjuvate;
    }

    public static T Determinant<T>(this Matrix<T> matrix) where T : INumber<T> => det(matrix.matrix);

    public static T Trace<T>(this Matrix<T> matrix) where T : INumber<T>
    {
        int lim = Math.Min(matrix.size.Rows, matrix.size.Columns);

        T trace = T.Zero;
        for (int i = 0; i < lim; i++)
            trace += matrix[i, i];

        return trace;
    }

    private static T[,] get_sub<T>(T[,] matrix, int row, int col) where T : INumber<T>
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

    private static T det<T>(T[,] matrix) where T : INumber<T>
    {
        if (matrix.GetLength(0) != matrix.GetLength(1))
            throw new InvalidOperationException("Determinant is only defined for square matrices.");

        if (matrix.GetLength(0) == 2 && matrix.GetLength(1) == 2)
            return matrix[0, 0] * matrix[1, 1] - matrix[0, 1] * matrix[1, 0];

        T determinant = T.Zero;

        for (int col = 0; col < matrix.GetLength(1); col++)
            if (matrix[0, col] != T.Zero)
                determinant += T.CreateChecked(Math.Pow(-1, col)) * matrix[0, col] * det(get_sub(matrix, 0, col));

        return determinant;
    }
}
