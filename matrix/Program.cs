using System.Numerics;
using System.Diagnostics;

namespace matrix;

internal class Program
{
    private static BigInteger fib(int n)
    {
        if (n <= 0)
            throw new("n must be greater or equal to 0");

        Matrix<BigInteger> f = new([0, 1],
                                   [1, 1]);

        Vector<BigInteger> start = new(0, 1);

        return ((f ^ (n - 1)) * start)[0];
    }

    private static BigInteger standart_fib(int n)
    {
        if (n <= 0)
            throw new("n must be greater or equal to 0");
        else if (n == 1)
            return 0;
        else if (n == 2)
            return 1;

        BigInteger a = 0;
        BigInteger b = 1;
        for (int i = 2; i < n; i++)
            (a, b) = (b, a + b);

        return b;
    }

    private static BigInteger[] fib_lst(int n)
    {
        if (n <= 0)
            throw new("n must be greater than 0");

        else if (n == 1)
            return [0];
        else if (n == 2)
            return [0, 1];

        BigInteger[] res = new BigInteger[n];

        Matrix<BigInteger> f = new([0, 1],
                                   [1, 1]);

        Vector<BigInteger> start = new(0, 1);

        for (int i = 0; i < n - 2; i++)
        {
            res[i] = start[0];
            start = f * start;
        }
        res[n - 2] = start[0];
        res[n - 1] = start[1];

        return res;
    }


    private static string test_func(string name, Func<int, BigInteger> func, int n)
    {
        Stopwatch sw = Stopwatch.StartNew();
        _ = func(n);
        sw.Stop();
        return $"{name}({n}), Time: {sw.ElapsedMilliseconds} ms";
    }


    static void Main()
    {
        Vector<double>[] vectors = [[1, 2, 3], [4, 5, 6], [7, 8, 9]];

        foreach(Vector<double> vector in Vector<double>.Base(vectors))
            Console.WriteLine(vector);
    }
}