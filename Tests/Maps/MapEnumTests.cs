using System;
using System.Diagnostics;
using NUnit.Framework;
using Transmute.Maps;
using Transmute.Tests.Types;

namespace Transmute.Tests.Maps
{
    [TestFixture]
    public class MapEnumTests : TypeMapTestBase<MapEnum<object>, object>
    {
        [Test]
        public override void CanMapFrom_AcceptedTypes()
        {
            Assert.IsTrue(Map.CanMap(typeof(EnumSrc), typeof(EnumDest)));
            Assert.IsTrue(Map.CanMap(typeof(EnumDest), typeof(EnumSrc)));
            Assert.IsTrue(Map.CanMap(typeof(EnumDest), typeof(EnumDest)));
            Assert.IsTrue(Map.CanMap(typeof(EnumSrc), typeof(EnumSrc)));
        }

        [Test]
        public override void CanMapFrom_RejectedTypes()
        {
            Assert.IsFalse(Map.CanMap(typeof(EnumSrc), typeof(EnumDestMinusOne)));
            Assert.IsFalse(Map.CanMap(typeof(EnumSrc), typeof(EnumDestPlusOne)));
            Assert.IsFalse(Map.CanMap(typeof(string), typeof(int)));
        }

        [Test]
        public override void Map_NullDestination()
        {
            Assert.AreEqual(EnumDest.Value2, InvokeMapper<EnumSrc, EnumDest>(EnumSrc.Value2, null));
        }

        [Test]
        public void Map_EnumSrc_To_EnumDest_Value1()
        {
            Assert.AreEqual(EnumDest.Value1, InvokeMapper(EnumSrc.Value1, EnumDest.Value2));
        }

        [Test]
        public void Map_EnumSrc_To_EnumDest_Value2()
        {
            Assert.AreEqual(EnumDest.Value2, InvokeMapper(EnumSrc.Value2, EnumDest.Value1));
        }

        [Test, Explicit("Basic benchmark for ensuring Enum.Parse is an acceptable speed")]
        public void Benchmark_EnumParse()
        {
            const int loops = 1000000;
            var timer = new Stopwatch();
            timer.Start();
            for(int i = 0; i < loops; i++)
            {
                Enum.Parse(typeof (EnumSrc), "Value1");
            }
            Console.Out.WriteLine(timer.ElapsedMilliseconds);
            timer.Restart();
            for(int i = 0; i < loops; i++)
            {
                Convert(EnumDest.Value1);
            }
            Console.Out.WriteLine(timer.ElapsedMilliseconds);
            timer.Stop();
        }

        [Test, Explicit("Basic benchmark for ensuring CanMap is an acceptable speed")]
        public void Benchmark_CanMap()
        {
            const int loops = 1000000;
            var timer = new Stopwatch();
            timer.Start();
            for (int i = 0; i < loops; i++)
            {
                Map.CanMap(typeof(EnumSrc), typeof(EnumDest));
            }
            Console.Out.WriteLine(timer.ElapsedMilliseconds);
            timer.Stop();
        }

        private static EnumSrc Convert(EnumDest value)
        {
            switch (value)
            {
                case EnumDest.Value2:
                    return EnumSrc.Value2;
                case EnumDest.Value1:
                    return EnumSrc.Value1;
                default:
                    throw new ArgumentOutOfRangeException("value");
            }
        }
    }
        
        public static class DiagnosticTimerExtension
        {
                public static void Restart(this Stopwatch stopwatch)
                {
                        stopwatch.Stop();
                        stopwatch.Reset();
                        stopwatch.Start();
                }
        }
}