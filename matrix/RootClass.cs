using System.Numerics;

namespace matrix;

public record ObjectSize(int Rows, int Columns);

public class RootClass<T> where T : INumber<T>
{
    public ObjectSize size { get; init; }

    protected T[,] _matrix;

    public T[,] _source => (T[,])_matrix.Clone();

    private T this[int r, int c]
    {
        get => _matrix[r, c];
        set => _matrix[r, c] = value;
    }

    protected RootClass(int rows, int columns)
    {
        _matrix = new T[rows, columns];
        size = new ObjectSize(rows, columns);
    }

    protected RootClass(ObjectSize size) : this(size.Rows, size.Columns) { }

    protected RootClass(T[,] data) : this(data.GetLength(0), data.GetLength(1))
        => _matrix = (T[,])data.Clone();

    public static bool operator ==(RootClass<T> a, RootClass<T> b)
    {
        if (a.size != b.size)
            return false;

        for (int i = 0; i < a.size.Rows; i++)
            for (int j = 0; j < a.size.Columns; j++)
                if (a[i, j] != b[i, j])
                    return false;

        return true;
    }

    public static bool operator !=(RootClass<T> a, RootClass<T> b) => !(a == b);

    public static RootClass<T> operator *(RootClass<T> a, T scalar)
    {
        RootClass<T> result = new(a.size);

        for (int i = 0; i < a.size.Rows; i++)
            for (int j = 0; j < a.size.Columns; j++)
                result[i, j] = a[i, j] * scalar;

        return result;
    }

    public static RootClass<T> operator *(T scalar, RootClass<T> a) => a * scalar;

    public static RootClass<T> operator /(RootClass<T> a, T scalar)
    {
        if (scalar == T.Zero)
            throw new DivideByZeroException("Cannot divide by zero.");

        RootClass<T> result = new(a.size);
        for (int i = 0; i < a.size.Rows; i++)
            for (int j = 0; j < a.size.Columns; j++)
                result[i, j] = a[i, j] / scalar;

        return result;
    }

    public static RootClass<T> operator +(RootClass<T> a, RootClass<T> b)
    {
        if (a.size != b.size)
            throw new InvalidOperationException("Matrices must have the same dimensions for addition.");

        return Add_Subtract_for_same(a, b, true);
    }

    public static RootClass<T> operator -(RootClass<T> a, RootClass<T> b)
    {
        if (a.size != b.size)
            throw new InvalidOperationException("Matrices must have the same dimensions for subtraction.");

        return Add_Subtract_for_same(a, b, false);
    }


    protected static RootClass<double> Add_Subtract_for_diff<LocalT>(RootClass<LocalT> a, RootClass<double> b, bool is_positive) where LocalT : INumber<LocalT>
    {
        RootClass<double> result = new(a.size);

        double sign = is_positive ? 1 : -1;

        for (int i = 0; i < a.size.Rows; i++)
            for (int j = 0; j < a.size.Columns; j++)
                result[i, j] = double.CreateChecked(a[i, j]) + b[i, j] * sign;

        return result;
    }

    protected static RootClass<LocalT> Add_Subtract_for_same<LocalT>(RootClass<LocalT> a, RootClass<LocalT> b, bool is_positive) where LocalT : INumber<LocalT>
    {
        RootClass<LocalT> result = new(a.size);


        LocalT sign = is_positive ? LocalT.One : -LocalT.One;

        for (int i = 0; i < a.size.Rows; i++)
            for (int j = 0; j < a.size.Columns; j++)
                result[i, j] = a[i, j] + b[i, j] * sign;

        return result;
    }


    public static RootClass<double> Add<LocalT>(RootClass<LocalT> a, RootClass<double> b) where LocalT : INumber<LocalT>
    {
        if (a.size != b.size)
            throw new InvalidOperationException("Matrices must have the same dimensions for addition.");

        return Add_Subtract_for_diff(a, b, true);
    }

    public static RootClass<double> Add<LocalT>(RootClass<double> a, RootClass<LocalT> b) where LocalT : INumber<LocalT> => Add(b, a);

    public static RootClass<double> Subtract<LocalT>(RootClass<LocalT> a, RootClass<double> b) where LocalT : INumber<LocalT>
    {
        if (a.size != b.size)
            throw new InvalidOperationException("Matrices must have the same dimensions for subtraction.");

        return Add_Subtract_for_diff(a, b, false);
    }

