using System;
using NUnit.Framework;
using Transmute.Maps;
using Transmute.Tests.Types;

namespace Transmute.Tests.Maps
{
    [TestFixture]
    public class MapByValTests : TypeMapTestBase<MapByVal<string>, string>
    {
        [Test]
        public override void CanMapFrom_AcceptedTypes()
        {
            Assert.IsTrue(Map.CanMap(typeof(EnumSrc), typeof(EnumSrc)));
            Assert.IsTrue(Map.CanMap(typeof(DateTime), typeof(DateTime)));
            Assert.IsTrue(Map.CanMap(typeof(int), typeof(int)));
            Assert.IsTrue(Map.CanMap(typeof(string), typeof(string)));
            Assert.IsTrue(Map.CanMap(typeof(bool), typeof(bool)));
            Assert.IsTrue(Map.CanMap(typeof(long), typeof(long)));
        }

        [Test]
        public override void CanMapFrom_RejectedTypes()
        {
            Assert.IsFalse(Map.CanMap(typeof(EnumSrc), typeof(EnumDest)));
            Assert.IsFalse(Map.CanMap(typeof(DomainClassSimple), typeof(DomainClassSimple)));
            Assert.IsFalse(Map.CanMap(typeof(DomainClassSimple), typeof(ResourceClassSimple)));
        }

        [Test]
        public override void Map_NullDestination()
        {
            Assert.AreEqual(10, InvokeMapper<int, int>(10, null));
        }
    }
}