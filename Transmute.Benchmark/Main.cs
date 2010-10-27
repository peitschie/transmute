using System;
using Transmute.Tests;
using Transmute.Internal.Utils;

namespace Transmute.Benchmark
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            Console.Out.WriteLine("Native mapper conversion");
            var testFixture = new Benchmarks();
            testFixture.SetUp();
            var totalMs = testFixture.BenchmarkNative();
            Console.Out.WriteLine("Total elapsed time: {0}ms  Total conversions: {1}  Conversions: {2}/s".With(
                totalMs, Benchmarks.Total, 100 * Benchmarks.Total / totalMs));

            Console.Out.WriteLine("Transmute mapper conversion - Delegate");
            for(int i = 0; i < 10; i++)
            {
                testFixture.SetUp();
                totalMs = testFixture.BenchmarkTransmute(MapBuilder.Delegate);
                Console.Out.WriteLine("Total elapsed time: {0}ms  Total conversions: {1}  Conversions: {2}/s".With(totalMs, Benchmarks.Total, 100 * Benchmarks.Total / totalMs));
            }

            Console.Out.WriteLine("Transmute mapper conversion - Emit");
            for(int i = 0; i < 10; i++)
            {
                testFixture.SetUp();
                totalMs = testFixture.BenchmarkTransmute(MapBuilder.Emit);
                Console.Out.WriteLine("Total elapsed time: {0}ms  Total conversions: {1}  Conversions: {2}/s".With(totalMs, Benchmarks.Total, 100 * Benchmarks.Total / totalMs));
            }
        }
    }
}

