using System;
using Transmute.Tests;
using Transmute.Internal.Utils;

namespace Transmute.Benchmark
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            var testFixture = new Benchmarks();
            double totalMs;

            var initialised = false;
            int total = 0;
            for(int i = 0; i < args.Length; i++)
            {
                if(int.TryParse(args[i], out total))
                {
                   initialised = true;
                   break;
                }
            }
            if(!initialised)
                total = 10;

            if(args.Length == 0)
            {
                Console.Out.WriteLine("Native mapper conversion");
                testFixture.SetUp();
                totalMs = testFixture.BenchmarkNative();
                Console.Out.WriteLine("Total elapsed time: {0}ms  Total conversions: {1}  Conversions: {2}/s".With(
                    totalMs, Benchmarks.Total, 100 * Benchmarks.Total / totalMs));
            }

            if(args.Length == 0 || args.Any(a => string.Equals(a, "delegate", StringComparison.CurrentCultureIgnoreCase)))
            {
                Console.Out.WriteLine("Transmute mapper conversion - Delegate");
                for(int i = 0; i < total; i++)
                {
                    testFixture.SetUp();
                    totalMs = testFixture.BenchmarkTransmute(MapBuilder.Delegate);
                    Console.Out.WriteLine("Total elapsed time: {0}ms  Total conversions: {1}  Conversions: {2}/s".With(totalMs, Benchmarks.Total, 100 * Benchmarks.Total / totalMs));
                }
            }

            if(args.Length == 0 || args.Any(a => string.Equals(a, "emit", StringComparison.CurrentCultureIgnoreCase)))
            {
                Console.Out.WriteLine("Transmute mapper conversion - Emit");
                for(int i = 0; i < total; i++)
                {
                    testFixture.SetUp();
                    totalMs = testFixture.BenchmarkTransmute(MapBuilder.Emit);
                    Console.Out.WriteLine("Total elapsed time: {0}ms  Total conversions: {1}  Conversions: {2}/s".With(totalMs, Benchmarks.Total, 100 * Benchmarks.Total / totalMs));
                }
            }
        }
    }

    public static class ArrayExtensions
    {
        public static bool Any(this string[] array, Func<string, bool> method)
        {
            for(int i = 0; i < array.Length; i++)
            {
                if(method(array[i]))
                    return true;
            }
            return false;
        }
    }
}

