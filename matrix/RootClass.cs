using System.Numerics;

namespace matrix;

public record struct ObjectSize(int Rows, int Columns);

public class RootClass<T>(int rows, int columns) where T : INumber<T>
{
    public ObjectSize size { get; init; } = new ObjectSize(rows, columns);

    public T[,] matrix { get; init; } = new T[rows, columns];

    public T[,] source => (T[,])matrix.Clone();

    internal T this[int r, int c]
    {
        get => matrix[r, c];
        set => matrix[r, c] = value;
    }

    public RootClass(ObjectSize size) : this(size.Rows, size.Columns) { }

    protected RootClass(T[,] data) : this(data.GetLength(0), data.GetLength(1))
        => matrix = (T[,])data.Clone();

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

    public static RootClass<T> operator *(RootClass<T> a, T scalar) => RootClassExtensions.Multiply<T, T, T>(a, scalar);

    public static RootClass<T> operator *(RootClass<T> a, RootClass<T> b) => RootClassExtensions.Multiply<T, T, T>(a, b);

    public static RootClass<T> operator *(T scalar, RootClass<T> a) => a * scalar;

    public static RootClass<T> operator /(RootClass<T> a, T scalar) => a * (T.One / scalar);

    public static RootClass<T> operator +(RootClass<T> a, RootClass<T> b) => RootClassExtensions.Add(a, b);

    public static RootClass<T> operator -(RootClass<T> a, RootClass<T> b) => RootClassExtensions.Subtract(a, b);


    public RootClass<T> copy() => new((T[,])matrix.Clone());

    protected T[] GetRow(int r)
    {
        T[] result = new T[size.Columns];
        for (int j = 0; j < size.Columns; j++)
            result[j] = matrix[r, j];

        return result;
    }

    protected T[] GetColumn(int c)
    {
        T[] result = new T[size.Rows];
        for (int i = 0; i < size.Rows; i++)
            result[i] = matrix[i, c];
        return result;
    }

    public override string ToString() => ToString("\t", "\n", "|");

    public string ToString(string split, string LineEnd, string Border)
    {
        System.Text.StringBuilder sb = new();
        for (int i = 0; i < size.Rows; i++)
        {
            sb.Append(Border);
            for (int j = 0; j < size.Columns -1; j++)
                sb.Append(matrix[i, j].ToString() + split);

            sb.Append(matrix[i, size.Columns - 1].ToString());
            sb.Append(Border);
            sb.Append(LineEnd);
        }

        return sb.ToString();
    }

    public override bool Equals(object? obj)
        => obj is RootClass<T> other && this == other;

    public override int GetHashCode() => matrix.GetHashCode();

    protected Vector<T> ToOneD()
    {
        Vector<T> vec = new(size.Rows * size.Columns);
        for (int i = 0; i < size.Rows; i++)
            for (int j = 0; j < size.Columns; j++)
                vec[i * size.Columns + j] = this[i, j];
        return vec;
    }

    public RootClass<T> ForEach(Func<T, T> func)
    {
        RootClass<T> result = new(size);
        for (int i = 0; i < size.Rows; i++)
            for (int j = 0; j < size.Columns; j++)
                result[i, j] = func(this[i, j]);
        return result;
    }

    private static T GetGcd(Vector<T> numbers, double epsilon = 1e-10)
    {
        static T CalculateGcd(T a, T b, T epsilon)
        {
            a = T.Abs(a);
            b = T.Abs(b);

            while (b > epsilon)
                (a, b) = (b, a % b);
            return a;
        }

        if (numbers is null || numbers.size == 0) return T.Zero;

        T ep = T.CreateChecked(epsilon);
        T sgn = numbers.All(t => T.IsNegative(t) || T.IsZero(t)) ? -T.One : T.One;

        return numbers.Aggregate((gcd, next) => CalculateGcd(gcd, next, ep)) * sgn;
    }

