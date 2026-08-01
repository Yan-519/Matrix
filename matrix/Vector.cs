using System.Collections;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace matrix;

internal static class VectorBuilder
{
    internal static Vector<T> Create<T>(ReadOnlySpan<T> values) where T : INumber<T> => new(values);
}

[CollectionBuilder(typeof(VectorBuilder), "Create")]
public class Vector<T> : RootClass<T>,
    IMultiplyOperators<Vector<T>, T, Vector<T>>,
    IAdditionOperators<Vector<T>, Vector<T>, Vector<T>>,
    ISubtractionOperators<Vector<T>, Vector<T>, Vector<T>>,
    IDivisionOperators<Vector<T>, T, Vector<T>>,
    IEnumerable<T> where T : INumber<T>
{
    public new T[] source => GetColumn(0);
    public new int size => base.size.Rows;

    public Vector(int size) : base(size, 1) { }

    //public Vector(ObjectSize size) : base(size)
    //{
    //    if (size.Columns != 1)
    //        throw new ArgumentException("A vector must have exactly one column.", nameof(size));
    //}

    public Vector(params T[] data) : base(data.Length, 1)
    {
        for (int i = 0; i < data.Length; i++)
            this[i] = data[i];
    }

    public Vector(ReadOnlySpan<T> data) : this(data.ToArray()) { }

    internal Vector(RootClass<T> root) : base(root.source) { }

    public T this[int r]
    {
        get => matrix[r, 0];
        set => matrix[r, 0] = value;
    }

    public static Vector<T> operator *(Vector<T> v, Matrix<T> m) => new(RootClassExtensions.Multiply<T, T, T>(v, m));

    public static Vector<T> operator *(Matrix<T> m, Vector<T> v) => new(RootClassExtensions.Multiply<T, T, T>(v, m));

    public static Vector<T> operator /(Vector<T> matrix, T scalar) => new((RootClass<T>)matrix / scalar);
    public static Vector<T> operator *(Vector<T> matrix, T scalar) => new((RootClass<T>)matrix * scalar);
    public static Vector<T> operator +(Vector<T> a, Vector<T> b) => new(RootClassExtensions.Add(a, b));
    public static Vector<T> operator -(Vector<T> a, Vector<T> b) => new(RootClassExtensions.Subtract(a, b));

    public override string ToString() => ToString("\t", "\n", string.Empty);

    public Matrix<T> FromOneD(ObjectSize size)
    {
        Matrix<T> matrix = new(size);
        for (int i = 0; i < size.Rows; i++)
            for (int j = 0; j < size.Columns; j++)
                matrix[i, j] = this[i * size.Columns + j];

        return matrix;
    }

    public static List<Vector<T>> Base(Vector<T>[] vectors) => BaseOfBase(vectors);

    public virtual IEnumerator<T> GetEnumerator()
    {
        for (int i = 0; i < size; i++)
            yield return this[i];
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

public static class VectorExtensions
{
    public static Vector<T> Reverse<T>(this Vector<T> v) where T : INumber<T>
    {
        Vector<T> result = new(v.size);
        for (int i = 0; i < v.size; i++)
            result[i] = v[v.size - 1 - i];
        return result;
    }

    public static T Norm<T>(this Vector<T> v) where T : INumber<T>
    {
        T sum = T.Zero;

        for (int i = 0; i < v.size; i++)
            sum += v[i] * v[i];

        return T.CreateChecked(Math.Sqrt(double.CreateChecked(sum)));
    }

    public static Vector<T> ToVectorOfOne<T>(this Vector<T> v) where T : INumber<T>
    {
        T divideBy = v.Norm();
        return new(v.ForEach(x => x / divideBy));
    }
}
