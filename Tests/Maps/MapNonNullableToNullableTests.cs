using System;
using Moq;
using NUnit.Framework;
using Transmute.Maps;

namespace Transmute.Tests.Maps
{
    [TestFixture]
    public class MapNonNullableToNullableTests : TypeMapTestBase<MapNonNullableToNullable<object>, object>
    {
        [Test]
        public override void CanMapFrom_AcceptedTypes()
        {
            Assert.IsTrue(Map.CanMap(typeof(int), typeof(int?)));
            Assert.IsTrue(Map.CanMap(typeof(DateTime), typeof(DateTime?)));

            ResourceMapper.Setup(m => m.CanMap(typeof(int), typeof(long))).Returns(true);
            Assert.IsTrue(Map.CanMap(typeof(int), typeof(long?)));
            ResourceMapper.Verify(m => m.CanMap(typeof(int), typeof(long)));
        }

        [Test]
        public override void CanMapFrom_RejectedTypes()
        {
            Assert.IsFalse(Map.CanMap(typeof(int), typeof(int)));
            Assert.IsFalse(Map.CanMap(typeof(DateTime?), typeof(DateTime)));
            Assert.IsFalse(Map.CanMap(typeof(int), typeof(long?)));
        }

        [Test]
        public override void Map_NullDestination()
        {
            ResourceMapper.Setup(m => m.RequireOneWayMap(typeof(int), typeof(long), It.IsAny<string>()));
            Map.GetMapper(typeof(int), typeof(long?));
            ResourceMapper.Verify(m => m.RequireOneWayMap(typeof(int), typeof(long), It.IsAny<string>()));
        }
    }
}