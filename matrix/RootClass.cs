namespace matrix;

public record ObjectSize(int Rows, int Columns);

public class RootClass<T> where T : System.Numerics.INumber<T>
{
    public ObjectSize _Size { get; init; }

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
        _Size = new ObjectSize(rows, columns);
    }


    protected RootClass(ObjectSize size) : this(size.Rows, size.Columns) { }

    protected RootClass(T[,] data) : this(data.GetLength(0), data.GetLength(1)) => _matrix = (T[,])data.Clone();

    public static bool operator ==(RootClass<T> a, RootClass<T> b)
    {
        if (a._Size != b._Size)
            return false;

        for (int i = 0; i < a._Size.Rows; i++)
            for (int j = 0; j < a._Size.Columns; j++)
                if (a[i, j] != b[i, j])
                    return false;

        return true;
    }

    public static bool operator !=(RootClass<T> a, RootClass<T> b) => !(a == b);

    public static RootClass<T> operator *(RootClass<T> a, T scalar)
    {
        RootClass<T> result = new(a._Size);

        for (int i = 0; i < a._Size.Rows; i++)
            for (int j = 0; j < a._Size.Columns; j++)
                result[i, j] = a[i, j] * scalar;

        return result;
    }

    public static RootClass<T> operator *(T scalar, RootClass<T> a) => a * scalar;

    public static RootClass<T> operator /(RootClass<T> a, T scalar)
    {
        if (scalar == T.Zero)
            throw new DivideByZeroException("Cannot divide by zero.");

        RootClass<T> result = new(a._Size);
        for (int i = 0; i < a._Size.Rows; i++)
            for (int j = 0; j < a._Size.Columns; j++)
                result[i, j] = a[i, j] / scalar;

        return result;
    }

    public static RootClass<T> operator +(RootClass<T> a, RootClass<T> b)
    {
        if (a._Size != b._Size)
            throw new InvalidOperationException("Matrices must have the same dimensions for addition.");

        return Add_Subtract_for_same(a, b, true);
    }

    public static RootClass<T> operator -(RootClass<T> a, RootClass<T> b)
    {
        if (a._Size != b._Size)
            throw new InvalidOperationException("Matrices must have the same dimensions for subtraction.");

        return Add_Subtract_for_same(a, b, false);
    }


    protected static RootClass<double> Add_Subtract_for_diff(RootClass<T> a, RootClass<double> b, bool is_positive)
    {
        RootClass<double> result = new(a._Size);

        double sign = is_positive ? 1 : -1;

        for (int i = 0; i < a._Size.Rows; i++)
            for (int j = 0; j < a._Size.Columns; j++)
                result[i, j] = double.CreateChecked(a[i, j]) + b[i, j] * sign;

        return result;
    }

    protected static RootClass<T> Add_Subtract_for_same(RootClass<T> a, RootClass<T> b, bool is_positive)
    {
        RootClass<T> result = new(a._Size);

        T sign = is_positive ? T.One : -T.One;

        for (int i = 0; i < a._Size.Rows; i++)
            for (int j = 0; j < a._Size.Columns; j++)
                result[i, j] = a[i, j] + b[i, j] * sign;

        return result;
    }


    public static RootClass<double> Add(RootClass<T> a, RootClass<double> b)
    {
        if (a._Size != b._Size)
            throw new InvalidOperationException("Matrices must have the same dimensions for addition.");

        return Add_Subtract_for_diff(a, b, true);
    }

    public static RootClass<double> Add(RootClass<double> a, RootClass<T> b) => Add(b, a);

    public static RootClass<double> Subtract(RootClass<T> a, RootClass<double> b)
    {
        if (a._Size != b._Size)
            throw new InvalidOperationException("Matrices must have the same dimensions for subtraction.");

        return Add_Subtract_for_diff(a, b, false);
    }

    public static RootClass<double> Subtract(RootClass<double> a, RootClass<T> b) => Subtract(b, a);

    public RootClass<T> copy() => new((T[,])_matrix.Clone());

    protected T[] GetRow(int r)
    {
        T[] result = new T[_Size.Columns];
        for (int j = 0; j < _Size.Columns; j++)
            result[j] = _matrix[r, j];

        return result;
    }

    protected T[] GetColumn(int c)
    {
        T[] result = new T[_Size.Rows];
        for (int i = 0; i < _Size.Rows; i++)
            result[i] = _matrix[i, c];
        return result;
    }

    public override string ToString() => ToString();

    public string ToString(string split = "\t", string LineEnd = "\n")
    {
        System.Text.StringBuilder sb = new();
        for (int i = 0; i < _Size.Rows; i++)
        {
            for (int j = 0; j < _Size.Columns; j++)
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
        Vector<T> vec = new(_Size.Rows * _Size.Columns);
        for (int i = 0; i < _Size.Rows; i++)
            for (int j = 0; j < _Size.Columns; j++)
                vec[i * _Size.Columns + j] = this[i, j];
        return vec;
    }


    private static Vector<T>[] ToZero(Vector<T>[] vectors, int x, int y, bool is_reverse = false)
    {
        if (vectors[y][x] == T.Zero)
            return vectors;

        Vector<T> currnt = vectors[y];
        int zero_count = currnt.Count(t => t == T.Zero);

        if (!is_reverse)
            for (int i = y - 1; i >= 0; i--)
            {
                Vector<T> temp = currnt - vectors[i] * (currnt[x] / vectors[i][x]);

                if (temp.Count(t => t == T.Zero) >= zero_count)
                {
                    vectors[y] = temp;
                    return vectors;
                }
            }

        else
        {
            for (int i = y; i < vectors.Length; i++)
            {
                Vector<T> temp = currnt - vectors[i] * (currnt[x] / vectors[i][x]);

                if (temp.Count(t => t == T.Zero) >= zero_count)
                {
                    vectors[y] = temp;
                    return vectors;
                }
            }
        }

        return vectors;
    }

    public static T GetGcd(Vector<T> numbers, double epsilon = 1e-10)
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

    protected static Vector<T>[] BaseOfBase(RootClass<T>[] objects)
    {
        if (objects.Length == 0 || objects.Any(b => b._Size != objects[0]._Size))
            throw new InvalidOperationException("These objects must have the same dimensions to form a basis.");

        if (objects[0]._Size.Columns * objects[0]._Size.Rows == 1)
            return [new Vector<T>(T.One)];

        Vector<T>[] vectors = objects.Select(obj => obj.ToOneD()).ToArray();

        if (objects.Length == 1)
            return vectors;

        vectors = vectors.Where(v => v.Any(t => t != T.Zero)).ToArray();
        if (vectors.Length == 0)
            return [new Vector<T>(objects[0]._Size.Rows * objects[0]._Size.Columns)];


        int stop = Math.Max(objects[0]._Size.Columns, objects[0]._Size.Rows) - 1;

        for (int x = 0; x < vectors[0].size - 1; x++)
        {
            for (int y = 0; y < objects.Length - x; y++)
            {
                vectors = ToZero(vectors, x, vectors.Length - 1 - y)
                    .Where(v => v.Any(t => t != T.Zero)).ToArray();
            }
        }


        //for (int x = 0; x < vectors[0].size - 1; x++)
        //{
        //    Console.WriteLine(vectors.First());
        //    vectors = ToZero(vectors, x, 0, true)
        //            .Where(v => v.Any(t => t != T.Zero)).ToArray();
        //}

        //Console.WriteLine();

        for (int i = 0; i < vectors.Length; i++)
        {
            T gcd = GetGcd(vectors[i]);
            for (int j = 0; j < vectors[i].size; j++)
                if (!T.IsZero(vectors[i][j]))
                    vectors[i][j] /= gcd;
        }

        vectors = Enumerable.OrderBy(vectors, v => v.Count(t => T.IsZero(t))).ToArray();

        return vectors;
    }
}
