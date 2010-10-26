using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using Transmute.Maps;
using Transmute.Tests.Types;

namespace Transmute.Tests.Maps
{
    [TestFixture]
    public class MapListTests : TypeMapTestBase<MapList<object>, object>
    {
        [Test]
        public override void CanMapFrom_AcceptedTypes()
        {
            Assert.IsTrue(Map.CanMap(typeof(int[]), typeof(IList<string>)));
            Assert.IsTrue(Map.CanMap(typeof(IList<int>), typeof(IEnumerable<string>)));
            Assert.IsTrue(Map.CanMap(typeof(int[]), typeof(List<string>)));
            Assert.IsTrue(Map.CanMap(typeof(IEnumerable<int>), typeof(List<string>)));
            Assert.IsTrue(Map.CanMap(typeof(CustomList), typeof(List<string>)));
        }

        [Test]
        public override void CanMapFrom_RejectedTypes()
        {
            Assert.IsFalse(Map.CanMap(typeof(string), typeof(int)));
            Assert.IsFalse(Map.CanMap(typeof(int[]), typeof(string[])));
            Assert.IsFalse(Map.CanMap(typeof(int[]), typeof(CustomList)));
            Assert.IsFalse(Map.CanMap(typeof(string), typeof(CustomList)));
        }

        [Test]
        public void CanMapFrom_AcceptedType_IfResourceMapperCanMap()
        {
            ResourceMapper.Setup(m => m.CanMap(typeof(DomainClassSimple), typeof(ResourceClassSimple))).Returns(true);
            Assert.IsTrue(Map.CanMap(typeof(IEnumerable<DomainClassSimple>), typeof(List<ResourceClassSimple>)));
            ResourceMapper.Verify(m => m.CanMap(typeof(DomainClassSimple), typeof(ResourceClassSimple)));
        }

        [Test]
        public void CanMapFrom_RejectedType_IfResourceMapperCantMap()
        {
            ResourceMapper.Setup(m => m.CanMap(typeof(DomainClassSimple), typeof(ResourceClassSimple))).Returns(false);
            Assert.IsFalse(Map.CanMap(typeof(IEnumerable<DomainClassSimple>), typeof(List<ResourceClassSimple>)));
            ResourceMapper.Verify(m => m.CanMap(typeof(DomainClassSimple), typeof(ResourceClassSimple)));
        }

        [Test]
        public override void Map_NullDestination()
        {
            InvokeMapper(new List<string>(), (IList<string>)null);
        }

        [Test]
        [Ignore("Broken test")]
        public void Map_NullEntryInFrom()
        {
            ResourceMapper.Setup(c => c.Map(typeof (string), typeof(string), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>()))
                .Returns<Type, Type, string, string, object>((tfrom, tto, from, to, context) => from);
            var result = InvokeMapper(new List<string>(){"1", null, "3"}, (IList<string>)null);
            Assert.AreEqual("1", result[0]);
            Assert.IsNull(result[1]);
            Assert.AreEqual("3", result[2]);
        }

        [Test]
        [Ignore("Broken test")]
        public void Map_CopiesEntriesToNewList()
        {
            ResourceMapper.Setup(c => c.Map(typeof(string), typeof(string), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>()))
                .Returns<Type, Type, string, string, object>((tfrom, tto, from, to, context) => from);
            var result = InvokeMapper(new List<string>() { "1", null, "3" }, new List<string>());
            Assert.AreEqual("1", result[0]);
            Assert.IsNull(result[1]);
            Assert.AreEqual("3", result[2]);
        }

        [Test]
        [Ignore("Broken test")]
        public void Map_ClearsList_ThenCopiesEntriesToNewList()
        {
            ResourceMapper.Setup(c => c.Map(typeof(string), typeof(string), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>()))
                .Returns<Type, Type, string, string, object>((tfrom, tto, from, to, context) => from);
            var result = InvokeMapper(new List<string>() { "1", null, "3" }, new List<string>());
            InvokeMapper(new List<string>() { "4", "5", "6" }, result);
            Assert.AreEqual("4", result[0]);
            Assert.AreEqual("5", result[1]);
            Assert.AreEqual("6", result[2]);
        }
    }
}