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
    public new T[] source => GetColumn(0);
    public new int size => base.size.Rows;

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

    private Vector(RootClass<T> root) : base(root.source) { }

    public T this[int r]
    {
        get => matrix[r, 0];
        set => matrix[r, 0] = value;
    }

    public static Vector<T> operator *(Matrix<T> m, Vector<T> v)
    {
        if (m.size.Columns != v.size)
            throw new InvalidOperationException("Number of columns in the first matrix must match the number of rows in the second matrix.");

        Vector<T> result = new(v.size);

        for (int i = 0; i < m.size.Rows; i++)
            for (int k = 0; k < m.size.Columns; k++)
                result[i] += m[i, k] * v[k];

        return result;
    }

    public static Vector<T> operator /(Vector<T> matrix, T scalar) => new((RootClass<T>)matrix / scalar);
    public static Vector<T> operator *(Vector<T> matrix, T scalar) => new((RootClass<T>)matrix * scalar);
    public static Vector<T> operator +(Vector<T> a, Vector<T> b) => (Vector<T>)Add(a, b);
    public static Vector<T> operator -(Vector<T> a, Vector<T> b) => (Vector<T>)Subtract(a, b);


    public static Vector<T> ToVectorOfOne(Vector<T> v)
    {
        Vector<T> result = new(v.size);

        T divideBy = v.Norm();
        for (int i = 0; i < v.size; i++)
            result[i] = v[i] / divideBy;

        return result;
    }
    public Vector<T> ToVectorOfOne() => ToVectorOfOne(this);

    public static T Norm(Vector<T> v)
    {
        T sum = T.Zero;

        for (int i = 0; i < v.size; i++)
            sum += v[i] * v[i];

        return T.CreateChecked(Math.Sqrt(double.CreateChecked(sum)));
    }
    public T Norm() => Norm(this);


    public string ToString(string split = ", ", string start = "[", string end = "]")
    {
        System.Text.StringBuilder sb = new(start);
        for (int i = 0; i < size; i++)
            sb.Append(this[i] + (i < size - 1 ? split : end));
        return sb.ToString();
    }

    public override string ToString() => ToString();

    public Matrix<T> FromOneD(ObjectSize size)
    {
        Matrix<T> matrix = new(size);
        for (int i = 0; i < size.Rows; i++)
            for (int j = 0; j < size.Columns; j++)
                matrix[i, j] = this[i * size.Columns + j];

        return matrix;
    }

    public Vector<T> reverse()
    {
        Vector<T> result = new(size);
        for (int i = 0; i < size; i++)
            result[i] = this[size - 1 - i];
        return result;
    }

    public static List<Vector<T>> Base(Vector<T>[] vectors) => BaseOfBase(vectors);

    public virtual IEnumerator<T> GetEnumerator()
    {
        for (int i = 0; i < size; i++)
            yield return this[i];
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
