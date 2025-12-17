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
    public T[] source => GetRow(0);

    public Vector(int size) : base(size, 1) { }

    public Vector(params T[] data) : base(data.Length, 1)
    {
        for (int i = 0; i < data.Length; i++)
            this[i] = data[i];
    }

    public Vector(ReadOnlySpan<T> data) : this(data.ToArray()) { }

    private Vector(RootClass<T> root) : this(root.GetColumn(0)) { }

    public T this[int r]
    {
        get => _matrix[r, 0];
        set => _matrix[r, 0] = value;
    }

    public static Vector<T> operator *(Matrix<T> m, Vector<T> v)
    {
        if (m.Size.Columns != v.Size.Rows)
            throw new InvalidOperationException("Number of columns in the first matrix must match the number of rows in the second matrix.");

        Vector<T> result = new(v.Size.Rows);

        for (int i = 0; i < m.Size.Rows; i++)
            for (int k = 0; k < m.Size.Columns; k++)
                result[i] += m[i, k] * v[k];

        return result;
    }

    public static Vector<T> operator /(Vector<T> matrix, T scalar) => new((RootClass<T>)matrix / scalar);
    public static Vector<T> operator *(Vector<T> matrix, T scalar) => new((RootClass<T>)matrix * scalar);
    public static Vector<T> operator +(Vector<T> a, Vector<T> b) => new((RootClass<T>)a + (RootClass<T>)b);
    public static Vector<T> operator -(Vector<T> a, Vector<T> b) => new((RootClass<T>)a - (RootClass<T>)b);


    public Matrix<T> AsMatrix() => new((T[,])_matrix.Clone());

    public virtual IEnumerator<T> GetEnumerator()
    {
        for (int i = 0; i < Size.Rows; i++)
            yield return this[i];
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
