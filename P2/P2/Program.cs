using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace P2
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                Console.WriteLine("Write:");
                int threads = Convert.ToInt32(Console.ReadLine());
                ThreadPool.SetMaxThreads(threads, threads);
                var watch = Stopwatch.StartNew();
                Parallel.For(0, 100, new ParallelOptions() { MaxDegreeOfParallelism = threads }, d =>
                {
                    Find10000PrimeNumbers();
                });
                
                watch.Stop();
                Console.WriteLine("Time: " + watch.ElapsedMilliseconds + "ms");
            }
        }
        public static void Find10000PrimeNumbers()
        {
            for(int i=1; i<=10000; i++)
            {
                IsPrimeNumber(i);
            }
        }
        public static bool IsPrimeNumber(int n)
        {
            var result = true;
            if (n > 1)
            {
                for (var i = 2u; i < n; i++)
                {
                    if (n % i == 0)
                    {
                        result = false;
                        break;
                    }
                }
            }
            else
            {
                result = false;
            }
            return result;
        }
    }
}