    public static RootClass<double> Subtract<LocalT>(RootClass<double> a, RootClass<LocalT> b) where LocalT : INumber<LocalT> => Subtract(b, a);

    public RootClass<T> copy() => new((T[,])_matrix.Clone());

    protected T[] GetRow(int r)
    {
        T[] result = new T[size.Columns];
        for (int j = 0; j < size.Columns; j++)
            result[j] = _matrix[r, j];

        return result;
    }

    protected T[] GetColumn(int c)
    {
        T[] result = new T[size.Rows];
        for (int i = 0; i < size.Rows; i++)
            result[i] = _matrix[i, c];
        return result;
    }

    public override string ToString() => ToString();

    public string ToString(string split = "\t", string LineEnd = "\n")
    {
        System.Text.StringBuilder sb = new();
        for (int i = 0; i < size.Rows; i++)
        {
            for (int j = 0; j < size.Columns; j++)
                sb.Append(_matrix[i, j].ToString() + split);
            sb.Append(LineEnd);
        }

        return sb.ToString();
    }

    public override bool Equals(object? obj)
        => obj is RootClass<T> other && this == other;

    public override int GetHashCode() => _matrix.GetHashCode();

    protected Vector<T> ToOneD()
    {
        Vector<T> vec = new(size.Rows * size.Columns);
        for (int i = 0; i < size.Rows; i++)
            for (int j = 0; j < size.Columns; j++)
                vec[i * size.Columns + j] = this[i, j];
        return vec;
    }

    private static List<Vector<LocalT>> ToZero<LocalT>(List<Vector<LocalT>> vectors, int x, int y) where LocalT : INumber<LocalT>
    {
        if (LocalT.IsZero(vectors[y][x]))
            return vectors;

        Vector<LocalT> current = vectors[y];
        int zero_count = current.Count(LocalT.IsZero);

        for (int i = y - 1; i >= 0; i--)
        {
            Vector<LocalT> temp = current - vectors[i] * (current[x] / vectors[i][x]);

            if (temp.Count(LocalT.IsZero) >= zero_count)
            {
                vectors[y] = temp;
                return vectors;
            }
        }

        return vectors;
    }

    public static LocalT GetGcd<LocalT>(Vector<LocalT> numbers, double epsilon = 1e-10) where LocalT : INumber<LocalT>
    {
        static LocalT CalculateGcd(LocalT a, LocalT b, LocalT epsilon)
        {
            a = LocalT.Abs(a);
            b = LocalT.Abs(b);

            while (b > epsilon)
                (a, b) = (b, a % b);
            return a;
        }

        if (numbers is null || numbers.size == 0) return LocalT.Zero;

        LocalT ep = LocalT.CreateChecked(epsilon);
        LocalT sgn = numbers.All(t => LocalT.IsNegative(t) || LocalT.IsZero(t)) ? -LocalT.One : LocalT.One;

        return numbers.Aggregate((gcd, next) => CalculateGcd(gcd, next, ep)) * sgn;
    }

    protected static IEnumerable<Vector<LocalT>> BaseOfBase<LocalT>(RootClass<LocalT>[] objects) where LocalT : INumber<LocalT>
    {
        if (objects.Length == 0 || objects.Any(b => b.size != objects[0].size))
            throw new InvalidOperationException("These objects must have the same dimensions to form a basis.");

        List<Vector<LocalT>> vectors = objects.Select(obj => obj.ToOneD()).Where(v => !v.All(LocalT.IsZero)).ToList();

        int size = objects[0].size.Rows * objects[0].size.Columns;

        if(size == 0)
            return [];

        else if (size == 1)
            return [[LocalT.One]];

        else if (vectors.Count == 0 || vectors.Count == 1)
            return vectors;

        else for (int t = 0; t < 2; t++)
        {
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < vectors.Count - x; y++)
                {
                    int targetIndex = vectors.Count - 1 - y;
                    vectors = ToZero(vectors, x, targetIndex);

                    if (vectors[targetIndex].All(LocalT.IsZero))
                    {
                        if (vectors.Count == 1)
                            return [];

                        vectors.RemoveAt(targetIndex);
                    }
                }
            }
            vectors.Reverse();
        }

        for (int i = 0; i < vectors.Count; i++)
        {
            LocalT gcd = GetGcd(vectors[i]);
            for (int j = 0; j < size; j++)
                if (!LocalT.IsZero(vectors[i][j]))
                    vectors[i][j] /= gcd;
        }

        return vectors;
    }
}
