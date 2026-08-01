using System.Numerics;
namespace matrix
{
    public class VectorSpace<T, V> where V : INumber<V> where T : RootClass<V>
    {
        public Func<T, T, V> multiplication { init; get; }
        public Func<T, V> norm { init; get; }


        public VectorSpace(Func<T, T, V> multiplication)
        {
            this.multiplication = multiplication;
            // double conversion
            norm = x => V.CreateChecked(Math.Sqrt(double.CreateChecked(multiplication(x, x))));
        }

        public static VectorSpace<TVector, V> CreateVectorSpace<TVector>(Matrix<V> operation) where TVector : Vector<V>
        {
            return new VectorSpace<TVector, V>((a, b) => (a.Transpose() * operation * b)[0, 0]);
        }

        public static VectorSpace<TVector, V> CreateVectorSpace<TVector>() where TVector : Vector<V>
        {
            return new VectorSpace<TVector, V>((a, b) => (a.Transpose() * Matrix<V>.Identity(a.size) * b)[0, 0]);
        }
    }
}
