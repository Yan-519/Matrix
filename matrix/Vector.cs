using System.Collections;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace matrix;

internal static class VectorBuilder
{
    internal static Vector<T> Create<T>(ReadOnlySpan<T> values) where T : INumber<T> => new(values);
}

[CollectionBuilder(typeof(VectorBuilder), "Create")]
public class Vector<T> : RootClass<T>, IEnumerable<T> where T : INumber<T>
{
    public T[] source => GetColumn(0);
    public int size => _Size.Rows;

    public Vector(int size) : base(size, 1) { }

    public Vector(ObjectSize size) : base(size)
    {
        if (size.Columns != 1)
            throw new ArgumentException("A vector must have exactly one column.", nameof(size));
    }

    public Vector(params T[] data) : base(data.Length, 1)
    {
        for (int i = 0; i < data.Length; i++)
            this[i] = data[i];
    }

    public Vector(ReadOnlySpan<T> data) : this(data.ToArray()) { }

    private Vector(RootClass<T> root) : base(((Matrix<T>)root).source) { }

    public T this[int r]
    {
        get => _matrix[r, 0];
        set => _matrix[r, 0] = value;
    }

    public static Vector<T> operator *(Matrix<T> m, Vector<T> v)
    {
        if (m._Size.Columns != v._Size.Rows)
            throw new InvalidOperationException("Number of columns in the first matrix must match the number of rows in the second matrix.");

        Vector<T> result = new(v._Size.Rows);

        for (int i = 0; i < m._Size.Rows; i++)
            for (int k = 0; k < m._Size.Columns; k++)
                result[i] += m[i, k] * v[k];

        return result;
    }

    public static Vector<T> operator /(Vector<T> matrix, T scalar) => new((RootClass<T>)matrix / scalar);
    public static Vector<T> operator *(Vector<T> matrix, T scalar) => new((RootClass<T>)matrix * scalar);
    public static Vector<T> operator +(Vector<T> a, Vector<T> b) => new((RootClass<T>)a + (RootClass<T>)b);
    public static Vector<T> operator -(Vector<T> a, Vector<T> b) => new((RootClass<T>)a - (RootClass<T>)b);


    public static Vector<T> ToVectorOfOne(Vector<T> v)
    {
        Vector<T> result = new(v._Size);

        T divideBy = v.Norm();
        for (int i = 0; i < v._Size.Rows; i++)
            result[i] = v[i] / divideBy;

        return result;
    }
    public Vector<T> ToVectorOfOne() => ToVectorOfOne(this);

    public static T Norm(Vector<T> v)
    {
        T sum = T.Zero;

        for (int i = 0; i < v._Size.Rows; i++)
            sum += v[i] * v[i];

        return T.CreateChecked(Math.Sqrt(double.CreateChecked(sum)));
    }
    public T Norm() => Norm(this);


    public Matrix<T> AsMatrix() => new((T[,])_matrix.Clone());

    public virtual IEnumerator<T> GetEnumerator()
    {
        for (int i = 0; i < _Size.Rows; i++)
            yield return this[i];
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