    protected static List<Vector<T>> BaseOfBase(RootClass<T>[] objects)
    {
        static List<Vector<T>> ToZero(List<Vector<T>> vectors, int x, int y)
        {
            if (T.IsZero(vectors[y][x]))
                return vectors;

            Vector<T> current = vectors[y];
            int zero_count = current.Count(T.IsZero);

            for (int i = y - 1; i >= 0; i--)
            {
                Vector<T> temp = current - vectors[i] * (current[x] / vectors[i][x]);

                if (temp.Count(T.IsZero) >= zero_count)
                {
                    vectors[y] = temp;
                    return vectors;
                }
            }

            return vectors;
        }

        if (objects.Length == 0 || objects.Any(b => b.size != objects.First().size))
            throw new InvalidOperationException("These objects must have the same dimensions to form a basis.");

        List<Vector<T>> vectors = objects.Select(obj => obj.ToOneD()).Where(v => !v.All(T.IsZero)).ToList();

        int size = objects.First().size.Rows * objects.First().size.Columns;

        if (size == 0 || vectors.Count == 0)
            return [];

        else if (size == 1)
            return [[T.One]];

        else if (vectors.Count != 1)
            for (int t = 0; t < 2; t++)
            {
                for (int x = 0; x < size; x++)
                {
                    for (int y = 0; y < vectors.Count - x; y++)
                    {
                        int targetIndex = vectors.Count - 1 - y;
                        vectors = ToZero(vectors, x, targetIndex);

                        if (vectors[targetIndex].All(T.IsZero))
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
            T gcd = GetGcd(vectors[i]);
            for (int j = 0; j < size; j++)
                if (!T.IsZero(vectors[i][j]))
                    vectors[i][j] /= gcd;
        }

        return vectors;
    }
}

public static class RootClassExtensions
{
    public static RootClass<T> Multiply<T, T1, T2>(this RootClass<T1> a, RootClass<T2> b) where T1 : INumber<T1> where T2 : INumber<T2> where T : INumber<T>
    {
        if (a.size.Columns != b.size.Rows)
            throw new InvalidOperationException("Number of columns in the first matrix must match the number of rows in the second matrix.");

        RootClass<T> result = new(a.size.Rows, b.size.Columns);

        for (int i = 0; i < a.size.Rows; i++)
            for (int j = 0; j < b.size.Columns; j++)
                for (int k = 0; k < a.size.Columns; k++)
                    result[i, j] += T.CreateChecked(a[i, k]) * T.CreateChecked(b[k, j]);

        return result;
    }

    public static RootClass<T> Round<T>(this RootClass<T> matrix, int rounded) where T : INumber<T>
    {
        RootClass<T> result = new(matrix.size);
        T factor = T.CreateChecked(Math.Pow(10, rounded));

        for (int i = 0; i < matrix.size.Rows; i++)
            for (int j = 0; j < matrix.size.Columns; j++)
                result[i, j] = T.CreateChecked(Math.Round(decimal.CreateChecked(matrix[i, j] * factor))) / factor;
        return result;
    }

    public static RootClass<T> Transpose<T>(this RootClass<T> matrix) where T : INumber<T>
    {
        RootClass<T> result = new(matrix.size.Columns, matrix.size.Rows);
        for (int i = 0; i < matrix.size.Rows; i++)
            for (int j = 0; j < matrix.size.Columns; j++)
                result[j, i] = matrix[i, j];
        return result;
    }

    public static bool AreEqual<T, T2>(this RootClass<T> a, RootClass<T2> b, double epsilon = 1e-10) where T : INumber<T> where T2 : INumber<T2>
    {
        if (ReferenceEquals(a, b))
            return true;

        if (a is null || b is null || a.size != b.size)
            return false;

        T ep = T.CreateChecked(epsilon);

        for (int i = 0; i < a.size.Rows; i++)
        {
            for (int j = 0; j < a.size.Columns; j++)
            {
                T da = T.CreateChecked(a[i, j]);
                T db = T.CreateChecked(b[i, j]);
                if (T.Abs(da - db) > ep)
                    return false;
            }
        }

        return true;
    }

    public static RootClass<T> Multiply<T, T1, T2>(this RootClass<T> a, T2 scalar) where T : INumber<T> where T2 : INumber<T2>
    {
        RootClass<T> result = new(a.size);
        T scalarT = T.CreateChecked(scalar);

        for (int i = 0; i < a.size.Rows; i++)
            for (int j = 0; j < a.size.Columns; j++)
                result[i, j] = T.CreateChecked(a[i, j]) * scalarT;

        return result;
    }

    public static RootClass<T> Add<T, T2>(this RootClass<T> a, RootClass<T2> b) where T : INumber<T> where T2 : INumber<T2>
        => Add_Subtract_for_diff(a, b, T.One);

    public static RootClass<T> Subtract<T, T2>(this RootClass<T> a, RootClass<T2> b) where T : INumber<T> where T2 : INumber<T2>
        => Add_Subtract_for_diff(a, b, -T.One);

    private static RootClass<T> Add_Subtract_for_diff<T, T2>(RootClass<T> a, RootClass<T2> b, T sign) where T : INumber<T> where T2 : INumber<T2>
    {
        if (a.size != b.size)
            throw new InvalidOperationException("Matrices must have the same dimensions for addition, subtraction.");

        RootClass<T> result = new(a.size);

        for (int i = 0; i < a.size.Rows; i++)
            for (int j = 0; j < a.size.Columns; j++)
                result[i, j] = T.CreateChecked(a[i, j]) + T.CreateChecked(b[i, j]) * sign;

        return result;
    }
}
