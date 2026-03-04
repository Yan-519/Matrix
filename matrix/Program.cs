using System.Diagnostics;
using System.Numerics;

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

        return ((f ^ (n - 1)) * start).First();
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

    private static BigInteger[] standart_fib_lst(int n)
    {
        if (n <= 0)
            throw new("n must be greater or equal to 0");
        else if (n == 1)
            return [0];
        else if (n == 2)
            return [0, 1];

        BigInteger[] res = new BigInteger[n];
        res[0] = 0;
        res[1] = 1;

        for (int i = 2; i < n; i++)
            res[i] = res[i - 1] + res[i - 2];

        return res;
    }


    private static Tuple<long, Out> test_func<In, Out>(Func<In, Out> func, In n)
    {
        Stopwatch sw = Stopwatch.StartNew();
        Out o = func(n);
        sw.Stop();
        return new Tuple<long, Out>(sw.ElapsedMilliseconds, o);
    }


    private static void Main()
    {
        //for(int i = 1; i < 7; i++)
        //{
        //    int n = (int)Math.Pow(10, i);

        //    Console.WriteLine($"pow: {i}");
        //    Console.WriteLine(test_func(standart_fib_lst, n).Item1);
        //    Console.WriteLine(test_func(fib_lst, n).Item1);
        //    Console.WriteLine();
        //}

        foreach(BigInteger n in standart_fib_lst(10000))
            Console.WriteLine(n);

        //Vector<double>[] vectors = [[1, 2, 3], [7324, 87153, 813], [287, 7, 0]];

        //foreach(Vector<double> vector in Vector<double>.Base(vectors))
        //    Console.WriteLine(vector);
    }
}