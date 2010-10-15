using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using Transmute.Maps;
using Transmute.Tests.Types;

namespace Transmute.Tests.Maps
{
    [TestFixture]
    public class MapArrayTests : TypeMapTestBase<MapArray<object>, object>
    {
        [Test]
        public override void CanMapFrom_AcceptedTypes()
        {
            Assert.IsTrue(Map.CanMap(typeof(int[]), typeof(string[])));
            Assert.IsTrue(Map.CanMap(typeof(IList<int>), typeof(string[])));
            Assert.IsTrue(Map.CanMap(typeof(IEnumerable<int>), typeof(string[])));
            Assert.IsTrue(Map.CanMap(typeof(CustomList), typeof(string[])));
        }

        [Test]
        public override void CanMapFrom_RejectedTypes()
        {
            Assert.IsFalse(Map.CanMap(typeof(string), typeof(int)));
            Assert.IsFalse(Map.CanMap(typeof(int[]), typeof(IList<string>)));
            Assert.IsFalse(Map.CanMap(typeof(int[]), typeof(CustomList)));
            Assert.IsFalse(Map.CanMap(typeof(string), typeof(CustomList)));
            Assert.IsFalse(Map.CanMap(typeof(string), typeof(List<string>)));
        }

        [Test]
        public void CanMapFrom_AcceptedType_IfResourceMapperCanMap()
        {
            ResourceMapper.Setup(m => m.CanMap(typeof(DomainClassSimple), typeof(ResourceClassSimple))).Returns(true);
            Assert.IsTrue(Map.CanMap(typeof(IEnumerable<DomainClassSimple>), typeof(ResourceClassSimple[])));
            ResourceMapper.Verify(m => m.CanMap(typeof(DomainClassSimple), typeof(ResourceClassSimple)));
        }

        [Test]
        public void CanMapFrom_RejectedType_IfResourceMapperCantMap()
        {
            ResourceMapper.Setup(m => m.CanMap(typeof(DomainClassSimple), typeof(ResourceClassSimple))).Returns(false);
            Assert.IsFalse(Map.CanMap(typeof(IEnumerable<DomainClassSimple>), typeof(ResourceClassSimple[])));
            ResourceMapper.Verify(m => m.CanMap(typeof(DomainClassSimple), typeof(ResourceClassSimple)));
        }

        [Test]
        public override void Map_NullDestination()
        {
            InvokeMapper(new List<string>(), (string[])null);
        }

        [Test]
        public void Map_NullEntryInFrom()
        {
            ResourceMapper.Setup(c => c.Map(typeof (string), typeof(string), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>()))
                .Returns<Type, Type, string, string, object>((tfrom, tto, from, to, context) => from);
            var result = InvokeMapper(new List<string>(){"1", null, "3"}, (string[])null);
            Assert.AreEqual("1", result[0]);
            Assert.IsNull(result[1]);
            Assert.AreEqual("3", result[2]);
        }

        [Test]
        public void Map_CopiesEntriesToNewArray()
        {
            ResourceMapper.Setup(c => c.Map(typeof(string), typeof(string), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>()))
                .Returns<Type, Type, string, string, object>((tfrom, tto, from, to, context) => from);
            var result = InvokeMapper(new List<string>() { "1", null, "3" }, new string[0]);
            Assert.AreEqual("1", result[0]);
            Assert.IsNull(result[1]);
            Assert.AreEqual("3", result[2]);
        }

        [Test]
        public void Map_ClearsArray_ThenCopiesEntriesToNewArray()
        {
            ResourceMapper.Setup(c => c.Map(typeof(string), typeof(string), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>()))
                .Returns<Type, Type, string, string, object>((tfrom, tto, from, to, context) => from);
            var result = InvokeMapper(new List<string>() { "1", null, "3" }, new []{"a","b","c"});
            InvokeMapper(new List<string>() { "4", "5", "6" }, result);
            Assert.AreEqual("4", result[0]);
            Assert.AreEqual("5", result[1]);
            Assert.AreEqual("6", result[2]);
        }
    }
}