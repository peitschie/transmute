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
            testFixture.SetUp();
            var totalMs = testFixture.BenchmarkNative();
            Console.Out.WriteLine("Native mapper conversion -  Total elapsed time: {0}ms  Total conversions: {1}  Conversions: {2}/s".With(
                totalMs, Benchmarks.Total, 100 * Benchmarks.Total / totalMs));

            for(int i = 0; i < 20; i++)
            {
                testFixture.SetUp();
                totalMs = testFixture.BenchmarkTransmute();
                Console.Out.WriteLine("Transmute mapper conversion -  Total elapsed time: {0}ms  Total conversions: {1}  Conversions: {2}/s".With(
                    totalMs, Benchmarks.Total, 100 * Benchmarks.Total / totalMs));
            }
        }
    }
}

