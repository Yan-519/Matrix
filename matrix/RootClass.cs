namespace matrix;

public record ObjectSize(int Rows, int Columns);

public class RootClass<T> where T : System.Numerics.INumber<T>
{
    public ObjectSize Size { get; init; }

    protected T[,] _matrix;

    private T this[int r, int c]
    {
        get => _matrix[r, c];
        set => _matrix[r, c] = value;
    }

    protected RootClass(int rows, int columns)
    {
        _matrix = new T[rows, columns];
        Size = new ObjectSize(rows, columns);
    }


    protected RootClass(ObjectSize size) : this(size.Rows, size.Columns) { }

    protected RootClass(T[,] data) : this(data.GetLength(0), data.GetLength(1)) => _matrix = (T[,])data.Clone();

    public static bool operator ==(RootClass<T> a, RootClass<T> b)
    {
        if (a.Size != b.Size)
            return false;

        for (int i = 0; i < a.Size.Rows; i++)
            for (int j = 0; j < a.Size.Columns; j++)
                if (a[i, j] != b[i, j])
                    return false;

        return true;
    }

    public static bool operator !=(RootClass<T> a, RootClass<T> b) => !(a == b);

    public static RootClass<T> operator *(RootClass<T> a, T scalar)
    {
        RootClass<T> result = new(a.Size);

        for (int i = 0; i < a.Size.Rows; i++)
            for (int j = 0; j < a.Size.Columns; j++)
                result[i, j] = a[i, j] * scalar;

        return result;
    }

    public static RootClass<T> operator *(T scalar, RootClass<T> a) => a * scalar;

    public static RootClass<T> operator /(RootClass<T> a, T scalar)
    {
        if (scalar == T.Zero)
            throw new DivideByZeroException("Cannot divide by zero.");

        RootClass<T> result = new(a.Size);
        for (int i = 0; i < a.Size.Rows; i++)
            for (int j = 0; j < a.Size.Columns; j++)
                result[i, j] = a[i, j] / scalar;

        return result;
    }

    public static RootClass<T> operator +(RootClass<T> a, RootClass<T> b)
    {
        if (a.Size != b.Size)
            throw new InvalidOperationException("Matrices must have the same dimensions for addition.");

        return Add_Subtract_for_same(a, b, true);
    }

    public static RootClass<T> operator -(RootClass<T> a, RootClass<T> b)
    {
        if (a.Size != b.Size)
            throw new InvalidOperationException("Matrices must have the same dimensions for subtraction.");

        return Add_Subtract_for_same(a, b, false);
    }


    protected static RootClass<double> Add_Subtract_for_diff(RootClass<T> a, RootClass<double> b, bool is_positive)
    {
        RootClass<double> result = new(a.Size);

        double sign = is_positive ? 1 : -1;

        for (int i = 0; i < a.Size.Rows; i++)
            for (int j = 0; j < a.Size.Columns; j++)
                result[i, j] = double.CreateChecked(a[i, j]) + b[i, j] * sign;

        return result;
    }

    protected static RootClass<T> Add_Subtract_for_same(RootClass<T> a, RootClass<T> b, bool is_positive)
    {
        RootClass<T> result = new(a.Size);

        T sign = is_positive ? T.One : -T.One;

        for (int i = 0; i < a.Size.Rows; i++)
            for (int j = 0; j < a.Size.Columns; j++)
                result[i, j] = a[i, j] + b[i, j] * sign;

        return result;
    }


    public static RootClass<double> Add(RootClass<T> a, RootClass<double> b)
    {
        if (a.Size != b.Size)
            throw new InvalidOperationException("Matrices must have the same dimensions for addition.");

        return Add_Subtract_for_diff(a, b, true);
    }

    public static RootClass<double> Add(RootClass<double> a, RootClass<T> b) => Add(b, a);

    public static RootClass<double> Subtract(RootClass<T> a, RootClass<double> b)
    {
        if (a.Size != b.Size)
            throw new InvalidOperationException("Matrices must have the same dimensions for subtraction.");

        return Add_Subtract_for_diff(a, b, false);
    }

    public static RootClass<double> Subtract(RootClass<double> a, RootClass<T> b) => Subtract(b, a);

    public RootClass<T> copy() => new((T[,])_matrix.Clone());

    protected T[] GetRow(int r)
    {
        T[] result = new T[Size.Columns];
        for (int j = 0; j < Size.Columns; j++)
            result[j] = _matrix[r, j];

        return result;
    }

    public T[] GetColumn(int c)
    {
        T[] result = new T[Size.Rows];
        for (int i = 0; i < Size.Rows; i++)
            result[i] = _matrix[i, c];
        return result;
    }

    public override string ToString()
    {
        System.Text.StringBuilder sb = new();
        for (int i = 0; i < Size.Rows; i++)
        {
            for (int j = 0; j < Size.Columns; j++)
                sb.Append(_matrix[i, j].ToString() + "\t");
            sb.Append('\n');
        }

        return sb.ToString();
    }

    public override bool Equals(object? obj)
        => obj is RootClass<T> other && this == other;

    public override int GetHashCode() => _matrix.GetHashCode();
}
