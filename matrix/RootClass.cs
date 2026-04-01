using System.Numerics;

namespace matrix;

public record ObjectSize(int Rows, int Columns);

public class RootClass<T> where T : INumber<T>
{
    public ObjectSize size { get; init; }

    protected T[,] matrix;

    public T[,] source => (T[,])matrix.Clone();

    private T this[int r, int c]
    {
        get => matrix[r, c];
        set => matrix[r, c] = value;
    }

    protected RootClass(int rows, int columns)
    {
        matrix = new T[rows, columns];
        size = new ObjectSize(rows, columns);
    }

    protected RootClass(ObjectSize size) : this(size.Rows, size.Columns) { }

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

    public static RootClass<T> operator *(RootClass<T> a, T scalar) => Multiply(a, scalar);

    public static RootClass<T> operator *(T scalar, RootClass<T> a) => a * scalar;

    public static RootClass<T> operator /(RootClass<T> a, T scalar) => a * (T.One / scalar);

    public static RootClass<T> operator +(RootClass<T> a, RootClass<T> b)
        => Add_Subtract_for_diff(a, b, T.One);

    public static RootClass<T> operator -(RootClass<T> a, RootClass<T> b)
        => Add_Subtract_for_diff(a, b, -T.One);

    public static RootClass<T> Multiply<T1, T2>(RootClass<T1> a, T2 scalar) where T1 : INumber<T1> where T2 : INumber<T2>
    {
        RootClass<T> result = new(a.size);
        T scalarT = T.CreateChecked(scalar);

        for (int i = 0; i < a.size.Rows; i++)
            for (int j = 0; j < a.size.Columns; j++)
                result[i, j] = T.CreateChecked(a[i, j]) * scalarT;

        return result;
    }

    public void Multiply<TOther>(TOther scalar) where TOther : INumber<TOther>
        => Multiply(this, scalar);

    protected static RootClass<T> Add_Subtract_for_diff<T1, T2>(RootClass<T1> a, RootClass<T2> b, T sign) where T1 : INumber<T1> where T2 : INumber<T2>
    {
        if (a.size != b.size)
            throw new InvalidOperationException("Matrices must have the same dimensions for addition, subtraction.");

        RootClass<T> result = new(a.size);

        for (int i = 0; i < a.size.Rows; i++)
            for (int j = 0; j < a.size.Columns; j++)
                result[i, j] = T.CreateChecked(a[i, j]) + T.CreateChecked(b[i, j]) * sign;

        return result;
    }

    public static RootClass<T> Add<T1, T2>(RootClass<T1> a, RootClass<T2> b) where T1 : INumber<T1> where T2 : INumber<T2>
        => Add_Subtract_for_diff(a, b, T.One);

    public static RootClass<T> Subtract<T1, T2>(RootClass<T1> a, RootClass<T2> b) where T1 : INumber<T1> where T2 : INumber<T2>
        => Add_Subtract_for_diff(a, b, -T.One);

    public void Add<TOther>(RootClass<TOther> other) where TOther : INumber<TOther>
        => Add(this, other);

    public void Subtract<TOther>(RootClass<TOther> other) where TOther : INumber<TOther>
        => Subtract(this, other);

    public static bool AreEqual<T1, T2>(RootClass<T1> a, RootClass<T2> b, double epsilon = 1e-10)
        where T1 : INumber<T1> where T2 : INumber<T2>
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

    public bool IsEqual<TOther>(RootClass<TOther> a, double epsilon = 1e-10) where TOther : INumber<TOther>
        => AreEqual(this, a, epsilon);

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

    public override string ToString() => ToString("\t", "\n");

    public string ToString(string split, string LineEnd)
    {
        System.Text.StringBuilder sb = new();
        for (int i = 0; i < size.Rows; i++)
        {
            for (int j = 0; j < size.Columns; j++)
                sb.Append(matrix[i, j].ToString() + split);
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
